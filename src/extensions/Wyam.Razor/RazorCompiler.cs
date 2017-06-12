using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Primitives;
using Wyam.Common.Util;

namespace Wyam.Razor
{
    /// <summary>
    /// Holds references to Razor objects based on the compilation parameters. This ensures the compilation cache and other
    /// service objects are persisted from one generation to the next, given the same compilation parameters.
    /// </summary>
    internal class RazorCompiler
    {
        private readonly ConcurrentDictionary<CompilerCacheKey, CompilationResult> _compilationCache = new ConcurrentDictionary<CompilerCacheKey, CompilationResult>();
        private readonly IServiceProvider _serviceProvider;

        // Razor is apparently not thread safe when reusing the same host for multiple threads.
        // We should investigate where to put the lock.
        private readonly object _lock = new object();

        internal RazorCompiler(CompilationParameters parameters)
        {
            ServiceCollection serviceCollection = new ServiceCollection();

            IMvcCoreBuilder builder = serviceCollection
                .AddMvcCore()
                .AddRazorViewEngine();

            builder.PartManager.FeatureProviders.Add(new MetadataReferenceFeatureProvider(parameters.DynamicAssemblies));
            serviceCollection.Configure<RazorViewEngineOptions>(options => { options.ViewLocationExpanders.Add(new ViewLocationExpander()); });

            serviceCollection.AddSingleton(parameters.FileSystem);
            serviceCollection.AddSingleton<ILoggerFactory, TraceLoggerFactory>();
            serviceCollection.AddSingleton<ILoggerFactory, TraceLoggerFactory>();
            serviceCollection.AddSingleton<DiagnosticSource, SilentDiagnosticSource>();
            serviceCollection.AddSingleton<IHostingEnvironment, HostingEnvironment>();
            serviceCollection.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
            serviceCollection.AddSingleton(parameters.Namespaces);
            serviceCollection.AddSingleton(parameters.DynamicAssemblies);
            serviceCollection.AddSingleton<IBasePageTypeProvider>(new BasePageTypeProvider(parameters.BasePageType ?? typeof(WyamRazorPage<>)));
            serviceCollection.AddSingleton<IMvcRazorHost, RazorHost>();

            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        public IServiceProvider ServiceProvider => _serviceProvider;

        public void RenderPage(RenderRequest request)
        {
            IRazorPage page = GetPageFromStream(request);

            // Razor is apparently not thread safe when reusing the same host for multiple threads.
            // We should investigate where to put the lock.
            lock (_lock)
            {
                IView view = GetViewFromStream(request, page);

                using (StreamWriter writer = request.Output.GetWriter())
                {
                    Microsoft.AspNetCore.Mvc.Rendering.ViewContext viewContext = GetViewContext(request, view, writer);
                    viewContext.View.RenderAsync(viewContext).GetAwaiter().GetResult();
                }
            }
        }

        private Microsoft.AspNetCore.Mvc.Rendering.ViewContext GetViewContext(RenderRequest request, IView view, TextWriter output)
        {
            HttpContext httpContext = new DefaultHttpContext
            {
                RequestServices = _serviceProvider
            };

            ActionContext actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

            ViewDataDictionary viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), actionContext.ModelState)
            {
                Model = request.Model
            };

            ITempDataDictionary tempData = new TempDataDictionary(actionContext.HttpContext, _serviceProvider.GetRequiredService<ITempDataProvider>());

            ViewContext viewContext = new ViewContext(
                actionContext,
                view,
                viewData,
                tempData,
                output,
                new HtmlHelperOptions(),
                request.Document,
                request.Context);

            return viewContext;
        }

        /// <summary>
        /// Gets the view for an input document (which is different than the view for a layout, partial, or
        /// other indirect view because it's not necessarily on disk or in the file system).
        /// </summary>
        private IView GetViewFromStream(RenderRequest request, IRazorPage page)
        {
            IEnumerable<string> viewStartLocations = request.ViewStartLocation != null
                ? new[] { request.ViewStartLocation }
                : ViewHierarchyUtility.GetViewStartLocations(request.RelativePath);

            List<IRazorPage> viewStartPages = viewStartLocations
                .Select(_serviceProvider.GetRequiredService<IRazorPageFactoryProvider>().CreateFactory)
                .Where(x => x.Success)
                .Select(x => x.RazorPageFactory())
                .Reverse()
                .ToList();

            if (request.LayoutLocation != null)
            {
                page.Layout = request.LayoutLocation;
            }

            IRazorViewEngine viewEngine = _serviceProvider.GetRequiredService<IRazorViewEngine>();
            IRazorPageActivator pageActivator = _serviceProvider.GetRequiredService<IRazorPageActivator>();
            HtmlEncoder htmlEncoder = _serviceProvider.GetRequiredService<HtmlEncoder>();

            return new RazorView(viewEngine, pageActivator, viewStartPages, page, htmlEncoder);
        }

        /// <summary>
        /// Gets the Razor page for an input document stream. This is roughly modeled on
        /// DefaultRazorPageFactory and CompilerCache. Note that we don't actually bother
        /// with caching the page if it's from a live stream.
        /// </summary>
        private IRazorPage GetPageFromStream(RenderRequest request)
        {
            string relativePath = request.RelativePath;

            if (relativePath.StartsWith("~/", StringComparison.Ordinal))
            {
                // For tilde slash paths, drop the leading ~ to make it work with the underlying IFileProvider.
                relativePath = relativePath.Substring(1);
            }

            // Get the file info by combining the stream content with info found at the document's original location (if any)
            IHostingEnvironment hostingEnvironment = _serviceProvider.GetRequiredService<IHostingEnvironment>();
            IFileInfo fileInfo = new StreamFileInfo(hostingEnvironment.WebRootFileProvider.GetFileInfo(relativePath), request.Input);
            RelativeFileInfo relativeFileInfo = new RelativeFileInfo(fileInfo, relativePath);

            // Compute a hash for the content since pipelines could have changed it from the underlying file
            // We have to pre-compute the hash (I.e., no CryptoStream) since we need to check for a hit before reading/compiling the view
            byte[] hash = MD5.Create().ComputeHash(request.Input);
            request.Input.Position = 0;

            CompilerCacheResult compilerCacheResult = CompilePage(request, hash, relativeFileInfo, relativePath);

            IRazorPage result = compilerCacheResult.PageFactory();

            return result;
        }

        private CompilerCacheResult CompilePage(RenderRequest request, byte[] hash, RelativeFileInfo relativeFileInfo, string relativePath)
        {
            CompilerCacheKey cacheKey = new CompilerCacheKey(request, hash);
            IRazorCompilationService razorCompilationService = _serviceProvider.GetRequiredService<IRazorCompilationService>();
            CompilationResult compilationResult = _compilationCache.GetOrAdd(cacheKey, _ => GetCompilation(relativeFileInfo, razorCompilationService));

            // Create and return the page
            // We're not actually using the ASP.NET cache, but the CompilerCacheResult ctor contains the logic to create the page factory
            CompilerCacheResult compilerCacheResult = new CompilerCacheResult(relativePath, compilationResult, Array.Empty<IChangeToken>());
            return compilerCacheResult;
        }

        private CompilationResult GetCompilation(RelativeFileInfo relativeFileInfo, IRazorCompilationService razorCompilationService)
        {
            CompilationResult compilationResult = razorCompilationService.Compile(relativeFileInfo);
            compilationResult.EnsureSuccessful();
            return compilationResult;
        }
    }
}