using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using Statiq.App;
using Statiq.Common;
using Statiq.Web.Hosting;

namespace Statiq.Web.Commands
{
    [Description("Builds the site and serves it, optionally watching for changes, rebuilding, and triggering client reload by default.")]
    public class PreviewCommand : InteractiveCommand<PreviewCommandSettings>
    {
        private readonly ConcurrentQueue<string> _changedFiles = new ConcurrentQueue<string>();
        private readonly ResetCacheMetadataValue _resetCacheMetadata = new ResetCacheMetadataValue();

        private InputFileWatcher _inputFolderWatcher;
        private Server _previewServer;
        private Dictionary<string, string> _contentTypes = new Dictionary<string, string>();
        private Dictionary<string, string> _customHeaders = new Dictionary<string, string>();
        private bool _resetCache = false;

        public PreviewCommand(
            IConfiguratorCollection configurators,
            Settings settings,
            IServiceCollection serviceCollection,
            IFileSystem fileSystem,
            Bootstrapper bootstrapper)
            : base(
                  configurators,
                  settings,
                  serviceCollection,
                  fileSystem,
                  bootstrapper)
        {
            // Add a lazy ResetCache value - we need to add it here and make it lazy since
            // the configuration settings are disposed once the engine is created
            settings[Keys.ResetCache] = _resetCacheMetadata;
        }

        protected override async Task AfterInitialExecutionAsync(
            CommandContext commandContext,
            PreviewCommandSettings commandSettings,
            IEngineManager engineManager,
            CancellationTokenSource cancellationTokenSource)
        {
            ILogger logger = engineManager.Engine.Services.GetRequiredService<ILogger<Bootstrapper>>();

            // Get content types and custom headers from the settings and/or CLI
            GetKeyValues(
                _contentTypes,
                WebKeys.ServerContentTypes,
                Settings,
                engineManager.Engine,
                commandSettings.ContentTypes,
                "content type");
            GetKeyValues(
                _customHeaders,
                WebKeys.ServerCustomHeaders,
                Settings,
                engineManager.Engine,
                commandSettings.CustomHeaders,
                "custom header");

            // Start the preview server
            IEnumerable<ILoggerProvider> loggerProviders = engineManager.Engine.Services.GetServices<ILoggerProvider>();
            IDirectory outputDirectory = engineManager.Engine.FileSystem.GetOutputDirectory();
            if (outputDirectory.Exists)
            {
                _previewServer = await StartPreviewServerAsync(
                    outputDirectory.Path,
                    commandSettings.Port,
                    commandSettings.ForceExt,
                    commandSettings.VirtualDirectory,
                    !commandSettings.NoReload,
                    _contentTypes,
                    _customHeaders,
                    loggerProviders,
                    logger);
            }

            // Start the watchers
            if (!commandSettings.NoWatch)
            {
                logger.LogInformation("Watching paths(s) {0}", string.Join(", ", engineManager.Engine.FileSystem.InputPaths));
                _inputFolderWatcher = new InputFileWatcher(
                    outputDirectory.Path,
                    engineManager.Engine.FileSystem.GetInputDirectories().Select(x => x.Path)
                        .Concat(engineManager.Engine.Settings.GetList(WebKeys.WatchPaths, Array.Empty<NormalizedPath>())
                            .Select(x => engineManager.Engine.FileSystem.GetRootPath(x))),
                    true,
                    "*.*",
                    paths =>
                    {
                        foreach (string path in paths)
                        {
                            _changedFiles.Enqueue(path);
                        }
                        TriggerExecution();
                        return Task.CompletedTask;
                    });
            }
        }

        protected override async Task<ExitCode> ExecutionTriggeredAsync(
            CommandContext commandContext,
            PreviewCommandSettings commandSettings,
            IEngineManager engineManager,
            ExitCode previousExitCode,
            CancellationTokenSource cancellationTokenSource)
        {
            ILogger logger = engineManager.Engine.Services.GetRequiredService<ILogger<Bootstrapper>>();

            // Execute if files have changed
            HashSet<string> changedFiles = new HashSet<string>();
            bool forceExecution = commandSettings.NoWatch;
            while (_changedFiles.TryDequeue(out string changedFile))
            {
                if (changedFile is null)
                {
                    // Special case to force execution when no files have changed (I.e. via interactive)
                    forceExecution = true;
                }
                else if (changedFiles.Add(changedFile))
                {
                    logger.LogInformation($"{changedFile} has changed");
                }
            }
            if (forceExecution || changedFiles.Count > 0)
            {
                if (changedFiles.Count > 0)
                {
                    logger.LogInformation($"{changedFiles.Count} files have changed, re-executing");
                }

                // Reset caches when an error occurs during the previous preview or if requested
                bool? existingResetCacheSetting = null;
                if (previousExitCode != ExitCode.Normal || _resetCache)
                {
                    existingResetCacheSetting = engineManager.Engine.Settings.ContainsKey(Keys.ResetCache)
                        ? engineManager.Engine.Settings.GetBool(Keys.ResetCache)
                        : (bool?)null;
                    _resetCache = true;
                    _resetCacheMetadata.Value = true;
                }

                // If there was an execution error due to reload, keep previewing but clear the cache
                previousExitCode = await base.ExecutionTriggeredAsync(commandContext, commandSettings, engineManager, previousExitCode, cancellationTokenSource);

                // Reset the reset cache setting after removing it
                if (_resetCache)
                {
                    _resetCacheMetadata.Value = existingResetCacheSetting ?? false;
                }
                _resetCache = false;

                if (_previewServer is null)
                {
                    IEnumerable<ILoggerProvider> loggerProviders = engineManager.Engine.Services.GetServices<ILoggerProvider>();
                    IDirectory outputDirectory = engineManager.Engine.FileSystem.GetOutputDirectory();
                    if (outputDirectory.Exists)
                    {
                        _previewServer = await StartPreviewServerAsync(
                            outputDirectory.Path,
                            commandSettings.Port,
                            commandSettings.ForceExt,
                            commandSettings.VirtualDirectory,
                            !commandSettings.NoReload,
                            _contentTypes,
                            _customHeaders,
                            loggerProviders,
                            logger);
                    }
                }
                else
                {
                    await _previewServer.TriggerReloadAsync();
                }
            }
            return previousExitCode;
        }

