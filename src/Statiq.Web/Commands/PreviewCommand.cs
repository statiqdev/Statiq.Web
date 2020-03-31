using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spectre.Cli;
using Statiq.App;
using Statiq.Common;
using Statiq.Web.Hosting;

namespace Statiq.Web.Commands
{
    [Description("Builds the site and serves it, optionally watching for changes, rebuilding, and triggering client reload by default.")]
    internal class PreviewCommand : PipelinesCommand<PreviewCommandSettings>
    {
        private readonly ConcurrentQueue<string> _changedFiles = new ConcurrentQueue<string>();
        private readonly AutoResetEvent _messageEvent = new AutoResetEvent(false);
        private readonly InterlockedBool _exit = new InterlockedBool(false);

        public PreviewCommand(
            IConfiguratorCollection configurators,
            IConfigurationSettings configurationSettings,
            IServiceCollection serviceCollection,
            IConfigurationRoot configurationRoot,
            Bootstrapper bootstrapper)
            : base(
                  configurators,
                  configurationSettings,
                  serviceCollection,
                  configurationRoot,
                  bootstrapper)
        {
        }

        protected override async Task<int> ExecuteEngineAsync(
            CommandContext commandContext,
            PreviewCommandSettings commandSettings,
            IEngineManager engineManager)
        {
            SetPipelines(commandContext, commandSettings, engineManager);

            ExitCode exitCode = ExitCode.Normal;
            ILogger logger = engineManager.Engine.Services.GetRequiredService<ILogger<Bootstrapper>>();

            // Execute the engine for the first time
            using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
            {
                if (!await engineManager.ExecuteAsync(cancellationTokenSource))
                {
                    return (int)ExitCode.ExecutionError;
                }
            }

            // Start the preview server
            Dictionary<string, string> contentTypes = commandSettings.ContentTypes?.Length > 0
                ? GetContentTypes(commandSettings.ContentTypes)
                : new Dictionary<string, string>();
            ILoggerProvider loggerProvider = engineManager.Engine.Services.GetRequiredService<ILoggerProvider>();
            IDirectory outputDirectory = engineManager.Engine.FileSystem.GetOutputDirectory();
            Server previewServer = null;
            if (outputDirectory.Exists)
            {
                previewServer = await StartPreviewServerAsync(
                    outputDirectory.Path,
                    commandSettings.Port,
                    commandSettings.ForceExt,
                    commandSettings.VirtualDirectory,
                    !commandSettings.NoReload,
                    contentTypes,
                    loggerProvider,
                    logger);
            }

            // Start the watchers
            ActionFileSystemWatcher inputFolderWatcher = null;
            if (!commandSettings.NoWatch)
            {
                logger.LogInformation("Watching paths(s) {0}", string.Join(", ", engineManager.Engine.FileSystem.InputPaths));
                inputFolderWatcher = new ActionFileSystemWatcher(
                    outputDirectory.Path,
                    engineManager.Engine.FileSystem.GetInputDirectories().Select(x => x.Path),
                    true,
                    "*.*",
                    path =>
                    {
                        _changedFiles.Enqueue(path);
                        _messageEvent.Set();
                    });
            }

            // Start the message pump
            CommandUtilities.WaitForControlC(
                () =>
                {
                    _exit.Set();
                    _messageEvent.Set();
                },
                logger);

            // Wait for activity
            while (true)
            {
                _messageEvent.WaitOne(); // Blocks the current thread until a signal
                if (_exit)
                {
                    break;
                }

                // Execute if files have changed
                HashSet<string> changedFiles = new HashSet<string>();
                while (_changedFiles.TryDequeue(out string changedFile))
                {
                    if (changedFiles.Add(changedFile))
                    {
                        logger.LogDebug($"{changedFile} has changed");
                    }
                }
                if (changedFiles.Count > 0)
                {
                    logger.LogInformation($"{changedFiles.Count} files have changed, re-executing");

                    // Reset caches when an error occurs during the previous preview
                    string existingResetCacheSetting = null;
                    bool setResetCacheSetting = false;
                    if (exitCode == ExitCode.ExecutionError)
                    {
                        existingResetCacheSetting = engineManager.Engine.Settings.GetString(Keys.ResetCache);
                        setResetCacheSetting = true;
                        ConfigurationSettings[Keys.ResetCache] = "true";
                    }

                    // If there was an execution error due to reload, keep previewing but clear the cache
                    using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
                    {
                        exitCode = await engineManager.ExecuteAsync(cancellationTokenSource)
                            ? ExitCode.Normal
                            : ExitCode.ExecutionError;
                    }

                    // Reset the reset cache setting after removing it
                    if (setResetCacheSetting)
                    {
                        if (existingResetCacheSetting == null)
                        {
                            ConfigurationSettings.Remove(Keys.ResetCache);
                        }
                        {
                            ConfigurationSettings[Keys.ResetCache] = existingResetCacheSetting;
                        }
                    }

                    if (previewServer == null)
                    {
                        if (outputDirectory.Exists)
                        {
                            previewServer = await StartPreviewServerAsync(
                                outputDirectory.Path,
                                commandSettings.Port,
                                commandSettings.ForceExt,
                                commandSettings.VirtualDirectory,
                                !commandSettings.NoReload,
                                contentTypes,
                                loggerProvider,
                                logger);
                        }
                    }
                    else
                    {
                        await previewServer.TriggerReloadAsync();
                    }
                }

                // Check one more time for exit
                if (_exit)
                {
                    break;
                }
                logger.LogInformation("Hit Ctrl-C to exit");
                _messageEvent.Reset();
            }

            // Shutdown
            logger.LogInformation("Shutting down");
            inputFolderWatcher?.Dispose();
            previewServer?.Dispose();

            return (int)exitCode;
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
            ILoggerProvider loggerProvider,
            ILogger logger)
        {
            Server server;
            try
            {
                server = new Server(path.FullPath, port, !forceExtension, virtualDirectory.IsNull ? null : virtualDirectory.FullPath, liveReload, contentTypes, loggerProvider);
                await server.StartAsync();
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, $"Error while running preview server: {ex}");
                return null;
            }

            string urlPath = server.VirtualDirectory ?? string.Empty;
            logger.LogInformation($"Preview server listening at http://localhost:{port}{urlPath} and serving from path {path}"
                + (liveReload ? " with LiveReload support" : string.Empty));
            return server;
        }
    }
}
