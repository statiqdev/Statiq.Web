using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Encodings.Web;
using System.Threading;
using System.Xml.Linq;

using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.Repositories;
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
using Microsoft.Extensions.DependencyInjection.Extensions;
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
        private readonly IServiceScopeFactory _serviceScopeFactory;

        internal RazorCompiler(CompilationParameters parameters)
        {
            ServiceCollection serviceCollection = new ServiceCollection();

            IMvcCoreBuilder builder = serviceCollection
                .AddMvcCore()
                .AddRazorViewEngine();

            builder.PartManager.FeatureProviders.Add(new MetadataReferenceFeatureProvider(parameters.DynamicAssemblies));
            serviceCollection.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new ViewLocationExpander());
            });

            // Disables the configuration of the DataProtection services, which we don't need for just razor generation.
            serviceCollection.AddTransient<IXmlRepository, DummyXmlRepository>();

            serviceCollection
                .AddSingleton(parameters.FileSystem)
                .AddSingleton<ILoggerFactory, TraceLoggerFactory>()
                .AddSingleton<ILoggerFactory, TraceLoggerFactory>()
                .AddSingleton<DiagnosticSource, SilentDiagnosticSource>()
                .AddSingleton<IHostingEnvironment, HostingEnvironment>()
                .AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>()
                .AddSingleton(parameters.Namespaces)
                .AddSingleton(parameters.DynamicAssemblies)
                .AddSingleton<IBasePageTypeProvider>(new BasePageTypeProvider(parameters.BasePageType ?? typeof(WyamRazorPage<>)))
                .AddScoped<IMvcRazorHost, RazorHost>();

            IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
            _serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        }

        public void ExpireChangeTokens()
        {
            // Use a new scope to get the hosting enviornment
            using (IServiceScope scope = _serviceScopeFactory.CreateScope())
            {
                HostingEnvironment hostingEnvironment = (HostingEnvironment)scope.ServiceProvider.GetService<IHostingEnvironment>();
                hostingEnvironment.ExpireChangeTokens();
            }
        }

        public void RenderPage(RenderRequest request)
        {
            using (IServiceScope scope = _serviceScopeFactory.CreateScope())
            {
                IServiceProvider serviceProvider = scope.ServiceProvider;
                IRazorPage page = GetPageFromStream(serviceProvider, request);
                IView view = GetViewFromStream(serviceProvider, request, page);

                using (StreamWriter writer = request.Output.GetWriter())
                {
                    Microsoft.AspNetCore.Mvc.Rendering.ViewContext viewContext = GetViewContext(serviceProvider, request, view, writer);
                    viewContext.View.RenderAsync(viewContext).GetAwaiter().GetResult();
                }
            }
        }

        private Microsoft.AspNetCore.Mvc.Rendering.ViewContext GetViewContext(IServiceProvider serviceProvider, RenderRequest request, IView view, TextWriter output)
        {
            HttpContext httpContext = new DefaultHttpContext
            {
                RequestServices = serviceProvider
            };

            ActionContext actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

            ViewDataDictionary viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), actionContext.ModelState)
            {
                Model = request.Model
            };

            ITempDataDictionary tempData = new TempDataDictionary(actionContext.HttpContext, serviceProvider.GetRequiredService<ITempDataProvider>());

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
        private IView GetViewFromStream(IServiceProvider serviceProvider, RenderRequest request, IRazorPage page)
        {
            IEnumerable<string> viewStartLocations = request.ViewStartLocation != null
                ? new[] { request.ViewStartLocation }
                : ViewHierarchyUtility.GetViewStartLocations(request.RelativePath);

            List<IRazorPage> viewStartPages = viewStartLocations
                .Select(serviceProvider.GetRequiredService<IRazorPageFactoryProvider>().CreateFactory)
                .Where(x => x.Success)
                .Select(x => x.RazorPageFactory())
                .Reverse()
                .ToList();

            if (request.LayoutLocation != null)
            {
                page.Layout = request.LayoutLocation;
            }

            IRazorViewEngine viewEngine = serviceProvider.GetRequiredService<IRazorViewEngine>();
            IRazorPageActivator pageActivator = serviceProvider.GetRequiredService<IRazorPageActivator>();
            HtmlEncoder htmlEncoder = serviceProvider.GetRequiredService<HtmlEncoder>();

            return new RazorView(viewEngine, pageActivator, viewStartPages, page, htmlEncoder);
        }

        /// <summary>
        /// Gets the Razor page for an input document stream. This is roughly modeled on
        /// DefaultRazorPageFactory and CompilerCache. Note that we don't actually bother
        /// with caching the page if it's from a live stream.
        /// </summary>
        private IRazorPage GetPageFromStream(IServiceProvider serviceProvider, RenderRequest request)
        {
            string relativePath = request.RelativePath;

            if (relativePath.StartsWith("~/", StringComparison.Ordinal))
            {
                // For tilde slash paths, drop the leading ~ to make it work with the underlying IFileProvider.
                relativePath = relativePath.Substring(1);
            }

            // Get the file info by combining the stream content with info found at the document's original location (if any)
            IHostingEnvironment hostingEnvironment = serviceProvider.GetRequiredService<IHostingEnvironment>();
            IFileInfo fileInfo = new StreamFileInfo(hostingEnvironment.WebRootFileProvider.GetFileInfo(relativePath), request.Input);
            RelativeFileInfo relativeFileInfo = new RelativeFileInfo(fileInfo, relativePath);

            // Compute a hash for the content since pipelines could have changed it from the underlying file
            // We have to pre-compute the hash (I.e., no CryptoStream) since we need to check for a hit before reading/compiling the view
            byte[] hash = SHA512.Create().ComputeHash(request.Input);
            request.Input.Position = 0;

            CompilerCacheResult compilerCacheResult = CompilePage(serviceProvider, request, hash, relativeFileInfo, relativePath);

            IRazorPage result = compilerCacheResult.PageFactory();

            return result;
        }

        private CompilerCacheResult CompilePage(IServiceProvider serviceProvider, RenderRequest request, byte[] hash, RelativeFileInfo relativeFileInfo, string relativePath)
        {
            CompilerCacheKey cacheKey = new CompilerCacheKey(request, hash);
            IRazorCompilationService razorCompilationService = serviceProvider.GetRequiredService<IRazorCompilationService>();
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

    internal class DummyXmlRepository : IXmlRepository
    {
        public IReadOnlyCollection<XElement> GetAllElements()
        {
            return new List<XElement>();
        }

        public void StoreElement(XElement element, string friendlyName)
        {
        }
    }
}