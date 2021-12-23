using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Buildalyzer;
using Buildalyzer.Workspaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Statiq.App;
using Statiq.Common;
using Statiq.Core;
using Statiq.Web.Theme;

namespace Statiq.Web
{
    public class ThemeManager
    {
        private readonly ClassCatalog _classCatalog;
        private readonly ILogger<ThemeManager> _logger;

        public ThemeManager(ClassCatalog classCatalog, ILogger<ThemeManager> logger)
        {
            _classCatalog = classCatalog;
            _logger = logger;
        }

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
                fileSystem.InputPaths.Insert(0, themePath.Combine("input"));
            }
        }

        internal void CompileProjects(ISettings settings, IServiceCollection serviceCollection, IReadOnlyFileSystem fileSystem)
        {
            // Iterate in reverse order so we start with the highest priority
            List<Assembly> assemblies = new List<Assembly>();
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
                        Compilation compilation = workspace.CurrentSolution.Projects.First().GetCompilationAsync().Result;
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            EmitResult result = compilation.Emit(memoryStream);
                            ScriptHelper.LogAndEnsureCompilationSuccess(result, _logger, projectFile.Path.Name);
                            memoryStream.Seek(0, SeekOrigin.Begin);
                            byte[] rawAssembly = memoryStream.ToArray();
                            assemblies.Add(Assembly.Load(rawAssembly));
                        }
                    }
                }
            }

            // Add the new assemblies and any references to the class catalog
            if (assemblies.Count > 0)
            {
                _classCatalog.AddAssemblies(assemblies);
                _classCatalog.LogDebugMessagesTo(_logger);
            }

            // Run any theme initializers
            foreach (IThemeInitializer initializer in _classCatalog.GetInstances<IThemeInitializer>())
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
    }
}