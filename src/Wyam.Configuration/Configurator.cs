using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Wyam.Common.Configuration;
using Wyam.Common.IO;
using Wyam.Common.Modules;
using Wyam.Common.Tracing;
using Wyam.Configuration.Assemblies;
using Wyam.Configuration.ConfigScript;
using Wyam.Configuration.NuGet;
using Wyam.Configuration.Preprocessing;
using Wyam.Core.Execution;

namespace Wyam.Configuration
{
    /// <summary>
    /// Manages configuration of an engine and coordinates configuration script processing.
    /// </summary>
    public class Configurator : IDisposable
    {
        private static readonly Dictionary<string, string> KnownRecipes
            = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"Blog", "Wyam.Blog"}
            };

        // Item1: Path to insert into input paths, Item2: List of NuGet packages needed for this theme
        // If the theme is just a NuGet content package, the content folder will be automatically included and Item1 can be null
        // If the theme uses a non-core file provider for the provided path, the NuGet package(s) containing the provider should be in Item2
        private static readonly Dictionary<string, Tuple<string, string[]>> KnownThemes
            = new Dictionary<string, Tuple<string, string[]>>(StringComparer.OrdinalIgnoreCase)
            {
                {"CleanBlog", Tuple.Create((string) null, new[] {"Wyam.Blog.CleanBlog"})}
            };

        private readonly ConfigCompilation _compilation = new ConfigCompilation();
        private readonly Engine _engine;
        private readonly Preprocessor _preprocessor;

        private bool _disposed;
        private bool _configured;

        public PackageInstaller PackageInstaller { get; }

        public AssemblyLoader AssemblyLoader { get; }

        public ClassCatalog ClassCatalog { get; }

        public bool OutputScript { get; set; }

        public FilePath OutputScriptPath { get; set; }

        public string Recipe { get; set; }

        public string Theme { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Configurator"/> class.
        /// </summary>
        /// <param name="engine">The engine to configure.</param>
        public Configurator(Engine engine)
            : this(engine, new Preprocessor())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Configurator"/> class. This overload
        /// allows passing in a <see cref="Preprocessor"/> that can be reused and pre-configured
        /// with directives not sourced from the script.
        /// </summary>
        /// <param name="engine">The engine to configure.</param>
        /// <param name="preprocessor">The preprocessor.</param>
        public Configurator(Engine engine, Preprocessor preprocessor)
        {
            _engine = engine;
            AssemblyLoader = new AssemblyLoader(_compilation, engine.FileSystem, engine.Assemblies);
            PackageInstaller = new PackageInstaller(engine.FileSystem, AssemblyLoader);
            ClassCatalog = new ClassCatalog();
            _preprocessor = preprocessor;

            // Add this namespace and assembly
            engine.Namespaces.Add(typeof(ConfigScriptBase).Namespace);
            engine.Assemblies.Add(typeof(ConfigScriptBase).Assembly);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            AssemblyLoader.Dispose();
            _disposed = true;
        }

        private void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(Configurator));
            }
        }

        /// <summary>
        /// Configures the engine using the specified script.
        /// </summary>
        /// <param name="script">The script.</param>
        public void Configure(string script)
        {
            CheckDisposed();
            if (_configured)
            {
                throw new InvalidOperationException("Configuration has already been performed.");
            }
            _configured = true;

            // Parse the script (or use an empty result if no script)
            ConfigParserResult parserResult = string.IsNullOrWhiteSpace(script)
                ? new ConfigParserResult()
                : ConfigParser.Parse(script, _preprocessor);

            // Process preprocessor directives
            _preprocessor.ProcessDirectives(this, parserResult.DirectiveValues);
            
            // Initialize everything (order here is very important)
            AddRecipePackage();
            AddThemePackagesAndPath();
            InstallPackages();
            LoadAssemblies();
            CatalogClasses();
            AddNamespaces();
            AddFileProviders();
            ApplyRecipe();

            // Finally evaluate the script
            Evaluate(parserResult);
        }

        private void AddRecipePackage()
        {
            string recipePackageId;
            if (!string.IsNullOrEmpty(Recipe) && KnownRecipes.TryGetValue(Recipe, out recipePackageId))
            {
                PackageInstaller.AddPackage(recipePackageId, allowPrereleaseVersions: true);
            }
        }

        private void AddThemePackagesAndPath()
        {
            string themePath = Theme;
            Tuple<string, string[]> theme;
            if (!string.IsNullOrEmpty(Theme) && KnownThemes.TryGetValue(Theme, out theme))
            {
                themePath = theme.Item1;

                // Add any packages needed for the theme
                if (theme.Item2 != null)
                {
                    foreach (string themePackageId in theme.Item2)
                    {
                        PackageInstaller.AddPackage(themePackageId, allowPrereleaseVersions: true);
                    }
                }
            }

            // Insert the theme path
            if (!string.IsNullOrEmpty(themePath))
            {
                _engine.FileSystem.InputPaths.Insert(0, new DirectoryPath(themePath));
            }
        }

        private void InstallPackages()
        {
            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            using (Trace.WithIndent().Information("Installing NuGet packages"))
            {
                PackageInstaller.InstallPackages();
                stopwatch.Stop();
                Trace.Information($"NuGet packages installed in {stopwatch.ElapsedMilliseconds} ms");
            }
        }

        private void LoadAssemblies()
        {
            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            using (Trace.WithIndent().Information("Recursively loading assemblies"))
            {
                AssemblyLoader.LoadAssemblies();
                stopwatch.Stop();
                Trace.Information($"Assemblies loaded in {stopwatch.ElapsedMilliseconds} ms");
            }
        }

        private void CatalogClasses()
        {
            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            using (Trace.WithIndent().Information("Cataloging classes"))
            {
                ClassCatalog.CatalogTypes(AssemblyLoader.DirectAssemblies);
                stopwatch.Stop();
                Trace.Information($"Classes cataloged in {stopwatch.ElapsedMilliseconds} ms");
            }
        }

        private void AddNamespaces()
        {
            // Add all Wyam.Common namespaces
            _engine.Namespaces.AddRange(typeof(IModule).Assembly.GetTypes()
                .Where(x => !string.IsNullOrWhiteSpace(x.Namespace))
                .Select(x => x.Namespace)
                .Distinct());

            // Add all module namespaces
            _engine.Namespaces.AddRange(ClassCatalog.GetClasses<IModule>().Select(x => x.Namespace));
        }

        private void AddFileProviders()
        {
            foreach (IFileProvider fileProvider in ClassCatalog.GetInstances<IFileProvider>())
            {
                string scheme = fileProvider.GetType().Name.ToLowerInvariant();
                if (scheme.EndsWith("fileprovider"))
                {
                    scheme = scheme.Substring(0, scheme.Length - 12);
                }
                if (scheme.EndsWith("provider"))
                {
                    scheme = scheme.Substring(0, 8);
                }
                if (!string.IsNullOrEmpty(scheme))
                {
                    _engine.FileSystem.FileProviders.Add(scheme, fileProvider);
                }
            }
        }

        private void ApplyRecipe()
        {
            if (!string.IsNullOrEmpty(Recipe))
            {
                IRecipe recipe = ClassCatalog.GetInstance<IRecipe>(Recipe, true);
                if (recipe == null)
                {
                    throw new Exception($"The recipe \"{Recipe}\" could not be found");
                }
                recipe.Apply(_engine);
            }
        }

        private void Evaluate(ConfigParserResult parserResult)
        {
            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            using (Trace.WithIndent().Information("Evaluating configuration script"))
            {
                _compilation.Generate(parserResult.Declarations, parserResult.Body,
                    ClassCatalog.GetClasses<IModule>(), _engine.Namespaces);
                WriteScript(ConfigCompilation.AssemblyName, _compilation.Code);
                _engine.RawAssemblies.Add(_compilation.Compile(_engine.Assemblies));
                _compilation.Invoke(_engine);
                stopwatch.Stop();
                Trace.Information($"Evaluated configuration script in {stopwatch.ElapsedMilliseconds} ms");
            }
        }

        private void WriteScript(string assemblyName, string code)
        {
            // Output only if requested
            if (OutputScript)
            {
                FilePath outputPath = _engine.FileSystem.RootPath.CombineFile(OutputScriptPath ?? new FilePath(assemblyName + ".cs"));
                _engine.FileSystem.GetFile(outputPath)?.WriteAllText(code);
            }
        }
    }
}
