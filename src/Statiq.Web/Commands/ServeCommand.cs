using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
    [Description("Serves a folder, optionally watching for changes and triggering client reload by default.")]
    public class ServeCommand : EngineCommand<ServeCommandSettings>
    {
        private readonly ConcurrentQueue<string> _changedFiles = new ConcurrentQueue<string>();
        private readonly AutoResetEvent _messageEvent = new AutoResetEvent(false);
        private readonly InterlockedBool _exit = new InterlockedBool(false);
        private Dictionary<string, string> _contentTypes = new Dictionary<string, string>();
        private Dictionary<string, string> _customHeaders = new Dictionary<string, string>();

        public ServeCommand(
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
        }

        protected override async Task<int> ExecuteEngineAsync(
            CommandContext commandContext,
            ServeCommandSettings commandSettings,
            IEngineManager engineManager)
        {
            ExitCode exitCode = ExitCode.Normal;
            ILogger logger = engineManager.Engine.Services.GetRequiredService<ILogger<Bootstrapper>>();

            // Set folders
            NormalizedPath currentDirectory = Environment.CurrentDirectory;
            IDirectory serveDirectory;
            if (string.IsNullOrEmpty(commandSettings.ServePath))
            {
                serveDirectory = FileSystem.GetOutputDirectory();
            }
            else
            {
                FileSystem.RootPath = currentDirectory.Combine(commandSettings.ServePath);
                serveDirectory = FileSystem.GetRootDirectory();
            }

            // Get content types and custom headers from the settings and/or CLI
            PreviewCommand.GetKeyValues(
                _contentTypes,
                WebKeys.ServerContentTypes,
                Settings,
                engineManager.Engine,
                commandSettings.ContentTypes,
                "content type");
            PreviewCommand.GetKeyValues(
                _customHeaders,
                WebKeys.ServerCustomHeaders,
                Settings,
                engineManager.Engine,
                commandSettings.CustomHeaders,
                "custom header");

            // Start the preview server
            IEnumerable<ILoggerProvider> loggerProviders = engineManager.Engine.Services.GetServices<ILoggerProvider>();

            Server previewServer = null;
            FileSystemWatcher serveFolderWatcher = null;

            if (serveDirectory.Exists)
            {
                // Starts Preview server
                previewServer = await PreviewCommand.StartPreviewServerAsync(
                    serveDirectory.Path,
                    commandSettings.Port,
                    commandSettings.ForceExt,
                    commandSettings.VirtualDirectory,
                    !commandSettings.NoReload,
                    _contentTypes,
                    _customHeaders,
                    loggerProviders,
                    logger);

                // Start the watchers
                if (!commandSettings.NoReload)
                {
                    logger.LogInformation("Watching path {0}", serveDirectory.Path);
                    serveFolderWatcher = new FileSystemWatcher(serveDirectory.Path.FullPath, "*.*")
                    {
                        IncludeSubdirectories = true,
                        EnableRaisingEvents = true
                    };

                    serveFolderWatcher.Changed += OnFileChanged;
                    serveFolderWatcher.Created += OnFileChanged;
                }

                // Start the message pump
                new ConsoleListener(() =>
                {
                    _exit.Set();
                    _messageEvent.Set();
                    return Task.CompletedTask;
                });

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

                    // Use a setting to propagate the list of changed files
                    engineManager.Engine.Settings[WebKeys.ChangedFiles] = changedFiles;

                    // If files have changed, reload
                    if (changedFiles.Count > 0)
                    {
                        logger.LogInformation($"{changedFiles.Count} files have changed, re-loading");
                        await previewServer.TriggerReloadAsync();
                    }

                    // Check one more time for exit
                    if (_exit)
                    {
                        break;
                    }
                    logger.LogInformation("Type Ctrl-C to exit");
                    _messageEvent.Reset();
                }
            }
            else
            {
                logger.LogError($"Directory {serveDirectory.Path} does not exist.");
            }

            // Shutdown
            logger.LogInformation("Shutting down");

            if (serveFolderWatcher is object)
            {
                serveFolderWatcher.EnableRaisingEvents = false;
                serveFolderWatcher.Changed -= OnFileChanged;
                serveFolderWatcher.Created -= OnFileChanged;
                serveFolderWatcher.Dispose();
            }

            previewServer?.Dispose();

            return (int)exitCode;
        }

        private void OnFileChanged(object sender, FileSystemEventArgs args)
        {
            _changedFiles.Enqueue(args.FullPath);
            _messageEvent.Set();
        }
    }
}