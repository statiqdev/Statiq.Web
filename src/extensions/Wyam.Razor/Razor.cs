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
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wyam.Common;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Util;
using Trace = Wyam.Common.Tracing.Trace;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Primitives;
using IFileProvider = Microsoft.Extensions.FileProviders.IFileProvider;

namespace Wyam.Razor
{
    /// <summary>
    /// Parses, compiles, and renders Razor templates.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Razor is the template language used by ASP.NET MVC. This module can parse and compile Razor
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
        private readonly ConcurrentDictionary<string, CompilationResult> _compilationCache = new ConcurrentDictionary<string, CompilationResult>();
        private readonly Type _basePageType;
        private DocumentConfig _viewStartPath;
        private DocumentConfig _layoutPath;
        private DocumentConfig _model;
        private string _ignorePrefix = "_";

        /// <summary>
        /// Parses Razor templates in each input document and outputs documents with rendered HTML content.
        /// If <c>basePageType</c> is specified, it will be used as the base type for Razor pages. The new base
        /// type must derive from <see cref="WyamRazorPage{TModel}"/>.
        /// </summary>
        /// <param name="basePageType">Type of the base Razor page class, or <c>null</c> for the default base class.</param>
        public Razor(Type basePageType = null)
        {
            if (basePageType != null && !IsSubclassOfRawGeneric(typeof(WyamRazorPage<>), basePageType))
            {
                throw new ArgumentException($"The Razor base page type must derive from {nameof(WyamRazorPage<object>)}.");
            }
            _basePageType = basePageType;
        }

