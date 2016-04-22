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
using Wyam.Common.NuGet;
using Wyam.Common.Tracing;
using Wyam.Core.Configuration.Preprocessing;
using Wyam.Core.NuGet;

namespace Wyam.Core.Configuration
{
    internal class Config : IConfig, IDisposable
    {
        private readonly Preprocessor _preprocessor = new Preprocessor();
        private readonly AssemblyCollection _assemblyCollection = new AssemblyCollection();
        private readonly AssemblyManager _assemblyManager = new AssemblyManager();
        private readonly IEngine _engine;
        private readonly IFileSystem _fileSystem;
        private readonly PackagesCollection _packages;

        private bool _disposed;
        private ConfigCompilation _compilation;
        private string _fileName;
        private bool _outputScripts;

        public bool Configured { get; private set; }

        IAssemblyCollection IConfig.Assemblies => _assemblyCollection;

        IPackagesCollection IConfig.Packages => _packages;

        public IEnumerable<Assembly> Assemblies => _assemblyManager.Assemblies;

        public IEnumerable<string> Namespaces => _assemblyManager.Namespaces;

        public byte[] RawConfigAssembly => _compilation?.RawAssembly;

        public Config(IEngine engine, IFileSystem fileSystem)
        {
            _engine = engine;
            _fileSystem = fileSystem;
            _packages = new PackagesCollection(fileSystem);

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
                throw new ObjectDisposedException(nameof(Config));
            }
        }

        // Setup is separated from config by a line with only '-' characters
        // If no such line exists, then the entire script is treated as config
        public void Configure(string script, bool updatePackages, string fileName, bool outputScripts)
        {
            CheckDisposed();
            if (Configured)
            {
                throw new InvalidOperationException("This engine has already been configured.");
            }
            Configured = true;
            _fileName = fileName;
            _outputScripts = outputScripts;

            // Parse the script (or use an empty result if no script)
            ConfigParserResult parserResult = string.IsNullOrWhiteSpace(script)
                ? new ConfigParserResult()
                : ConfigParser.Parse(script, _preprocessor);

            // TODO: Process preprocessor directives

            Initialize(updatePackages);
            Evaluate(parserResult);
        }

        // Initialize the assembly manager (includes searching for module types)
        private void Initialize(bool updatePackages)
        {
            // Install packages
            using (Trace.WithIndent().Information("Installing NuGet packages"))
            {
                _packages.InstallPackages(updatePackages);
            }

            // Scan assemblies
            using (Trace.WithIndent().Information("Initializing scripting environment"))
            {
                _assemblyManager.Initialize(_assemblyCollection, _packages, _fileSystem);
            }
        }

        private void Evaluate(ConfigParserResult parserResult)
        {
            using (Trace.WithIndent().Information("Evaluating configuration script"))
            {
                _compilation = new ConfigCompilation(parserResult.Declarations, parserResult.Body,
                    _assemblyManager.ModuleTypes, _assemblyManager.Namespaces);
                OutputScript(ConfigCompilation.AssemblyName, _compilation.Code);
                _compilation.Compile(_assemblyManager.Assemblies);
                _compilation.Invoke(_engine);
            }
        }

        private void OutputScript(string assemblyName, string code)
        {
            // Output only if requested
            if (_outputScripts)
            {
                File.WriteAllText(System.IO.Path.Combine(_fileSystem.RootPath.FullPath, $"{(string.IsNullOrWhiteSpace(_fileName) ? string.Empty : _fileName + ".")}{assemblyName}.cs"), code);
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
                if (_assemblyManager.TryGetAssembly(args.Name, out assembly))
                {
                    return assembly;
                }
            }
            return null;
        }
    }
}
