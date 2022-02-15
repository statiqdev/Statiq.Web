using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Buildalyzer;
using Buildalyzer.Environment;
using Buildalyzer.Workspaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Statiq.App;
using Statiq.Common;
using Statiq.Core;

namespace Statiq.Web
{
    public class ThemeManager
    {
        private static readonly EmitOptions AssemblyEmitOptions = new EmitOptions(debugInformationFormat: DebugInformationFormat.PortablePdb);

        private readonly List<Assembly> _compiledProjects = new List<Assembly>();

        public PathCollection ThemePaths { get; } = new PathCollection
        {
            "theme"
        };

        internal void AddPathsFromSettings(IReadOnlySettings settings, IFileSystem fileSystem)
        {
            // Add theme paths from settings
            IReadOnlyList<NormalizedPath> settingsThemePaths = settings.GetList<NormalizedPath>(WebKeys.ThemePaths);
            if (settingsThemePaths?.Count > 0)
            {
                ThemePaths.Clear();
                foreach (NormalizedPath settingsThemePath in settingsThemePaths)
                {
                    ThemePaths.Add(settingsThemePath);
                }
            }

            // Iterate in reverse order so we start with the highest priority
            foreach (NormalizedPath themePath in ThemePaths.Reverse())
            {
                // Inserting at 0 preserves the original order since we're iterating in reverse
                // Add theme paths as non-removable so they persist though input path changes
                fileSystem.InputPaths.Insert(0, themePath.Combine("input"), false);
            }
        }

        internal void CompileProjects(
            ISettings settings,
            IServiceCollection serviceCollection,
            IReadOnlyFileSystem fileSystem,
            ClassCatalog classCatalog,
            ILogger logger)
        {
            // Iterate in reverse order so we start with the highest priority
            foreach (NormalizedPath themePath in ThemePaths.Reverse())
            {
                // Get and build any csproj files in the theme directory
                IDirectory themeDirectory = fileSystem.GetRootDirectory(themePath);
                if (themeDirectory.Exists)
                {
                    foreach (IFile projectFile in themeDirectory.GetFiles()
                        .Where(x => x.Path.Extension.Equals(".csproj", StringComparison.OrdinalIgnoreCase)))
                    {
                        AnalyzerManager analyzerManager = new AnalyzerManager();
                        IProjectAnalyzer projectAnalyzer = analyzerManager.GetProject(projectFile.Path.FullPath);
                        Workspace workspace = projectAnalyzer.GetWorkspace();

                        // Being called from a synchronous method so we've got to get the result synchronously here
#pragma warning disable VSTHRD002 // Synchronously waiting on tasks or awaiters may cause deadlocks. Use await or JoinableTaskFactory.Run instead.
                        Compilation compilation = workspace.CurrentSolution.Projects.First().GetCompilationAsync().GetAwaiter().GetResult();
#pragma warning restore VSTHRD002

                        // Emit the assembly and PDB
                        MemoryStream assemblyStream = new MemoryStream();
                        MemoryStream pdbStream = new MemoryStream();
                        EmitResult result = compilation.Emit(
                            assemblyStream, pdbStream, options: AssemblyEmitOptions);
                        foreach (Diagnostic diagnostic in result.Diagnostics.Where(x => !x.IsSuppressed))
                        {
                            LogLevel logLevel = diagnostic.Severity switch
                            {
                                DiagnosticSeverity.Error => LogLevel.Error,
                                DiagnosticSeverity.Warning => LogLevel.Warning,
                                DiagnosticSeverity.Info => LogLevel.Information,
                                _ => LogLevel.Debug
                            };
                            logger.Log(logLevel, diagnostic.ToString());
                        }
                        if (!result.Success)
                        {
                            throw new Exception("Theme compilation failed for " + projectFile.Path.FullPath);
                        }

                        // Save them to disk in a "bin" directory under the project file
                        // which seems like a reasonable guess - we can't save them in temp
                        // because that gets cleared before and after every execution
                        assemblyStream.Seek(0, SeekOrigin.Begin);
                        pdbStream.Seek(0, SeekOrigin.Begin);
                        IFile assemblyFile = fileSystem.GetFile(
                            projectFile.Path.Parent.Combine("bin").Combine(
                                projectFile.Path.FileNameWithoutExtension.AppendExtension(".dll")));
                        using (Stream assemblyFileStream = assemblyFile.OpenWrite())
                        {
                            assemblyStream.CopyTo(assemblyFileStream);
                            assemblyFileStream.SetLength(assemblyStream.Length);
                        }
                        IFile pdbFile = fileSystem.GetFile(
                            projectFile.Path.Parent.Combine("bin").Combine(
                                projectFile.Path.FileNameWithoutExtension.AppendExtension(".pdb")));
                        using (Stream pdbFileStream = pdbFile.OpenWrite())
                        {
                            pdbStream.WriteTo(pdbFileStream);
                            pdbFileStream.SetLength(pdbStream.Length);
                        }

                        // Load the assembly from the file (make sure to use LoadFrom and not LoadFile so it binds later to Razor, etc.)
                        Assembly assembly = Assembly.LoadFrom(assemblyFile.Path.FullPath);
                        _compiledProjects.Add(assembly);
                    }
                }
            }

            // Add the new assemblies and any references to the class catalog
            if (_compiledProjects.Count > 0)
            {
                classCatalog.AddAssemblies(_compiledProjects);
                classCatalog.LogDebugMessagesTo(logger);
            }

            // Run any theme initializers
            foreach (IThemeInitializer initializer in classCatalog.GetInstances<IThemeInitializer>())
            {
                initializer.Initialize(settings, serviceCollection, fileSystem);
            }
        }

        internal void AddSettingsToEngine(IEngine engine)
        {
            // Iterate in reverse order so we start with the highest priority
            foreach (NormalizedPath themePath in ThemePaths.Reverse())
            {
                // Build a configuration for each of the theme paths and manually merge the
                // values into the engine settings since it's already created
                IDirectory themeDirectory = engine.FileSystem.GetRootDirectory(themePath);
                if (themeDirectory.Exists)
                {
                    IConfigurationRoot configuration = new ConfigurationBuilder()
                        .SetBasePath(themeDirectory.Path.FullPath)
                        .AddSettingsFile("themesettings")
                        .AddSettingsFile("settings")
                        .AddSettingsFile("statiq")
                        .Build();
                    foreach (KeyValuePair<string, string> config in configuration.AsEnumerable())
                    {
                        // Since we're iterating highest priority first, the first key will set and lower priority will be ignored
                        // I.e. if the setting already exists, the theme setting will be ignored
                        if (!engine.Settings.ContainsKey(config.Key))
                        {
                            engine.Settings[config.Key] = config.Value;
                        }
                    }
                }
            }
        }

        internal void AddCompiledNamespacesToEngine(IEngine engine)
        {
            foreach (Assembly assembly in _compiledProjects)
            {
                engine.Namespaces.AddRange(
                    engine.ClassCatalog
                        .GetTypesFromAssembly(assembly, true)
                        .Select(x => x.Namespace)
                        .Where(x => !x.IsNullOrWhiteSpace())
                        .Distinct());
            }
        }
    }
}