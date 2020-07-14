using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Statiq.App;
using Statiq.Common;
using Statiq.Web.Commands;
using Statiq.Web.Modules;

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
                .AddDefaultWebSettings();

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
                    { WebKeys.ContentFiles, "**/{!_,}*.{html,cshtml,md}" },
                    { WebKeys.DataFiles, $"**/{{!_,}}*.{{{string.Join(",", ParseDataContent.SupportedExtensions)}}}" },
                    { WebKeys.DirectoryMetadataFiles, "**/_{d,D}irectory.{json,yaml,yml}" },
                    { WebKeys.Xref, Config.FromDocument(doc => doc.GetTitle().Replace(' ', '-')) },
                    { WebKeys.Excluded, Config.FromDocument(doc => doc.GetPublishedDate(false) > DateTime.Today.AddDays(1)) } // Add +1 days so the threshold is midnight on the current day
                });

        /// <summary>
        /// Adds the "preview" and "serve" commands (this is called by default when you
        /// call <see cref="BootstrapperFactoryExtensions.CreateWeb(BootstrapperFactory, string[])"/>.
        /// </summary>
        /// <param name="bootstrapper">The current bootstrapper.</param>
        /// <returns>The bootstrapper.</returns>
        public static Bootstrapper AddHostingCommands(this Bootstrapper bootstrapper)
        {
            _ = bootstrapper ?? throw new ArgumentNullException(nameof(bootstrapper));
            bootstrapper.AddCommand<PreviewCommand>();
            bootstrapper.AddCommand<ServeCommand>();
            return bootstrapper;
        }

        public static Bootstrapper SetDefaultTemplate(this Bootstrapper bootstrapper, string defaultTemplate) =>
            bootstrapper.ConfigureTemplates(templates => templates.DefaultTemplate = defaultTemplate);

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

        public static Bootstrapper ConfigureThemePaths(this Bootstrapper bootstrapper, Action<PathCollection> action) =>
            bootstrapper.ConfigureServices(services =>
                action(services
                    .BuildServiceProvider() // We need to build an intermediate service provider to get access to the singleton
                    .GetRequiredService<ThemeManager>()
                    .ThemePaths));

        public static Bootstrapper AddThemePath(this Bootstrapper bootstrapper, NormalizedPath themePath) =>
            bootstrapper.ConfigureThemePaths(paths => paths.Add(themePath));

        public static Bootstrapper SetThemePath(this Bootstrapper bootstrapper, NormalizedPath themePath) =>
            bootstrapper.ConfigureThemePaths(paths =>
            {
                paths.Clear();
                paths.Add(themePath);
            });

        public static Bootstrapper DeployToGitHubPages(
            this Bootstrapper bootstrapper,
            Config<string> owner,
            Config<string> name,
            Config<string> username,
            Config<string> password) =>
            bootstrapper
                .AddSettingsIfNonExisting(new Dictionary<string, object>
                {
                    { WebKeys.GitHubOwner, owner },
                    { WebKeys.GitHubName, name },
                    { WebKeys.GitHubUsername, username },
                    { WebKeys.GitHubPassword, password }
                });

        public static Bootstrapper DeployToGitHubPagesBranch(
            this Bootstrapper bootstrapper,
            Config<string> owner,
            Config<string> name,
            Config<string> username,
            Config<string> password,
            Config<string> branch) =>
            bootstrapper
                .AddSettingsIfNonExisting(new Dictionary<string, object>
                {
                    { WebKeys.GitHubOwner, owner },
                    { WebKeys.GitHubName, name },
                    { WebKeys.GitHubUsername, username },
                    { WebKeys.GitHubPassword, password },
                    { WebKeys.GitHubBranch, branch }
                });

        public static Bootstrapper DeployToGitHubPages(
            this Bootstrapper bootstrapper,
            Config<string> owner,
            Config<string> name,
            Config<string> token) =>
            bootstrapper
                .AddSettingsIfNonExisting(new Dictionary<string, object>
                {
                    { WebKeys.GitHubOwner, owner },
                    { WebKeys.GitHubName, name },
                    { WebKeys.GitHubToken, token }
                });

        public static Bootstrapper DeployToGitHubPagesBranch(
            this Bootstrapper bootstrapper,
            Config<string> owner,
            Config<string> name,
            Config<string> token,
            Config<string> branch) =>
            bootstrapper
                .AddSettingsIfNonExisting(new Dictionary<string, object>
                {
                    { WebKeys.GitHubOwner, owner },
                    { WebKeys.GitHubName, name },
                    { WebKeys.GitHubToken, token },
                    { WebKeys.GitHubBranch, branch }
                });

        public static Bootstrapper DeployToNetlify(
            this Bootstrapper bootstrapper,
            Config<string> siteId,
            Config<string> accessToken) =>
            bootstrapper
                .AddSettingsIfNonExisting(new Dictionary<string, object>
                {
                    { WebKeys.NetlifySiteId, siteId },
                    { WebKeys.NetlifyAccessToken, accessToken }
                });

        public static Bootstrapper DeployToAzureAppService(
            this Bootstrapper bootstrapper,
            Config<string> siteName,
            Config<string> username,
            Config<string> password) =>
            bootstrapper
                .AddSettingsIfNonExisting(new Dictionary<string, object>
                {
                    { WebKeys.AzureAppServiceSiteName, siteName },
                    { WebKeys.AzureAppServiceUsername, username },
                    { WebKeys.AzureAppServicePassword, password }
                });
    }
}
