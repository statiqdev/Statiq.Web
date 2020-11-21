using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spectre.Cli;
using Statiq.App;
using Statiq.Common;
using Statiq.Web.Hosting;

namespace Statiq.Web.Commands
{
    [Description("Builds the site and serves it, optionally watching for changes, rebuilding, and triggering client reload by default.")]
    public class PreviewCommand : InteractiveCommand<PreviewCommandSettings>
    {
        private readonly ConcurrentQueue<string> _changedFiles = new ConcurrentQueue<string>();
        private readonly ResetCacheMetadataValue _resetCache = new ResetCacheMetadataValue();

        private ActionFileSystemWatcher _inputFolderWatcher;
        private Server _previewServer;
        private Dictionary<string, string> _contentTypes;

        public PreviewCommand(
            IConfiguratorCollection configurators,
            Settings settings,
            IServiceCollection serviceCollection,
            Bootstrapper bootstrapper)
            : base(
                  configurators,
                  settings,
                  serviceCollection,
                  bootstrapper)
        {
            // Add a lazy ResetCache value - we need to add it here and make it lazy since
            // the configuration settings are disposed once the engine is created
            settings[Keys.ResetCache] = _resetCache;
        }

        protected override async Task AfterInitialExecutionAsync(
            CommandContext commandContext,
            PreviewCommandSettings commandSettings,
            IEngineManager engineManager,
            CancellationTokenSource cancellationTokenSource)
        {
            ILogger logger = engineManager.Engine.Services.GetRequiredService<ILogger<Bootstrapper>>();

            // Start the preview server
            _contentTypes = commandSettings.ContentTypes?.Length > 0
                ? GetContentTypes(commandSettings.ContentTypes)
                : new Dictionary<string, string>();
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
                    loggerProviders,
                    logger);
            }

            // Start the watchers
            if (!commandSettings.NoWatch)
            {
                logger.LogInformation("Watching paths(s) {0}", string.Join(", ", engineManager.Engine.FileSystem.InputPaths));
                _inputFolderWatcher = new ActionFileSystemWatcher(
                    outputDirectory.Path,
                    engineManager.Engine.FileSystem.GetInputDirectories().Select(x => x.Path),
                    true,
                    "*.*",
                    path =>
                    {
                        _changedFiles.Enqueue(path);
                        TriggerExecution();
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
                    logger.LogDebug($"{changedFile} has changed");
                }
            }
            if (forceExecution || changedFiles.Count > 0)
            {
                if (changedFiles.Count > 0)
                {
                    logger.LogInformation($"{changedFiles.Count} files have changed, re-executing");
                }

                // Reset caches when an error occurs during the previous preview
                bool? existingResetCacheSetting = null;
                bool setResetCacheSetting = false;
                if (previousExitCode != ExitCode.Normal)
                {
                    existingResetCacheSetting = engineManager.Engine.Settings.ContainsKey(Keys.ResetCache)
                        ? engineManager.Engine.Settings.GetBool(Keys.ResetCache)
                        : (bool?)null;
                    setResetCacheSetting = true;
                    _resetCache.Value = true;
                }

                // If there was an execution error due to reload, keep previewing but clear the cache
                previousExitCode = await base.ExecutionTriggeredAsync(commandContext, commandSettings, engineManager, previousExitCode, cancellationTokenSource);

                // Reset the reset cache setting after removing it
                if (setResetCacheSetting)
                {
                    _resetCache.Value = existingResetCacheSetting ?? false;
                }

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

        internal static Dictionary<string, string> GetContentTypes(string[] contentTypes)
        {
            Dictionary<string, string> contentTypeDictionary = new Dictionary<string, string>();
            foreach (string contentType in contentTypes)
            {
                string[] splitContentType = contentType.Split('=');
                if (splitContentType.Length != 2)
                {
                    throw new ArgumentException($"Invalid content type {contentType} specified.");
                }
                contentTypeDictionary[splitContentType[0].Trim().Trim('\"')] = splitContentType[1].Trim().Trim('\"');
            }
            return contentTypeDictionary;
        }

        internal static async Task<Server> StartPreviewServerAsync(
            NormalizedPath path,
            int port,
            bool forceExtension,
            NormalizedPath virtualDirectory,
            bool liveReload,
            IDictionary<string, string> contentTypes,
            IEnumerable<ILoggerProvider> loggerProviders,
            ILogger logger)
        {
            Server server;
            try
            {
                server = new Server(path.FullPath, port, !forceExtension, virtualDirectory.IsNull ? null : virtualDirectory.FullPath, liveReload, contentTypes, loggerProviders);
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
            new InteractiveGlobals(
                engineManager.Engine,
                () =>
                {
                    _changedFiles.Enqueue(null);
                    TriggerExecution();
                },
                TriggerExit);
    }
}
