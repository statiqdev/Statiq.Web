using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Wyam.Common.Configuration;
using Wyam.Common.IO;
using Wyam.Common.Tracing;
using Wyam.Configuration.NuGet;
using Wyam.Configuration.Preprocessing;
using Wyam.Core.Execution;

namespace Wyam.Configuration
{
    public class Configurator : IDisposable
    {
        private readonly Preprocessor _preprocessor = new Preprocessor();
        private readonly AssemblyLoader _assemblyLoader = new AssemblyLoader();
        private readonly Engine _engine;

        private bool _disposed;
        private ConfigCompilation _compilation;
        private string _fileName;
        private bool _outputScripts;
        private bool _configured;

        internal PackageInstaller PackageInstaller { get; private set; }

        public Configurator(Engine engine)
        {
            _engine = engine;
            PackageInstaller = new PackageInstaller(engine.FileSystem);

            // Add the config namespace and assembly
            engine.Namespaces.Add(typeof(ConfigScriptBase).Namespace);
            engine.Assemblies.Add(typeof(ConfigScriptBase).Assembly);

            // Manually resolve included assemblies
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
            AppDomain.CurrentDomain.SetupInformation.PrivateBinPathProbe = string.Empty; // non-null means exclude application base path
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            AppDomain.CurrentDomain.AssemblyResolve -= OnAssemblyResolve;
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
        public void Configure(string script, bool updatePackages, bool outputScripts, string fileName)
        {
            CheckDisposed();
            if (_configured)
            {
                throw new InvalidOperationException("Configuration has already been performed.");
            }
            _configured = true;
            _fileName = fileName;
            _outputScripts = outputScripts;

            // Add the directives at this stage so everything is initialized
            _preprocessor.AddDirectives(this);

            // Parse the script (or use an empty result if no script)
            ConfigParserResult parserResult = string.IsNullOrWhiteSpace(script)
                ? new ConfigParserResult()
                : ConfigParser.Parse(script, _preprocessor);

            // Process preprocessor directives
            _preprocessor.ProcessDirectives(parserResult.DirectiveUses);

            // Initialize and evaluate the script
            Initialize(updatePackages);
            Evaluate(parserResult);
        }

        // Initialize the assembly manager (includes searching for module types)
        private void Initialize(bool updatePackages)
        {
            // Install packages
            using (Trace.WithIndent().Information("Installing NuGet packages"))
            {
                PackageInstaller.InstallPackages(updatePackages);
            }

            // Scan assemblies
            using (Trace.WithIndent().Information("Loading assemblies and scanning for types"))
            {
                _assemblyLoader.LoadAssemblies(PackageInstaller, _engine.FileSystem, _engine.Assemblies, _engine.Namespaces);
            }
        }

        private void Evaluate(ConfigParserResult parserResult)
        {
            using (Trace.WithIndent().Information("Evaluating configuration script"))
            {
                _compilation = new ConfigCompilation(parserResult.Declarations, parserResult.Body,
                    _assemblyLoader.ModuleTypes, _engine.Namespaces);
                OutputScript(ConfigCompilation.AssemblyName, _compilation.Code);
                _engine.RawAssemblies.Add(_compilation.Compile(_engine.Assemblies));
                _compilation.Invoke(_engine);
            }
        }

        private void OutputScript(string assemblyName, string code)
        {
            // Output only if requested
            if (_outputScripts)
            {
                File.WriteAllText(System.IO.Path.Combine(_engine.FileSystem.RootPath.FullPath, 
                    $"{(string.IsNullOrWhiteSpace(_fileName) ? string.Empty : _fileName + ".")}{assemblyName}.cs"), code);
            }
        }

        private Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            // Only start resolving after we've generated the config assembly
            if (_compilation != null)
            {
                if (args.Name == _compilation.AssemblyFullName)
                {
                    return _compilation.Assembly;
                }

                Assembly assembly;
                if (_engine.Assemblies.TryGetAssembly(args.Name, out assembly))
                {
                    return assembly;
                }
            }
            return null;
        }
    }
}
