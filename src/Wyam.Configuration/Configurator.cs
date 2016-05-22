using System;
using System.IO;
using System.Linq;
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
    public class Configurator : IDisposable
    {
        private readonly ConfigCompilation _compilation = new ConfigCompilation();
        private readonly Engine _engine;
        private readonly Preprocessor _preprocessor;

        private bool _disposed;
        private bool _configured;
        
        public PackageInstaller PackageInstaller { get; }

        public AssemblyLoader AssemblyLoader { get; }

        public TypeFinder TypeFinder { get; }

        public bool OutputScript { get; set; }

        public FilePath OutputScriptPath { get; set; }

        public Configurator(Engine engine)
            : this(engine, new Preprocessor())
        {
        }
        
        public Configurator(Engine engine, Preprocessor preprocessor)
        {
            _engine = engine;
            PackageInstaller = new PackageInstaller(engine.FileSystem, AssemblyLoader);
            AssemblyLoader = new AssemblyLoader(_compilation, engine.FileSystem, engine.Assemblies);
            TypeFinder = new TypeFinder(engine.Namespaces);
            _preprocessor = preprocessor;

            // Add the config namespace and assembly
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

        // Setup is separated from config by a line with only '-' characters
        // If no such line exists, then the entire script is treated as config
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

            // Initialize and evaluate the script
            Initialize();
            Evaluate(parserResult);
        }

        // Initialize the assembly manager (includes searching for module types)
        private void Initialize()
        {
            // Include all Wyam.Common namespaces
            _engine.Namespaces.AddRange(typeof(IModule).Assembly.GetTypes()
                .Where(x => !string.IsNullOrWhiteSpace(x.Namespace))
                .Select(x => x.Namespace));

            // Install packages
            using (Trace.WithIndent().Information("Installing NuGet packages"))
            {
                PackageInstaller.InstallPackages();
            }

            // Scan assemblies
            using (Trace.WithIndent().Information("Loading assemblies"))
            {
                AssemblyLoader.LoadAssemblies();
            }

            // Load types
            using (Trace.WithIndent().Information("Finding types"))
            {
                TypeFinder.FindTypes(AssemblyLoader.DirectAssemblies);
            }
        }

        private void Evaluate(ConfigParserResult parserResult)
        {
            using (Trace.WithIndent().Information("Evaluating configuration script"))
            {
                _compilation.Generate(parserResult.Declarations, parserResult.Body,
                    TypeFinder.GetImplementations<IModule>(), _engine.Namespaces);
                WriteScript(ConfigCompilation.AssemblyName, _compilation.Code);
                _engine.RawAssemblies.Add(_compilation.Compile(_engine.Assemblies));
                _compilation.Invoke(_engine);
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
