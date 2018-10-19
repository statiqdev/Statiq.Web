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

        private readonly ConcurrentDictionary<CompilerCacheKey, CompilationResult> _compilationCache = new ConcurrentDictionary<CompilerCacheKey, CompilationResult>();
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IList<MetadataReference> _metadataReferences;
        private readonly string _baseType;

        internal RazorCompiler(CompilationParameters parameters)
        {
            ServiceCollection serviceCollection = new ServiceCollection();

            IMvcCoreBuilder builder = serviceCollection
                .AddMvcCore()
                .AddRazorViewEngine();

            // Get and register MetadataReferences
            _metadataReferences = GetMetadataReferences(parameters.DynamicAssemblies);
            builder.PartManager.FeatureProviders.Add(new MetadataReferenceFeatureProvider(_metadataReferences));

            // Calculate the base page type
            Type basePageType = parameters.BasePageType ?? typeof(WyamRazorPage<>);
            string baseClassName = basePageType.FullName;
            int tickIndex = baseClassName.IndexOf('`');
            if (tickIndex > 0)
            {
                baseClassName = baseClassName.Substring(0, tickIndex);
            }
            _baseType = basePageType.IsGenericTypeDefinition ? $"{baseClassName}<TModel>" : baseClassName;

            // Register the view location expander
            serviceCollection.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new ViewLocationExpander());
            });

            // Register other services
            serviceCollection
                .AddSingleton(parameters.FileSystem)
                .AddSingleton(parameters.Namespaces)
                .AddSingleton(parameters.DynamicAssemblies)
                .AddSingleton<FileSystemFileProvider>()
                .AddSingleton<ILoggerFactory, TraceLoggerFactory>()
                .AddSingleton<DiagnosticSource, SilentDiagnosticSource>()
                .AddSingleton<IHostingEnvironment, HostingEnvironment>()
                .AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>()
                .AddSingleton<IRazorViewEngineFileProviderAccessor, DefaultRazorViewEngineFileProviderAccessor>()
                .AddSingleton<WyamRazorProjectFileSystem>();

            IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
            _serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        }

        private static IList<MetadataReference> GetMetadataReferences(DynamicAssemblyCollection dynamicAssemblies) =>
            AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => !x.IsDynamic && !string.IsNullOrEmpty(x.Location))
                .Select(x => MetadataReference.CreateFromFile(x.Location))
                .Concat((dynamicAssemblies ?? Enumerable.Empty<byte[]>())
                    .Select(x => (MetadataReference)MetadataReference.CreateFromImage(x)))
                .Concat(new MetadataReference[]
                {
                    // Razor/MVC assemblies that might not be loaded yet
                    MetadataReference.CreateFromFile(typeof(IHtmlContent).GetTypeInfo().Assembly.Location)
                })
                .ToList();

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
            // Compile with Razor
            RazorProjectEngine projectEngine = RazorProjectEngine.Create(RazorConfiguration.Default, projectFileSystem, builder =>
            {
                RazorExtensions.Register(builder);

                // We need to register a new document classifier phase because builder.SetBaseType() (which uses builder.ConfigureClass())
                // use the DefaultRazorDocumentClassifierPhase which stops applying document classifier passes after DocumentIntermediateNode.DocumentKind is set
                // (which gets set by the Razor document classifier passes registered in RazorExtensions.Register())
                // Also need to add it just after the DocumentClassifierPhase, otherwise it'll miss the C# lowering phase
                builder.Phases.Insert(
                    builder.Phases.IndexOf(builder.Phases.OfType<IRazorDocumentClassifierPhase>().Last()) + 1,
                    new WyamDocumentPhase(_baseType));
            });
            RazorTemplateEngine templateEngine = new MvcRazorTemplateEngine(projectEngine.Engine, projectFileSystem);
            RazorCodeDocument codeDocument = templateEngine.CreateCodeDocument(projectItem);
            RazorCSharpDocument cSharpDocument = templateEngine.GenerateCode(codeDocument);

            // Compile with Roslyn
            SourceText sourceText = SourceText.From(cSharpDocument.GeneratedCode, Encoding.UTF8);
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(sourceText);
            CSharpCompilationOptions compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithSpecificDiagnosticOptions(
                    new Dictionary<string, ReportDiagnostic>
                    {
                        { "CS1701", ReportDiagnostic.Suppress }, // Binding redirects
                        { "CS1702", ReportDiagnostic.Suppress }, // Disable 1702 until roslyn turns this off by default
                        { "CS1705", ReportDiagnostic.Suppress },
                        { "CS8019", ReportDiagnostic.Suppress }
                    });
            CSharpCompilation compilation = CSharpCompilation.Create("WyamRazor", options: compilationOptions, references: _metadataReferences).AddSyntaxTrees(syntaxTree);

            // Emit assembly
            Assembly assembly;
            using (MemoryStream assemblyStream = new MemoryStream())
            {
                using (MemoryStream pdbStream = new MemoryStream())
                {
                    EmitResult result = compilation.Emit(
                        assemblyStream,
                        pdbStream,
                        options: new EmitOptions(debugInformationFormat: DebugInformationFormat.PortablePdb));

                    // See https://github.com/aspnet/Mvc/blob/a67d9363e22be8ef63a1a62539991e1da3a6e30e/src/Microsoft.AspNetCore.Mvc.Razor/Internal/CompilationFailedExceptionFactory.cs
                    if (!result.Success)
                    {
                        List<Diagnostic> errorDiagnostics = result.Diagnostics
                            .Where(d => d.IsWarningAsError || d.Severity == DiagnosticSeverity.Error)
                            .ToList();
                        List<CompilationErrorException> compilationErrors = new List<CompilationErrorException>();
                        foreach (Diagnostic diagnostic in errorDiagnostics)
                        {
                            string path = diagnostic.Location == Location.None ? codeDocument.Source.FilePath : diagnostic.Location.GetMappedLineSpan().Path;
                            FileLinePositionSpan mappedLineSpan = diagnostic.Location.SourceTree.GetMappedLineSpan(diagnostic.Location.SourceSpan);
                            string errorMessage = diagnostic.GetMessage();
                            compilationErrors.Add(new CompilationErrorException(path, mappedLineSpan, errorMessage));
                        }
                        throw new CompilationErrorsException(compilationErrors);
                    }

                    assemblyStream.Seek(0, SeekOrigin.Begin);
                    pdbStream.Seek(0, SeekOrigin.Begin);

                    assembly = Assembly.Load(assemblyStream.ToArray(), pdbStream.ToArray());
                }
            }

            // Get the runtime item
            RazorCompiledItemLoader compiledItemLoader = new RazorCompiledItemLoader();
            RazorCompiledItem compiledItem = compiledItemLoader.LoadItems(assembly).SingleOrDefault();
            return new CompilationResult(compiledItem);
        }
    }
}