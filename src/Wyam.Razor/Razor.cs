using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.AspNetCore.Routing;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wyam.Common;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Trace = Wyam.Common.Tracing.Trace;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Primitives;
using Wyam.Razor.FileProviders;
using IFileProvider = Microsoft.Extensions.FileProviders.IFileProvider;

namespace Wyam.Razor
{
    /// <summary>
    /// Parses, compiles, and renders Razor templates.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Razor is the templating language used by ASP.NET MVC. This module can parse and compile Razor 
    /// templates and then render them to HTML. While a bit 
    /// outdated, <a href="http://haacked.com/archive/2011/01/06/razor-syntax-quick-reference.aspx/">this guide</a> 
    /// is a good quick reference for the Razor language syntax. This module uses the Razor engine from ASP.NET Core.
    /// </para>
    /// <para>
    /// Whenever possible, the same conventions as the Razor engine in ASP.NET MVC were used. It's 
    /// important to keep in mind however, that this is <em>not</em> ASP.NET MVC. Many features you may 
    /// be used to will not work (like most of the <c>HtmlHelper</c> extensions) and others just don't 
    /// make sense (like the concept of <em>actions</em> and <em>controllers</em>). Also, while property names and 
    /// classes in the two engines have similar names(such as <c>HtmlHelper</c>) they are not the same, 
    /// and code intended to extend the capabilities of Razor in ASP.NET MVC probably won't work. 
    /// That said, a lot of functionality does function the same as it does in ASP.NET MVC.
    /// </para>
    /// </remarks>
    /// <category>Templates</category>
    /// <include file='Documentation.xml' path='/Documentation/Razor/*' />
    public class Razor : IModule
    {
        private readonly Type _basePageType;
        private DocumentConfig _viewStartPath;
        private string _ignorePrefix = "_";

        /// <summary>
        /// Parses Razor templates in each input document and outputs documents with rendered HTML content. 
        /// If <c>basePageType</c> is specified, it will be used as the base type for Razor pages. The new base
        /// type must derive from <see cref="RazorPage"/>.
        /// </summary>
        /// <param name="basePageType">Type of the base Razor page class, or <c>null</c> for the default base class.</param>
        public Razor(Type basePageType = null)
        {
            if (basePageType != null && !typeof(RazorPage).IsAssignableFrom(basePageType))
            {
                throw new ArgumentException($"The Razor base page type must derive from {nameof(RazorPage)}.");
            }
            _basePageType = basePageType;
        }

        /// <summary>
        /// Specifies an alternate ViewStart file to use for all Razor pages processed by this module. If the
        /// provided path is relative, it will be combined with the file system root path to find the view start.
        /// </summary>
        /// <param name="path">The path to the alternate ViewStart file.</param>
        public Razor WithViewStart(FilePath path)
        {
            _viewStartPath = (doc, ctx) => path.IsAbsolute ? path : ctx.FileSystem.GetInputFile(path).Path;
            return this;
        }

        /// <summary>
        /// Specifies an alternate ViewStart file to use for all Razor pages processed by this module. This 
        /// lets you specify a different ViewStart file for each document. For example, you could return a 
        /// ViewStart based on document location or document metadata. Returning <c>null</c> from the 
        /// function reverts back to the default ViewStart search behavior for that document.
        /// </summary>
        /// <param name="path">A delegate that should return the ViewStart path as a <c>FilePath</c>, 
        /// or <c>null</c> for the default ViewStart search behavior.</param>
        public Razor WithViewStart(DocumentConfig path)
        {
            _viewStartPath = path;
            return this;
        }