        // From http://stackoverflow.com/a/457708/807064
        private static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                Type current = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == current)
                {
                    return true;
                }
                toCheck = toCheck.BaseType;
            }
            return false;
        }

        /// <summary>
        /// Specifies an alternate ViewStart file to use for all Razor pages processed by this module.
        /// </summary>
        /// <param name="path">The path to the alternate ViewStart file.</param>
        /// <returns>The current module instance.</returns>
        public Razor WithViewStart(FilePath path)
        {
            _viewStartPath = (doc, ctx) => path;
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
        /// <returns>The current module instance.</returns>
        public Razor WithViewStart(DocumentConfig path)
        {
            _viewStartPath = path;
            return this;
        }

        /// <summary>
        /// Specifies a layout file to use for all Razor pages processed by this module.
        /// </summary>
        /// <param name="path">The path to the layout file.</param>
        /// <returns>The current module instance.</returns>
        public Razor WithLayout(FilePath path)
        {
            _layoutPath = (doc, ctx) => path;
            return this;
        }

        /// <summary>
        /// Specifies a layout file to use for all Razor pages processed by this module. This
        /// lets you specify a different layout file for each document.
        /// </summary>
        /// <param name="path">A delegate that should return the layout path as a <c>FilePath</c>.</param>
        /// <returns>The current module instance.</returns>
        public Razor WithLayout(DocumentConfig path)
        {
            _layoutPath = path;
            return this;
        }

        /// <summary>
        /// Specifies a model to use for each page.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>The current module instance.</returns>
        public Razor WithModel(object model)
        {
            _model = (doc, ctx) => model;
            return this;
        }

        /// <summary>
        /// Specifies a model to use for each page based on the current input
        /// document and context.
        /// </summary>
        /// <param name="model">A delegate that returns the model.</param>
        /// <returns>The current module instance.</returns>
        public Razor WithModel(DocumentConfig model)
        {
            _model = model;
            return this;
        }

        /// <summary>
        /// Specifies a file prefix to ignore. If a document has a metadata value for <c>SourceFileName</c> and
        /// that metadata value starts with the specified prefix, that document will not be processed or
        /// output by the module. By default, the Razor module ignores all documents prefixed with
        /// an underscore (_). Specifying <c>null</c> will result in no documents being ignored.
        /// </summary>
        /// <param name="prefix">The file prefix to ignore.</param>
        /// <returns>The current module instance.</returns>
        public Razor IgnorePrefix(string prefix)
        {
            _ignorePrefix = prefix;
            return this;
        }

        /// <inheritdoc />
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
            serviceCollection.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new ViewLocationExpander());
            });
            serviceCollection
                .AddSingleton<ILoggerFactory, TraceLoggerFactory>()
                .AddSingleton<DiagnosticSource, SilentDiagnosticSource>()
                .AddSingleton<IHostingEnvironment, HostingEnvironment>()
                .AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>()
                .AddSingleton<IExecutionContext>(context)
                .AddSingleton<IBasePageTypeProvider>(new BasePageTypeProvider(_basePageType ?? typeof(WyamRazorPage<>)))
                .AddScoped<IMvcRazorHost, RazorHost>();
            IServiceProvider services = serviceCollection.BuildServiceProvider();

            // Eliminate input documents that we shouldn't process
            List<IDocument> validInputs = inputs
                .Where(context, x => _ignorePrefix == null
                    || !x.ContainsKey(Keys.SourceFileName)
                    || !x.FilePath(Keys.SourceFileName).FullPath.StartsWith(_ignorePrefix))
                .ToList();
            if (validInputs.Count < inputs.Count)
            {
                Trace.Information($"Ignoring {inputs.Count - validInputs.Count} inputs due to source file name prefix");
            }

            // Compile and evaluate the pages in parallel
            IServiceScopeFactory scopeFactory = services.GetRequiredService<IServiceScopeFactory>();
            return validInputs.AsParallel().Select(context, input =>
            {
                Trace.Verbose("Processing Razor for {0}", input.SourceString());
                using (IServiceScope scope = scopeFactory.CreateScope())
                {
                    // Get services
                    IRazorViewEngine viewEngine = scope.ServiceProvider.GetRequiredService<IRazorViewEngine>();
                    IRazorPageActivator pageActivator = scope.ServiceProvider.GetRequiredService<IRazorPageActivator>();
                    HtmlEncoder htmlEncoder = scope.ServiceProvider.GetRequiredService<HtmlEncoder>();
                    IRazorPageFactoryProvider pageFactoryProvider = scope.ServiceProvider.GetRequiredService<IRazorPageFactoryProvider>();
                    IRazorCompilationService razorCompilationService = scope.ServiceProvider.GetRequiredService<IRazorCompilationService>();
                    IHostingEnvironment hostingEnviornment = scope.ServiceProvider.GetRequiredService<IHostingEnvironment>();

                    // Compile the view
                    string relativePath = GetRelativePath(input, context);
                    FilePath viewStartLocationPath = _viewStartPath?.Invoke<FilePath>(input, context);
                    string viewStartLocation = viewStartLocationPath != null ? GetRelativePath(viewStartLocationPath, context) : null;
                    string layoutLocation = _layoutPath?.Invoke<FilePath>(input, context)?.FullPath;
                    IView view;
                    using (Stream stream = input.GetStream())
                    {
                        view = GetViewFromStream(
                            relativePath,
                            stream,
                            viewStartLocation,
                            layoutLocation,
                            viewEngine,
                            pageActivator,
                            htmlEncoder,
                            pageFactoryProvider,
                            hostingEnviornment.WebRootFileProvider,
                            razorCompilationService);
                    }

                    // Render the view
                    object model = _model == null ? input : _model.Invoke(input, context);
                    Stream contentStream = context.GetContentStream();
                    using (StreamWriter writer = contentStream.GetWriter())
                    {
                        Microsoft.AspNetCore.Mvc.Rendering.ViewContext viewContext =
                            GetViewContext(scope.ServiceProvider, view, model, input, context, writer);
                        viewContext.View.RenderAsync(viewContext).GetAwaiter().GetResult();
                        writer.Flush();
                    }
                    return context.GetDocument(input, contentStream);
                }
            });
        }

        private Microsoft.AspNetCore.Mvc.Rendering.ViewContext GetViewContext(
            IServiceProvider services,
            IView view,
            object model,
            IDocument document,
            IExecutionContext executionContext,
            TextWriter output)
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
                Model = model
            };
            ITempDataDictionary tempData = new TempDataDictionary(
                actionContext.HttpContext, services.GetRequiredService<ITempDataProvider>());
            ViewContext viewContext = new ViewContext(
                actionContext,
                view,
                viewData,
                tempData,
                output,
                new HtmlHelperOptions(),
                document,
                executionContext);
            return viewContext;
        }

        /// <summary>
        /// Gets the view for an input document (which is different than the view for a layout, partial, or
        /// other indirect view because it's not necessarily on disk or in the file system).
        /// </summary>
        private IView GetViewFromStream(
            string relativePath,
            Stream stream,
            string viewStartLocation,
            string layoutLocation,
            IRazorViewEngine viewEngine,
            IRazorPageActivator pageActivator,
            HtmlEncoder htmlEncoder,
            IRazorPageFactoryProvider pageFactoryProvider,
            IFileProvider rootFileProvider,
            IRazorCompilationService razorCompilationService)
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
            IRazorPage page = GetPageFromStream(relativePath, viewStartLocation, layoutLocation, stream, rootFileProvider, razorCompilationService);
            if (layoutLocation != null)
            {
                page.Layout = layoutLocation;
            }
            return new RazorView(viewEngine, pageActivator, viewStartPages, page, htmlEncoder);
        }

        /// <summary>
        /// Gets the Razor page for an input document stream. This is roughly modeled on
        /// DefaultRazorPageFactory and CompilerCache. Note that we don't actually bother
        /// with caching the page if it's from a live stream.
        /// </summary>
        private IRazorPage GetPageFromStream(
            string relativePath,
            string viewStartLocation,
            string layoutLocation,
            Stream stream,
            IFileProvider rootFileProvider,
            IRazorCompilationService razorCompilationService)
        {
            if (relativePath.StartsWith("~/", StringComparison.Ordinal))
            {
                // For tilde slash paths, drop the leading ~ to make it work with the underlying IFileProvider.
                relativePath = relativePath.Substring(1);
            }

            // Get the file info by combining the stream content with info found at the document's original location (if any)
            IFileInfo fileInfo = new StreamFileInfo(rootFileProvider.GetFileInfo(relativePath), stream);
            RelativeFileInfo relativeFileInfo = new RelativeFileInfo(fileInfo, relativePath);

            // Try to get the compilation from the cache, but only if the stream is empty
            // Cache key is relative path if no explicit view start or layout OR either/both of those if specified
            CompilationResult compilationResult = stream.Length == 0
                ? _compilationCache.GetOrAdd(
                    viewStartLocation == null
                        ? (layoutLocation ?? relativePath)
                        : (layoutLocation == null ? viewStartLocation : viewStartLocation + layoutLocation),
                    _ => GetCompilation(relativeFileInfo, razorCompilationService))
                : GetCompilation(relativeFileInfo, razorCompilationService);

            // Create and return the page
            // We're not actually using the ASP.NET cache, but the CompilerCacheResult ctor contains the logic to create the page factory
            CompilerCacheResult compilerCacheResult = new CompilerCacheResult(relativePath, compilationResult, Array.Empty<IChangeToken>());
            return compilerCacheResult.PageFactory();
        }

        private CompilationResult GetCompilation(RelativeFileInfo relativeFileInfo, IRazorCompilationService razorCompilationService)
        {
            CompilationResult compilationResult = razorCompilationService.Compile(relativeFileInfo);
            compilationResult.EnsureSuccessful();
            return compilationResult;
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
                if (path.IsRelative)
                {
                    // If the path is relative, combine it with the input path to make it absolute
                    path = inputPath.CombineFile(path);
                }
                return $"/{inputPath.GetRelativePath(path).FullPath}";
            }

            // If there's no path, give this document a placeholder name
            return $"/{Path.GetRandomFileName()}.cshtml";
        }
    }
}