        protected override Task ExitingAsync(CommandContext commandContext, PreviewCommandSettings commandSettings, IEngineManager engineManager)
        {
            _inputFolderWatcher?.Dispose();
            _inputFolderWatcher = null;
            _previewServer?.Dispose();
            _previewServer = null;
            return Task.CompletedTask;
        }

        internal static void GetKeyValues(
            Dictionary<string, string> dictionary,
            string settingsKey,
            Settings commandSettings,
            IEngine engine,
            string[] commandValues,
            string optionName)
        {
            // First get them from the command settings (likely none here, but try anyway)
            IReadOnlyList<string> values = commandSettings.GetList<string>(settingsKey);
            if (values?.Count > 0)
            {
                dictionary.AddOrReplaceRange(GetKeyValues(values, optionName));
            }

            // Then try the engine settings
            values = engine.Settings.GetList<string>(settingsKey);
            if (values?.Count > 0)
            {
                dictionary.AddOrReplaceRange(GetKeyValues(values, optionName));
            }

            // Then finally apply from the command line
            if (commandValues?.Length > 0)
            {
                dictionary.AddOrReplaceRange(GetKeyValues(commandValues, optionName));
            }
        }

        private static Dictionary<string, string> GetKeyValues(IEnumerable<string> keysAndValues, string optionName)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            foreach (string keyAndValue in keysAndValues)
            {
                string[] split = keyAndValue.Split('=');
                if (split.Length < 2)
                {
                    throw new ArgumentException($"Invalid {optionName} {keyAndValue} specified.");
                }
                dictionary[split[0].Trim().Trim('\"')] = split[1].Trim().Trim('\"');
            }
            return dictionary;
        }

        internal static async Task<Server> StartPreviewServerAsync(
            NormalizedPath path,
            int port,
            bool forceExtension,
            NormalizedPath virtualDirectory,
            bool liveReload,
            IReadOnlyDictionary<string, string> contentTypes,
            IReadOnlyDictionary<string, string> customHeaders,
            IEnumerable<ILoggerProvider> loggerProviders,
            ILogger logger)
        {
            Server server;
            try
            {
                ServerFactory serverFactory = new ServerFactory()
                    .WithExtensionless(!forceExtension)
                    .WithVirtualDirectory(virtualDirectory.IsNull ? null : virtualDirectory.FullPath)
                    .WithLiveReload(liveReload)
                    .WithContentTypes(contentTypes)
                    .WithCustomHeaders(customHeaders)
                    .WithLoggerProviders(loggerProviders);
                server = serverFactory.CreateServer(path.FullPath, port);
                await server.StartAsync();
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, $"Error while running preview server: {ex}");
                return null;
            }

            string urlPath = server.VirtualDirectory ?? string.Empty;
            logger.LogInformation($"Preview server listening at http://localhost:{port}{urlPath} (on any hostname/IP) and serving from path {path}"
                + (liveReload ? " with LiveReload support" : string.Empty));
            return server;
        }

        // This needs to be lazily evaluated so that we can change it after the configuration settings are copied to the engine
        private class ResetCacheMetadataValue : IMetadataValue
        {
            public bool Value { get; set; }

            public object Get(string key, IMetadata metadata) => Value;
        }

        protected override InteractiveGlobals GetInteractiveGlobals(
            CommandContext commandContext,
            PreviewCommandSettings commandSettings,
            IEngineManager engineManager) =>
            new PreviewGlobals(
                this,
                engineManager.Engine,
                () =>
                {
                    _changedFiles.Enqueue(null);
                    TriggerExecution();
                },
                TriggerExit);

        public class PreviewGlobals : InteractiveGlobals
        {
            private readonly PreviewCommand _previewCommand;

            public PreviewGlobals(PreviewCommand previewCommand, IEngine engine, Action triggerExecution, Action triggerExit)
                : base(engine, triggerExecution, triggerExit)
            {
                _previewCommand = previewCommand;
            }

            [Description("Resets the cache on the next execution.")]
            public void ResetCache()
            {
                _previewCommand._resetCache = true;
                Engine.Logger.LogInformation("The cache will be reset on the next execution");
            }
        }
    }
}