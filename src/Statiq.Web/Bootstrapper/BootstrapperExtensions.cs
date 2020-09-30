using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Statiq.App;
using Statiq.Common;
using Statiq.Core;
using Statiq.Web.Commands;

namespace Statiq.Web
{
    public static class BootstrapperExtensions
    {
        /// <summary>
        /// Adds Statiq Web functionality to an existing bootstrapper.
        /// This method does not need to be called if using <see cref="BootstrapperFactoryExtensions.CreateWeb(BootstrapperFactory, string[])"/>.
        /// </summary>
        /// <remarks>
        /// This method is useful when you want to add Statiq Web support to an existing bootstrapper,
        /// for example because you created the bootstrapper without certain default functionality
        /// by calling <see cref="Statiq.App.BootstrapperFactoryExtensions.CreateDefaultWithout(BootstrapperFactory, string[], DefaultFeatures)"/>.
        /// </remarks>
        /// <param name="boostrapper">The bootstrapper to add Statiq Web functionality to.</param>
        /// <returns>The bootstrapper.</returns>
        public static Bootstrapper AddWeb(this Bootstrapper boostrapper) =>
            boostrapper
                .AddPipelines(typeof(BootstrapperFactoryExtensions).Assembly)
                .AddHostingCommands()
                .AddWebServices()
                .AddInputPaths()
                .AddExcludedPaths()
                .SetOutputPath()
                .AddThemePaths()
                .AddDefaultWebSettings()
                .ConfigureEngine(e => e.LogAndCheckVersion(typeof(BootstrapperExtensions).Assembly, "Statiq Web", WebKeys.MinimumStatiqWebVersion));

        private static Bootstrapper AddWebServices(this Bootstrapper bootstrapper) =>
            bootstrapper
                .ConfigureServices(services => services
                    .AddSingleton(new Templates())
                    .AddSingleton(new ThemeManager()));

        private static Bootstrapper AddInputPaths(this Bootstrapper bootstrapper) =>
            bootstrapper
                .ConfigureEngine(engine =>
                {
                    IReadOnlyList<NormalizedPath> paths = engine.Settings.GetList<NormalizedPath>(WebKeys.InputPaths);
                    if (paths?.Count > 0)
                    {
                        engine.FileSystem.InputPaths.Clear();
                        foreach (NormalizedPath path in paths)
                        {
                            engine.FileSystem.InputPaths.Add(path);
                        }
                    }
                });

        private static Bootstrapper AddExcludedPaths(this Bootstrapper bootstrapper) =>
            bootstrapper
                .ConfigureEngine(engine =>
                {
                    IReadOnlyList<NormalizedPath> paths = engine.Settings.GetList<NormalizedPath>(WebKeys.ExcludedPaths);
                    if (paths?.Count > 0)
                    {
                        engine.FileSystem.ExcludedPaths.Clear();
                        foreach (NormalizedPath path in paths)
                        {
                            engine.FileSystem.ExcludedPaths.Add(path);
                        }
                    }
                });

        private static Bootstrapper SetOutputPath(this Bootstrapper bootstrapper) =>
            bootstrapper
                .ConfigureEngine(engine =>
                {
                    NormalizedPath path = engine.Settings.GetPath(WebKeys.OutputPath);
                    if (!path.IsNullOrEmpty)
                    {
                        engine.FileSystem.OutputPath = path;
                    }
                });

        private static Bootstrapper AddThemePaths(this Bootstrapper bootstrapper) =>
            bootstrapper
                .ConfigureEngine(engine =>
                {
                    ThemeManager themeManager = engine.Services.GetRequiredService<ThemeManager>();

                    // Add theme paths from settings
                    IReadOnlyList<NormalizedPath> settingsThemePaths = engine.Settings.GetList<NormalizedPath>(WebKeys.ThemePaths);
                    if (settingsThemePaths?.Count > 0)
                    {
                        themeManager.ThemePaths.Clear();
                        foreach (NormalizedPath settingsThemePath in settingsThemePaths)
                        {
                            themeManager.ThemePaths.Add(settingsThemePath);
                        }
                    }

                    // Iterate in reverse order so we start with the highest priority
                    foreach (NormalizedPath themePath in themeManager.ThemePaths.Reverse())
                    {
                        // Inserting at 0 preserves the original order since we're iterating in reverse
                        engine.FileSystem.InputPaths.Insert(0, themePath.Combine("input"));

                        // Build a configuration for each of the theme paths
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
                                if (!engine.Settings.ContainsKey(config.Key))
                                {
                                    engine.Settings[config.Key] = config.Value;
                                }
                            }
                        }
                    }
                });

        private static Bootstrapper AddDefaultWebSettings(this Bootstrapper bootstrapper) =>
            bootstrapper
                .AddSettingsIfNonExisting(new Dictionary<string, object>
                {
                    { WebKeys.InputFiles, "**/{!_,}*" },
                    { WebKeys.DirectoryMetadataFiles, "**/_{d,D}irectory.*" },
                    { WebKeys.Xref, Config.FromDocument(doc => doc.GetTitle().Replace(' ', '-')) },
                    { WebKeys.Excluded, Config.FromDocument(doc => doc.GetDateTime(WebKeys.Published) > DateTime.Today.AddDays(1)) }, // Add +1 days so the threshold is midnight on the current day
                    { WebKeys.PublishedUsesLastModifiedDate, true }
                });

        /// <summary>
        /// Adds the "preview" and "serve" commands (this is called by default when you
        /// call <see cref="BootstrapperFactoryExtensions.CreateWeb(BootstrapperFactory, string[])"/>.
        /// </summary>
        /// <param name="bootstrapper">The current bootstrapper.</param>
        /// <returns>The bootstrapper.</returns>
        public static Bootstrapper AddHostingCommands(this Bootstrapper bootstrapper)
        {
            bootstrapper.ThrowIfNull(nameof(bootstrapper));
            bootstrapper.AddCommand<PreviewCommand>();
            bootstrapper.AddCommand<ServeCommand>();
            return bootstrapper;
        }

        /// <summary>
        /// Configures the set of template modules.
        /// </summary>
        /// <param name="bootstrapper">The current bootstrapper.</param>
        /// <param name="action">The configuration action.</param>
        /// <returns>The bootstrapper.</returns>
        public static Bootstrapper ConfigureTemplates(this Bootstrapper bootstrapper, Action<Templates> action) =>
            bootstrapper.ConfigureServices(services =>
                action(services
                    .BuildServiceProvider() // We need to build an intermediate service provider to get access to the singleton
                    .GetRequiredService<Templates>()));
    }
}
