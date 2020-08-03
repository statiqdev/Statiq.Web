using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
    [Description("Serves a folder, optionally watching for changes and triggering client reload by default.")]
    internal class ServeCommand : EngineCommand<ServeCommandSettings>
    {
        private readonly ConcurrentQueue<string> _changedFiles = new ConcurrentQueue<string>();
        private readonly AutoResetEvent _messageEvent = new AutoResetEvent(false);
        private readonly InterlockedBool _exit = new InterlockedBool(false);

        public ServeCommand(
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
            ServeCommandSettings commandSettings,
            IEngineManager engineManager)
        {
            ExitCode exitCode = ExitCode.Normal;
            ILogger logger = engineManager.Engine.Services.GetRequiredService<ILogger<Bootstrapper>>();

            // Set folders
            IFileSystem fileSystem = engineManager.Engine.FileSystem;
            NormalizedPath currentDirectory = Environment.CurrentDirectory;
            IDirectory serveDirectory;
            if (string.IsNullOrEmpty(commandSettings.RootPath))
            {
                fileSystem.RootPath = currentDirectory;
                serveDirectory = fileSystem.GetOutputDirectory();
            }
            else
            {
                fileSystem.RootPath = currentDirectory.Combine(commandSettings.RootPath);
                serveDirectory = fileSystem.GetRootDirectory();
            }

            Dictionary<string, string> contentTypes = commandSettings.ContentTypes?.Length > 0
                ? PreviewCommand.GetContentTypes(commandSettings.ContentTypes)
                : new Dictionary<string, string>();
            ILoggerProvider loggerProvider = engineManager.Engine.Services.GetRequiredService<ILoggerProvider>();

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
                    contentTypes,
                    loggerProvider,
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
                        logger.LogInformation($"{changedFiles.Count} files have changed, re-loading");
                        await previewServer.TriggerReloadAsync();
                    }

                    // Check one more time for exit
                    if (_exit)
                    {
                        break;
                    }
                    logger.LogInformation("Hit Ctrl-C to exit");
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