        /// <summary>
        /// Specifies a file prefix to ignore. If a document has a metadata value for <c>SourceFileName</c> and 
        /// that metadata value starts with the specified prefix, that document will not be processed or 
        /// output by the module. By default, the Razor module ignores all documents prefixed with 
        /// an underscore (_). Specifying <c>null</c> will result in no documents being ignored.
        /// </summary>
        /// <param name="prefix">The file prefix to ignore.</param>
        public Razor IgnorePrefix(string prefix)
        {
            _ignorePrefix = prefix;
            return this;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            // Register all the MVC and Razor services
            // In the future, if DI is implemented for all Wyam, the IExecutionContext would be registered as a service
            // and the IHostingEnviornment would be registered as transient with the execution context provided in ctor
            IServiceCollection serviceCollection = new ServiceCollection();
            IMvcCoreBuilder builder = serviceCollection
                .AddMvcCore()
                .AddRazorViewEngine();
            builder.PartManager.FeatureProviders.Add(new MetadataReferenceFeatureProvider(context));
            serviceCollection
                .AddSingleton<ILoggerFactory, TraceLoggerFactory>()
                .AddSingleton<DiagnosticSource, SilentDiagnosticSource>()
                .AddSingleton<IHostingEnvironment, HostingEnvironment>()
                .AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>()
                .AddSingleton<IExecutionContext>(context)
                .AddSingleton<IBasePageTypeProvider>(new BasePageTypeProvider(_basePageType ?? typeof(RazorPage)))
                .AddScoped<IViewBufferScope, SafeMemoryPoolViewBufferScope>()
                .AddScoped<IMvcRazorHost, RazorHost>();
            IServiceProvider services = serviceCollection.BuildServiceProvider();
            
            // Eliminate input documents that we shouldn't process
            List<IDocument> validInputs = inputs
                .Where(x => _ignorePrefix == null 
                    || !x.ContainsKey(Keys.SourceFileName) 
                    || !x.FilePath(Keys.SourceFileName).FullPath.StartsWith(_ignorePrefix))
                .ToList();

            // Compile and evaluate the pages in parallel
            IRazorViewEngine viewEngine = services.GetRequiredService<IRazorViewEngine>();
            IRazorPageActivator pageActivator = services.GetRequiredService<IRazorPageActivator>();
            HtmlEncoder htmlEncoder = services.GetRequiredService<HtmlEncoder>();
            IRazorPageFactoryProvider pageFactoryProvider = services.GetRequiredService<IRazorPageFactoryProvider>();
            IRazorCompilationService razorCompilationService = services.GetRequiredService<IRazorCompilationService>();
            IHostingEnvironment hostingEnviornment = services.GetRequiredService<IHostingEnvironment>();
            return validInputs.AsParallel().Select(input =>
            {
                Trace.Verbose("Compiling Razor for {0}", input.SourceString());
                string relativePath = GetRelativePath(input, context);
                FilePath viewStartLocationPath = _viewStartPath?.Invoke<FilePath>(input, context);
                string viewStartLocation = viewStartLocationPath != null ? GetRelativePath(viewStartLocationPath, context) : null;
                IView view;
                using (Stream stream = input.GetStream())
                {
                    view = GetViewFromStream(relativePath, stream, viewStartLocation, viewEngine, pageActivator,
                        htmlEncoder, pageFactoryProvider, hostingEnviornment.WebRootFileProvider, razorCompilationService);
                }

                Trace.Verbose("Processing Razor for {0}", input.SourceString());
                using (StringWriter output = new StringWriter())
                {
                    Microsoft.AspNetCore.Mvc.Rendering.ViewContext viewContext = 
                        GetViewContext(services, view, input, context, output);
                    viewContext.View.RenderAsync(viewContext).GetAwaiter().GetResult();
                    return context.GetDocument(input, output.ToString());
                }
            });
        }
        
