using System;
using System.Collections.Generic;
using System.Linq;
using Wyam.Common.IO;
using Wyam.Common.Tracing;
using Wyam.Configuration;
using Wyam.Configuration.Preprocessing;
using Wyam.Core.Execution;

namespace Wyam
{
    internal class EngineManager : IDisposable
    {
        private readonly Settings _settings;
        private bool _disposed;

        public Engine Engine { get; }
        public Configurator Configurator { get; }

        public EngineManager(Preprocessor preprocessor, Settings settings)
        {
            _settings = settings;
            Engine = new Engine();
            Configurator = new Configurator(Engine, preprocessor);
            
            // Set no cache if requested
            if (_settings.NoCache)
            {
                Engine.Settings.UseCache = false;
            }

            // Set folders
            Engine.FileSystem.RootPath = _settings.RootPath;
            if (_settings.InputPaths != null && _settings.InputPaths.Count > 0)
            {
                // Clear existing default paths if new ones are set
                // and reverse the inputs so the last one is first to match the semantics of multiple occurrence single options
                Engine.FileSystem.InputPaths.Clear();
                Engine.FileSystem.InputPaths.AddRange(_settings.InputPaths.Reverse());
            }
            if (_settings.OutputPath != null)
            {
                Engine.FileSystem.OutputPath = _settings.OutputPath;
            }
            if (_settings.NoClean)
            {
                Engine.Settings.CleanOutputPath = false;
            }
            if (_settings.GlobalMetadata != null)
            {
                foreach (KeyValuePair<string, object> item in _settings.GlobalMetadata)
                {
                    Engine.GlobalMetadata.Add(item);
                }
            }

            // Set NuGet settings
            Configurator.PackageInstaller.UpdatePackages = _settings.UpdatePackages;
            Configurator.PackageInstaller.UseLocalPackagesFolder = _settings.UseLocalPackages;
            Configurator.PackageInstaller.UseGlobalPackageSources = _settings.UseGlobalSources;
            if (_settings.PackagesPath != null)
            {
                Configurator.PackageInstaller.PackagesPath = _settings.PackagesPath;
            }

            // Script output
            Configurator.OutputScript = _settings.OutputScript;

            // Application input
            Engine.ApplicationInput = _settings.Stdin;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(EngineManager));
            }
            Configurator.Dispose();
            Engine.Dispose();
            _disposed = true;
        }

        public bool Configure()
        {
            try
            {
                // Make sure the root path exists
                if (!Engine.FileSystem.GetRootDirectory().Exists)
                {
                    throw new InvalidOperationException($"The root path {Engine.FileSystem.RootPath.FullPath} does not exist.");
                }

                // If we have a configuration file use it, otherwise configure with defaults  
                IFile configFile = Engine.FileSystem.GetRootFile(_settings.ConfigFilePath);
                if (configFile.Exists)
                {
                    Trace.Information("Loading configuration from {0}", configFile.Path);
                    Configurator.OutputScriptPath = configFile.Path.ChangeExtension(".generated.cs");
                    Configurator.Configure(configFile.ReadAllText());
                }
                else
                {
                    Trace.Information("Could not find configuration file at {0}", _settings.ConfigFilePath);
                    Configurator.Configure(null);
                }
            }
            catch (Exception ex)
            {
                Trace.Critical("Error while loading configuration: {0}", ex);
                return false;
            }

            return true;
        }

        public bool Execute()
        {
            try
            {
                Engine.Execute();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}
