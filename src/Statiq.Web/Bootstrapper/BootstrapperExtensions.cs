using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        /// <param name="bootstrapper">The bootstrapper to add Statiq Web functionality to.</param>
        /// <returns>The bootstrapper.</returns>
        public static TBootstrapper AddWeb<TBootstrapper>(this TBootstrapper bootstrapper)
            where TBootstrapper : IBootstrapper =>
            bootstrapper
                .AddPipelines(typeof(BootstrapperFactoryExtensions).Assembly)
                .AddHostingCommands()
                .AddWebServices()
                .AddInputPaths()
                .AddExcludedPaths()
                .SetOutputPath()
                .SetTempPath()
                .SetCachePath()
                .AddThemes()
                .AddDefaultWebSettings()
                .AddWebAnalyzers()
                .AddProcessEventHandlers()
                .ConfigureEngine(e => e.LogAndCheckVersion(typeof(BootstrapperExtensions).Assembly, "Statiq Web", WebKeys.MinimumStatiqWebVersion));

        // Add these as new instances so that the same singleton gets returned for a temporary BuildServiceCollection()
        private static TBootstrapper AddWebServices<TBootstrapper>(this TBootstrapper bootstrapper)
            where TBootstrapper : IBootstrapper =>
            bootstrapper
                .ConfigureServices(services => services
                    .AddSingleton(new Templates())
                    .AddSingleton(new Processes())
                    .AddSingleton(new ThemeManager()));

        private static TBootstrapper AddInputPaths<TBootstrapper>(this TBootstrapper bootstrapper)
            where TBootstrapper : IBootstrapper =>
            bootstrapper
                .ConfigureFileSystem((fileSystem, settings) =>
                {
                    IReadOnlyList<NormalizedPath> paths = settings.GetList<NormalizedPath>(WebKeys.InputPaths);
                    if (paths?.Count > 0)
                    {
                        fileSystem.InputPaths.Clear();
                        foreach (NormalizedPath path in paths)
                        {
                            fileSystem.InputPaths.Add(path);
                        }
                    }
                });

        private static TBootstrapper AddExcludedPaths<TBootstrapper>(this TBootstrapper bootstrapper)
            where TBootstrapper : IBootstrapper =>
            bootstrapper
                .ConfigureFileSystem((fileSystem, settings) =>
                {
                    IReadOnlyList<NormalizedPath> paths = settings.GetList<NormalizedPath>(WebKeys.ExcludedPaths);
                    if (paths?.Count > 0)
                    {
                        fileSystem.ExcludedPaths.Clear();
                        foreach (NormalizedPath path in paths)
                        {
                            fileSystem.ExcludedPaths.Add(path);
                        }
                    }
                });

        private static TBootstrapper SetOutputPath<TBootstrapper>(this TBootstrapper bootstrapper)
            where TBootstrapper : IBootstrapper =>
            bootstrapper
                .ConfigureFileSystem((fileSystem, settings) =>
                {
                    NormalizedPath path = settings.GetPath(WebKeys.OutputPath);
                    if (!path.IsNullOrEmpty)
                    {
                        fileSystem.OutputPath = path;
                    }
                });

        private static TBootstrapper SetTempPath<TBootstrapper>(this TBootstrapper bootstrapper)
            where TBootstrapper : IBootstrapper =>
            bootstrapper
                .ConfigureFileSystem((fileSystem, settings) =>
                {
                    NormalizedPath path = settings.GetPath(WebKeys.TempPath);
                    if (!path.IsNullOrEmpty)
                    {
                        fileSystem.TempPath = path;
                    }
                });

        private static TBootstrapper SetCachePath<TBootstrapper>(this TBootstrapper bootstrapper)
            where TBootstrapper : IBootstrapper =>
            bootstrapper
                .ConfigureFileSystem((fileSystem, settings) =>
                {
                    NormalizedPath path = settings.GetPath(WebKeys.CachePath);
                    if (!path.IsNullOrEmpty)
                    {
                        fileSystem.CachePath = path;
                    }
                });

        private static TBootstrapper AddThemes<TBootstrapper>(this TBootstrapper bootstrapper)
            where TBootstrapper : IBootstrapper =>
            bootstrapper
                .ConfigureFileSystem((fileSystem, settings, serviceCollection) =>
                {
                    // Create a temporary service provider to get the theme manager (which is okay
                    // since it was registered as a singleton instance, not constructed by the service provider)
                    IServiceProvider services = serviceCollection.BuildServiceProvider();
                    ThemeManager themeManager = services.GetRequiredService<ThemeManager>();
                    themeManager.AddPathsFromSettings(settings, fileSystem);
                })
                .ConfigureSettings((settings, serviceCollection, fileSystem) =>
                {
                    // Build any csproj files in the theme directory
                    // This needs to be done in ConfigureSettings because it's after the file system is configured
                    // so the root path is set, but it's before the engine is created so we can still manipulate
                    // the services and settings

                    // Create a temporary service provider to get the theme manager (which is okay
                    // since it was registered as a singleton instance, not constructed by the service provider)
                    IServiceProvider services = serviceCollection.BuildServiceProvider();
                    ThemeManager themeManager = services.GetRequiredService<ThemeManager>();
                    ClassCatalog classCatalog = services.GetRequiredService<ClassCatalog>();
                    ILogger logger = services.GetRequiredService<ILogger<ThemeManager>>();
                    themeManager.CompileProjects(settings, serviceCollection, fileSystem, classCatalog, logger);
                })
                .ConfigureEngine(engine =>
                {
                    ThemeManager themeManager = engine.Services.GetRequiredService<ThemeManager>();
                    themeManager.AddSettingsToEngine(engine);
                    themeManager.AddCompiledNamespacesToEngine(engine);
                });

        private static TBootstrapper AddDefaultWebSettings<TBootstrapper>(this TBootstrapper bootstrapper)
            where TBootstrapper : IBootstrapper =>
            bootstrapper
                .AddSettingsIfNonExisting(new Dictionary<string, object>
                {
                    { WebKeys.InputFiles, "**/{!_,}*" },
                    { WebKeys.DirectoryMetadataFiles, "**/_{d,D}irectory.*" },
                    { WebKeys.Xref, Config.FromDocument(doc => doc.GetTitle().Replace(' ', '-')) },
                    { WebKeys.Excluded, Config.FromDocument(doc => doc.GetDateTime(WebKeys.Published) > DateTime.Today.AddDays(1)) }, // Add +1 days so the threshold is midnight on the current day
                    { WebKeys.PublishedUsesLastModifiedDate, true }
                });

        private static TBootstrapper AddWebAnalyzers<TBootstrapper>(this TBootstrapper bootstrapper)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddAnalyzers(typeof(BootstrapperExtensions).Assembly);

        private static TBootstrapper AddProcessEventHandlers<TBootstrapper>(this TBootstrapper bootstrapper)
            where TBootstrapper : IBootstrapper =>
            bootstrapper
                .ConfigureEngine(engine =>
                {
                    bool previewCommand = typeof(PreviewCommand).IsAssignableFrom(bootstrapper.Command.GetType());
                    Processes processes = engine.Services.GetRequiredService<Processes>();
                    processes.CreateProcessLaunchers(previewCommand, engine);
                    bool firstExecution = true;
                    engine.Events.Subscribe<BeforeEngineExecution>(evt =>
                    {
                        if (firstExecution)
                        {
                            processes.StartProcesses(ProcessTiming.Initialization, evt.Engine);
                            processes.WaitForRunningProcesses(ProcessTiming.Initialization);
                        }
                        firstExecution = false;

                        processes.StartProcesses(ProcessTiming.BeforeExecution, evt.Engine);
                    });
                    engine.Events.Subscribe<BeforeDeployment>(evt =>
                    {
                        processes.WaitForRunningProcesses(ProcessTiming.BeforeExecution);
                        processes.StartProcesses(ProcessTiming.BeforeDeployment, evt.Engine);
                    });
                    engine.Events.Subscribe<AfterEngineExecution>(evt =>
                    {
                        processes.WaitForRunningProcesses(ProcessTiming.BeforeDeployment);
                        processes.StartProcesses(ProcessTiming.AfterExecution, evt.Engine);
                        processes.WaitForRunningProcesses(ProcessTiming.AfterExecution);
                    });
                });

        /// <summary>
        /// Adds the "preview" and "serve" commands (this is called by default when you
        /// call <see cref="BootstrapperFactoryExtensions.CreateWeb(BootstrapperFactory, string[])"/>.
        /// </summary>
        /// <param name="bootstrapper">The current bootstrapper.</param>
        /// <returns>The bootstrapper.</returns>
        public static TBootstrapper AddHostingCommands<TBootstrapper>(this TBootstrapper bootstrapper)
            where TBootstrapper : IBootstrapper
        {
            bootstrapper.ThrowIfNull(nameof(bootstrapper));
            bootstrapper.AddCommand(typeof(PreviewCommand));
            bootstrapper.AddCommand(typeof(ServeCommand));
            return bootstrapper;
        }
    }
}