        private Microsoft.AspNetCore.Mvc.Rendering.ViewContext GetViewContext(
            IServiceProvider services, IView view, 
            IDocument document, IExecutionContext executionContext, TextWriter output)
        {
            HttpContext httpContext = new DefaultHttpContext
            {
                RequestServices = services
            };
            ActionContext actionContext = new ActionContext(
                httpContext, new RouteData(), new ActionDescriptor());
            ViewDataDictionary viewData = new ViewDataDictionary(
                new EmptyModelMetadataProvider(), actionContext.ModelState)
            {
                Model = document
            };
            ITempDataDictionary tempData = new TempDataDictionary(
                actionContext.HttpContext, services.GetRequiredService<ITempDataProvider>());
            ViewContext viewContext = new ViewContext(actionContext, view, viewData, tempData,
                output, new HtmlHelperOptions(), document, executionContext);
            return viewContext;
        }

        /// <summary>
        /// Gets the view for an input document (which is different than the view for a layout, partial, or
        /// other indirect view because it's not necessarily on disk or in the file system).
        /// </summary>
        private IView GetViewFromStream(string relativePath, Stream stream, string viewStartLocation, IRazorViewEngine viewEngine,
            IRazorPageActivator pageActivator, HtmlEncoder htmlEncoder, IRazorPageFactoryProvider pageFactoryProvider,
            IFileProvider rootFileProvider, IRazorCompilationService razorCompilationService)
        {
            IEnumerable<string> viewStartLocations = viewStartLocation != null
                ? new [] { viewStartLocation } 
                : ViewHierarchyUtility.GetViewStartLocations(relativePath);
            List<IRazorPage> viewStartPages = viewStartLocations
                .Select(pageFactoryProvider.CreateFactory)
                .Where(x => x.Success)
                .Select(x => x.RazorPageFactory())
                .Reverse()
                .ToList();
            IRazorPage page = GetPageFromStream(relativePath, stream, rootFileProvider, razorCompilationService);
            return new RazorView(viewEngine, pageActivator, viewStartPages, page, htmlEncoder);
        }

        /// <summary>
        /// Gets the Razor page for an input document stream. This is roughly modeled on
        /// DefaultRazorPageFactory and CompilerCache. Note that we don't actually bother 
        /// with caching the page if it's from a live stream.
        /// </summary>
        private IRazorPage GetPageFromStream(string relativePath, Stream stream,
            IFileProvider rootFileProvider, IRazorCompilationService razorCompilationService)
        {
            if (relativePath.StartsWith("~/", StringComparison.Ordinal))
            {
                // For tilde slash paths, drop the leading ~ to make it work with the underlying IFileProvider.
                relativePath = relativePath.Substring(1);
            }

            // Get the file info by combining the stream content with info found at the document's original location (if any)
            IFileProvider fileProvider = new StreamFileProvider(rootFileProvider, stream);
            IFileInfo fileInfo = fileProvider.GetFileInfo(relativePath);
            RelativeFileInfo relativeFileInfo = new RelativeFileInfo(fileInfo, relativePath);

            // Create the compilation
            CompilationResult compilationResult = razorCompilationService.Compile(relativeFileInfo);
            compilationResult.EnsureSuccessful();

            // Create and return the page
            // We're not actually using the cache, but the CompilerCacheResult ctor contains the logic to create the page factory
            CompilerCacheResult compilerCacheResult = new CompilerCacheResult(relativePath, compilationResult, Array.Empty<IChangeToken>());
            return compilerCacheResult.PageFactory();
        }

        private string GetRelativePath(IDocument document, IExecutionContext context)
        {
            // Use the pre-calculated relative file path if available
            FilePath relativePath = document.FilePath(Keys.RelativeFilePath);
            return relativePath != null ? $"/{relativePath.FullPath}" : GetRelativePath(document.Source, context);
        }

        private string GetRelativePath(FilePath path, IExecutionContext context)
        {
            // Calculate a relative path from the input path(s) (or root) to the provided path
            if (path != null)
            {
                DirectoryPath inputPath = context.FileSystem.GetContainingInputPath(path) ?? new DirectoryPath("/");
                return $"/{inputPath.GetRelativePath(path).FullPath}";
            }

            // If there's no path, give this document a placeholder name
            return $"/{Path.GetRandomFileName()}.cshtml";
        }
    }
}
