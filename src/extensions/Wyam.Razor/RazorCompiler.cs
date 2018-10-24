using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Xml.Linq;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Mvc.Razor.Extensions;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.Hosting;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.AspNetCore.Routing;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Razor;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
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
        private const string ViewStartFileName = "_ViewStart.cshtml";

        private static readonly MethodInfo CompileAndEmitMethod;
        private static readonly MethodInfo CreateCompilationFailedException;

        private readonly ConcurrentDictionary<CompilerCacheKey, CompilationResult> _compilationCache
            = new ConcurrentDictionary<CompilerCacheKey, CompilationResult>();

        private readonly NamespaceCollection _namespaces;
        private readonly string _baseType;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        static RazorCompiler()
        {
            CompileAndEmitMethod = typeof(RazorViewCompiler).GetMethod(
                "CompileAndEmit",
                BindingFlags.Instance | BindingFlags.NonPublic,
                Type.DefaultBinder,
                new Type[] { typeof(RazorCodeDocument), typeof(string) },
                null);
            Type compilationFailedExceptionFactory = typeof(IRazorPage).Assembly
                .GetType("Microsoft.AspNetCore.Mvc.Razor.Internal.CompilationFailedExceptionFactory");
            CreateCompilationFailedException = compilationFailedExceptionFactory.GetMethod(
                "Create",
                new Type[] { typeof(RazorCodeDocument), typeof(IEnumerable<RazorDiagnostic>) });
        }

        public RazorCompiler(CompilationParameters parameters)
        {
            _namespaces = parameters.Namespaces;

            // Calculate the base page type
            Type basePageType = parameters.BasePageType ?? typeof(WyamRazorPage<>);
            string baseClassName = basePageType.FullName;
            int tickIndex = baseClassName.IndexOf('`');
            if (tickIndex > 0)
            {
                baseClassName = baseClassName.Substring(0, tickIndex);
            }
            _baseType = basePageType.IsGenericTypeDefinition ? $"{baseClassName}<TModel>" : baseClassName;

            // Create the service collection that MVC needs and add default MVC services
            ServiceCollection serviceCollection = new ServiceCollection();

            // Register some of our own types
            serviceCollection
                .AddSingleton(parameters.FileSystem)
                .AddSingleton<FileSystemFileProvider>()
                .AddSingleton<ILoggerFactory, TraceLoggerFactory>()
                .AddSingleton<DiagnosticSource, SilentDiagnosticSource>()
                .AddSingleton<IHostingEnvironment, HostingEnvironment>()
                .AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>()
                .AddSingleton<IRazorViewEngineFileProviderAccessor, DefaultRazorViewEngineFileProviderAccessor>()
                .AddSingleton<WyamRazorProjectFileSystem>()
                .AddSingleton<RazorProjectFileSystem, WyamRazorProjectFileSystem>()
                .AddSingleton<RazorProjectEngine>(x =>
                    RazorProjectEngine.Create(
                        RazorConfiguration.Default,
                        x.GetRequiredService<RazorProjectFileSystem>(),
                        b =>
                        {
                            // See MvcRazorMvcCoreBuilderExtensions.AddRazorViewEngineServices(IServiceCollection)
                            RazorExtensions.Register(b);
                            b.Features.Add(x.GetRequiredService<LazyMetadataReferenceFeature>()); // Lazily calls the MetadataReferenceFeatureProvider
                            b.Features.Add(new CompilationTagHelperFeature());
                            b.Features.Add(new DefaultTagHelperDescriptorProvider());
                            b.Features.Add(new ViewComponentTagHelperDescriptorProvider());

                            // We need to register a new document classifier phase because builder.SetBaseType() (which uses builder.ConfigureClass())
                            // use the DefaultRazorDocumentClassifierPhase which stops applying document classifier passes after DocumentIntermediateNode.DocumentKind is set
                            // (which gets set by the Razor document classifier passes registered in RazorExtensions.Register())
                            // Also need to add it just after the DocumentClassifierPhase, otherwise it'll miss the C# lowering phase
                            b.Phases.Insert(
                                b.Phases.IndexOf(b.Phases.OfType<IRazorDocumentClassifierPhase>().Last()) + 1,
                                new WyamDocumentPhase(_baseType, _namespaces));
                        }));

            // Register the view location expander
            serviceCollection.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new ViewLocationExpander());
            });

            // Add the default services _after_ adding our own
            // (most default registration use .TryAdd...() so they skip already registered types)
            IMvcCoreBuilder builder = serviceCollection
                .AddMvcCore()
                .AddRazorViewEngine();

            // Get and register MetadataReferences
            builder.PartManager.FeatureProviders.Add(
                new MetadataReferenceFeatureProvider(parameters.DynamicAssemblies));

            IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
            _serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        }

        public void ExpireChangeTokens()
        {
            // Use a new scope to get the file provider
            using (IServiceScope scope = _serviceScopeFactory.CreateScope())
            {
                FileSystemFileProvider fileProvider = scope.ServiceProvider.GetService<FileSystemFileProvider>();
                fileProvider.ExpireChangeTokens();
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

        private ViewContext GetViewContext(IServiceProvider serviceProvider, RenderRequest request, IView view, TextWriter output)
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
            WyamRazorProjectFileSystem projectFileSystem = serviceProvider.GetRequiredService<WyamRazorProjectFileSystem>();

            IEnumerable<string> viewStartLocations = request.ViewStartLocation != null
                ? new[] { request.ViewStartLocation }
                : projectFileSystem.FindHierarchicalItems(request.RelativePath, ViewStartFileName).Select(x => x.FilePath);

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
            DiagnosticSource diagnosticSource = serviceProvider.GetRequiredService<DiagnosticSource>();

            return new RazorView(viewEngine, pageActivator, viewStartPages, page, htmlEncoder, diagnosticSource);
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
            WyamRazorProjectFileSystem projectFileSystem = serviceProvider.GetRequiredService<WyamRazorProjectFileSystem>();
            RazorProjectItem projectItem = projectFileSystem.GetItem(relativePath, request.Input);

            // Compute a hash for the content since pipelines could have changed it from the underlying file
            // We have to pre-compute the hash (I.e., no CryptoStream) since we need to check for a hit before reading/compiling the view
            byte[] hash = SHA512.Create().ComputeHash(request.Input);
            request.Input.Position = 0;

            CompilationResult compilationResult = CompilePage(serviceProvider, request, hash, projectItem, projectFileSystem);

            IRazorPage result = compilationResult.GetPage(request.RelativePath);

            return result;
        }

        private CompilationResult CompilePage(IServiceProvider serviceProvider, RenderRequest request, byte[] hash, RazorProjectItem projectItem, RazorProjectFileSystem projectFileSystem)
        {
            CompilerCacheKey cacheKey = new CompilerCacheKey(request, hash);
            return _compilationCache.GetOrAdd(cacheKey, _ => GetCompilation(projectItem, projectFileSystem));
        }

        private CompilationResult GetCompilation(RazorProjectItem projectItem, RazorProjectFileSystem projectFileSystem)
        {
            using (IServiceScope scope = _serviceScopeFactory.CreateScope())
            {
                IServiceProvider serviceProvider = scope.ServiceProvider;

                // See RazorViewCompiler.CompileAndEmit()
                RazorProjectEngine projectEngine = serviceProvider.GetRequiredService<RazorProjectEngine>();
                RazorCodeDocument codeDocument = projectEngine.Process(projectItem);
                RazorCSharpDocument cSharpDocument = codeDocument.GetCSharpDocument();
                if (cSharpDocument.Diagnostics.Count > 0)
                {
                    throw (Exception)CreateCompilationFailedException.Invoke(
                        null,
                        new object[] { codeDocument, cSharpDocument.Diagnostics });
                }

                // Use the RazorViewCompiler to finish compiling the view for consistency with layouts
                IViewCompilerProvider viewCompilerProvider = serviceProvider.GetRequiredService<IViewCompilerProvider>();
                IViewCompiler viewCompiler = viewCompilerProvider.GetCompiler();
                Assembly assembly = (Assembly)CompileAndEmitMethod.Invoke(
                    viewCompiler,
                    new object[] { codeDocument, cSharpDocument.GeneratedCode });

                // Get the runtime item
                RazorCompiledItemLoader compiledItemLoader = new RazorCompiledItemLoader();
                RazorCompiledItem compiledItem = compiledItemLoader.LoadItems(assembly).SingleOrDefault();
                return new CompilationResult(compiledItem);
            }
        }
    }
}