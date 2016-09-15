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
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
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

// TODO: How to handle caching - should we continue to let Razor handle it, or stick it in our own cache - what about in between runs?

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
    /// is a good quick reference for the Razor language syntax.
    /// </para>
    /// <para>
    /// This module is based on the Razor code in the forthcoming ASP.NET 5 (vNext). 
    /// It was written from the ground-up and doesn't use an intermediate library like RazorEngine 
    /// (which is a great library, implementing directly just provides more control). Note that for 
    /// now, TagHelpers are not implemented in Wyam. Their API is still changing and it would have 
    /// been too difficult to keep up. Support for TagHelpers may be introduced 
    /// once ASP.NET MVC 5 becomes more stable.
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
        // TODO: Figure out how to handle these settings (and test!)
        private readonly Type _basePageType;
        private DocumentConfig _viewStartPath;
        private string _ignorePrefix = "_";

        /// <summary>
        /// Parses Razor templates in each input document and outputs documents with rendered HTML content. 
        /// If <c>basePageType</c> is specified, it will be used as the base type for Razor pages.
        /// </summary>
        /// <param name="basePageType">Type of the base Razor page class, or <c>null</c> for the default base class.</param>
        public Razor(Type basePageType = null)
        {
            //if (basePageType != null && !typeof(BaseRazorPage).IsAssignableFrom(basePageType))
            //{
            //    throw new ArgumentException("The Razor base page type must derive from BaseRazorPage.");
            //}
            _basePageType = basePageType;
        }

        /// <summary>
        /// Specifies an alternate ViewStart file to use for all Razor pages processed by this module.
        /// </summary>
        /// <param name="path">The path to the alternate ViewStart file.</param>
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
                .AddSingleton<IRazorViewEngine, RazorViewEngine>()
                .AddSingleton<IExecutionContext>(context)
                .AddScoped<IMvcRazorHost, RazorHost>()
                .Configure<RazorViewEngineOptions>(options =>
                {
                    // TODO: Add my file provider(s) - maybe not needed if custom RazorViewEngine
                    //options.FileProviders
                });
            IServiceProvider services = serviceCollection.BuildServiceProvider();
            
            // Eliminate input documents that we shouldn't process
            List<IDocument> validInputs = inputs
                .Where(x => _ignorePrefix == null 
                    || !x.ContainsKey(Keys.SourceFileName) 
                    || !x.FilePath(Keys.SourceFileName).FullPath.StartsWith(_ignorePrefix))
                .ToList();

            // Compile and evaluate the pages in parallel
            return validInputs.AsParallel().Select(input =>
            {
                Trace.Verbose("Compiling Razor for {0}", input.SourceString());
                IRazorViewEngine viewEngine = services.GetService<IRazorViewEngine>();
                ViewEngineResult viewEngineResult = viewEngine
                    .GetView(context.FileSystem.RootPath.ToString(), GetRelativePath(input), true)
                    .EnsureSuccessful(null);

                Trace.Verbose("Processing Razor for {0}", input.SourceString());
                using (StringWriter output = new StringWriter())
                {
                    Microsoft.AspNetCore.Mvc.Rendering.ViewContext viewContext = GetViewContext(
                        services, viewEngineResult, input, context, output);
                    viewContext.View.RenderAsync(viewContext).GetAwaiter().GetResult();
                    return context.GetDocument(input, output.ToString());
                }
            });
        }
        
        private Microsoft.AspNetCore.Mvc.Rendering.ViewContext GetViewContext(
            IServiceProvider services, ViewEngineResult viewEngineResult, 
            IDocument document, IExecutionContext executionContext, TextWriter output)
        {
            HttpContext httpContext = new DefaultHttpContext()
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
            ViewContext viewContext = new ViewContext(
                actionContext, viewEngineResult.View, viewData, tempData, output,
                new HtmlHelperOptions(), document, executionContext);
            return viewContext;
        }

        private string GetRelativePath(IDocument document)
        {
            string relativePath = "/";
            if (document.ContainsKey(Keys.RelativeFilePath))
            {
                relativePath += document.String(Keys.RelativeFilePath);
            }
            return relativePath;
        }
    }

    internal class RazorViewEngine : IRazorViewEngine
    {
        private const string ViewExtension = ".cshtml";

        private static readonly IEnumerable<string> _viewLocationFormats = new[]
        {
            "/{0}",
            "/Shared/{0}",
            "/Views/{0}",
            "/Views/Shared/{0}"
        };

        private readonly IRazorPageFactoryProvider _pageFactory;
        private readonly IRazorPageActivator _pageActivator;
        private readonly HtmlEncoder _htmlEncoder;

        public RazorViewEngine(IRazorPageFactoryProvider pageFactory, 
            IRazorPageActivator pageActivator, HtmlEncoder htmlEncoder)
        {
            _pageFactory = pageFactory;
            _pageActivator = pageActivator;
            _htmlEncoder = htmlEncoder;
        }

        public ViewEngineResult FindView(ActionContext context, string viewName, bool isMainPage)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (string.IsNullOrEmpty(viewName))
            {
                throw new ArgumentException(nameof(viewName));
            }

            throw new NotImplementedException();
        }

        public ViewEngineResult GetView(string executingFilePath, string viewPath, bool isMainPage)
        {
            throw new NotImplementedException();
        }

        public RazorPageResult FindPage(ActionContext context, string pageName)
        {
            throw new NotImplementedException();
        }

        public RazorPageResult GetPage(string executingFilePath, string pagePath)
        {
            throw new NotImplementedException();
        }

        public string GetAbsolutePath(string executingFilePath, string pagePath)
        {
            throw new NotImplementedException();
        }
    }
}
