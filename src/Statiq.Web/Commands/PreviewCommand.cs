using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
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
    public class PreviewCommand : PipelinesCommand<PreviewCommandSettings>
    {
        private readonly ConcurrentQueue<string> _changedFiles = new ConcurrentQueue<string>();
        private readonly AutoResetEvent _messageEvent = new AutoResetEvent(false);
        private readonly InterlockedBool _exit = new InterlockedBool(false);

        private readonly ResetCacheMetadataValue _resetCache = new ResetCacheMetadataValue();

        private ScriptOptions _scriptOptions;
        private ScriptState<object> _scriptState;

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

        protected override async Task<int> ExecuteEngineAsync(
            CommandContext commandContext,
            PreviewCommandSettings commandSettings,
            IEngineManager engineManager)
        {
            using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
            {
                SetPipelines(commandContext, commandSettings, engineManager);

                ExitCode exitCode = ExitCode.Normal;
                ILogger logger = engineManager.Engine.Services.GetRequiredService<ILogger<Bootstrapper>>();

                // Start the console listener
                ConsoleListener consoleListener = new ConsoleListener(
                    () => OnExitAsync(cancellationTokenSource),
                    input => EvaluateScriptAsync(input, engineManager, cancellationTokenSource));

                // Execute the engine for the first time
                exitCode = await engineManager.ExecuteAsync(cancellationTokenSource);

                // Start previewing if we didn't cancel
                ActionFileSystemWatcher inputFolderWatcher = null;
                Server previewServer = null;
                if (exitCode != ExitCode.OperationCanceled)
                {
                    // Start the preview server
                    Dictionary<string, string> contentTypes = commandSettings.ContentTypes?.Length > 0
                        ? GetContentTypes(commandSettings.ContentTypes)
                        : new Dictionary<string, string>();
                    IEnumerable<ILoggerProvider> loggerProviders = engineManager.Engine.Services.GetServices<ILoggerProvider>();
                    IDirectory outputDirectory = engineManager.Engine.FileSystem.GetOutputDirectory();
                    if (outputDirectory.Exists)
                    {
                        previewServer = await StartPreviewServerAsync(
                            outputDirectory.Path,
                            commandSettings.Port,
                            commandSettings.ForceExt,
                            commandSettings.VirtualDirectory,
                            !commandSettings.NoReload,
                            contentTypes,
                            loggerProviders,
                            logger);
                    }

                    // Start the watchers
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

                    // Log that we're ready and start waiting on input
                    logger.LogInformation("Hit Ctrl-C to exit");
                    ConsoleLoggerProvider.FlushAndWait();
                    consoleListener.StartReadingLines();

                    // Wait for activity
                    while (true)
                    {
                        _messageEvent.WaitOne(); // Blocks the current thread until a signal
                        if (_exit)
                        {
                            break;
                        }

                        // Stop listening while we run again
                        consoleListener.StopReadingLines();

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
                            bool? existingResetCacheSetting = null;
                            bool setResetCacheSetting = false;
                            if (exitCode != ExitCode.Normal)
                            {
                                existingResetCacheSetting = engineManager.Engine.Settings.ContainsKey(Keys.ResetCache)
                                    ? engineManager.Engine.Settings.GetBool(Keys.ResetCache)
                                    : (bool?)null;
                                setResetCacheSetting = true;
                                _resetCache.Value = true;
                            }

                            // If there was an execution error due to reload, keep previewing but clear the cache
                            exitCode = await engineManager.ExecuteAsync(cancellationTokenSource);

                            // Reset the reset cache setting after removing it
                            if (setResetCacheSetting)
                            {
                                _resetCache.Value = existingResetCacheSetting ?? false;
                            }

                            if (previewServer is null)
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
                                        loggerProviders,
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
                        _messageEvent.Reset();

                        // Log that we're ready and start waiting on input (again)
                        logger.LogInformation("Hit Ctrl-C to exit");
                        ConsoleLoggerProvider.FlushAndWait();
                        consoleListener.StartReadingLines();
                    }
                }

                // Shutdown
                logger.LogInformation("Shutting down");
                inputFolderWatcher?.Dispose();
                previewServer?.Dispose();

                return (int)exitCode;
            }
        }

        private Task OnExitAsync(CancellationTokenSource cancellationTokenSource)
        {
            _exit.Set();
            _messageEvent.Set();
            cancellationTokenSource.Cancel();
            return Task.CompletedTask;
        }

        private async Task EvaluateScriptAsync(string code, IEngineManager engineManager, CancellationTokenSource cancellationTokenSource)
        {
            if (!code.IsNullOrWhiteSpace())
            {
                // Create the script options
                if (_scriptOptions is null)
                {
                    IEnumerable<MetadataReference> references = engineManager.Engine.ScriptHelper
                        .GetScriptReferences().Select(x => MetadataReference.CreateFromFile(x.Location));
                    _scriptOptions = ScriptOptions.Default
                        .WithReferences(references)
                        .WithImports(engineManager.Engine.ScriptHelper.GetScriptNamespaces());
                }

                // Run the script
                try
                {
                    if (_scriptState is null)
                    {
                        ScriptGlobals scriptGlobals = new ScriptGlobals(engineManager.Engine, () => OnExitAsync(cancellationTokenSource).GetAwaiter().GetResult());
                        _scriptState = await CSharpScript.RunAsync(code, _scriptOptions, globals: scriptGlobals, cancellationToken: cancellationTokenSource.Token);
                    }
                    else
                    {
                        _scriptState = await _scriptState.ContinueWithAsync(code, _scriptOptions, cancellationToken: cancellationTokenSource.Token);
                    }
                }
                catch (CompilationErrorException e)
                {
                    Console.WriteLine(string.Join(Environment.NewLine, e.Diagnostics));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

                // Output the result (if any)
                if (_scriptState?.ReturnValue is object && TypeHelper.TryConvert(_scriptState.ReturnValue, out string result))
                {
                    Console.WriteLine(result);
                }
            }
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
    }
}
