using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Threading;
using Wyam.Common.IO;
using Wyam.Configuration.Preprocessing;
using Trace = Wyam.Common.Tracing.Trace;

namespace Wyam.Commands
{
    internal class BuildCommand : Command
    {
        private readonly ConcurrentQueue<string> _changedFiles = new ConcurrentQueue<string>();
        private readonly AutoResetEvent _messageEvent = new AutoResetEvent(false);
        private readonly InterlockedBool _exit = new InterlockedBool(false);
        private readonly InterlockedBool _newEngine = new InterlockedBool(false);
        private readonly ConfigOptions _configOptions = new ConfigOptions();
        
        private bool _preview = false;
        private int _previewPort = 5080;
        private DirectoryPath _previewVirtualDirectory = null;
        private bool _previewForceExtension = false;
        private FilePath _logFilePath = null;
        private bool _verifyConfig = false;
        private DirectoryPath _previewRoot = null;
        private bool _watch = false;
        
        public override string Description => "Runs the build process (this is the default command).";

        public override string[] SupportedDirectives => new[]
        {
            "nuget",
            "nuget-source",
            "assembly",
            "recipe",
            "theme"
        };

        protected override void ParseOptions(ArgumentSyntax syntax)
        {
            syntax.DefineOption("w|watch", ref _watch, "Watches the input folder for any changes.");
            _preview = syntax.DefineOption("p|preview", ref _previewPort, false, "Start the preview web server on the specified port (default is " + _previewPort + ").").IsSpecified;
            if (syntax.DefineOption("force-ext", ref _previewForceExtension, "Force the use of extensions in the preview web server (by default, extensionless URLs may be used).").IsSpecified && !_preview)
            {
                syntax.ReportError("force-ext can only be specified if the preview server is running.");
            }
            if (syntax.DefineOption("virtual-dir", ref _previewVirtualDirectory, DirectoryPath.FromString, "Serve files in the preview web server under the specified virtual directory.").IsSpecified && !_preview)
            {
                syntax.ReportError("virtual-dir can only be specified if the preview server is running.");
            }
            if (syntax.DefineOption("preview-root", ref _previewRoot, DirectoryPath.FromString, "The path to the root of the preview server, if not the output folder.").IsSpecified && !_preview)
            {
                syntax.ReportError("preview-root can only be specified if the preview server is running.");
            }
            syntax.DefineOptionList("i|input", ref _configOptions.InputPaths, DirectoryPath.FromString, "The path(s) of input files, can be absolute or relative to the current folder.");
            syntax.DefineOption("o|output", ref _configOptions.OutputPath, DirectoryPath.FromString, "The path to output files, can be absolute or relative to the current folder.");
            syntax.DefineOption("c|config", ref _configOptions.ConfigFilePath, FilePath.FromString, "Configuration file (by default, config.wyam is used).");
            syntax.DefineOption("u|update-packages", ref _configOptions.UpdatePackages, "Check the NuGet server for more recent versions of each package and update them if applicable.");
            syntax.DefineOption("use-local-packages", ref _configOptions.UseLocalPackages, "Toggles the use of a local NuGet packages folder.");
            syntax.DefineOption("use-global-sources", ref _configOptions.UseGlobalSources, "Toggles the use of the global NuGet sources (default is false).");
            syntax.DefineOption("packages-path", ref _configOptions.PackagesPath, DirectoryPath.FromString, "The packages path to use (only if use-local is true).");
            syntax.DefineOption("output-script", ref _configOptions.OutputScript, "Outputs the config script after it's been processed for further debugging.");
            syntax.DefineOption("verify-config", ref _verifyConfig, false, "Compile the configuration but do not execute.");
            syntax.DefineOption("noclean", ref _configOptions.NoClean, "Prevents cleaning of the output path on each execution.");
            syntax.DefineOption("nocache", ref _configOptions.NoCache, "Prevents caching information during execution (less memory usage but slower execution).");

            _logFilePath = $"wyam-{DateTime.Now:yyyyMMddHHmmssfff}.txt";
            if (!syntax.DefineOption("l|log", ref _logFilePath, FilePath.FromString, false, "Log all trace messages to the specified log file (by default, wyam-[datetime].txt).").IsSpecified)
            {
                _logFilePath = null;
            }

            // Metadata
            // TODO: Remove this dictionary and initial/global options
            Dictionary<string, object> settingsDictionary = new Dictionary<string, object>();
            IReadOnlyList<string> globalMetadata = null;
            if (syntax.DefineOptionList("g|global", ref globalMetadata, "Deprecated, do not use.").IsSpecified)
            {
                Trace.Warning("-g/--global is deprecated and will be removed in a future version. Please use -s/--setting instead.");
                AddSettings(settingsDictionary, globalMetadata);
            }
            IReadOnlyList<string> initialMetadata = null;
            if (syntax.DefineOptionList("initial", ref initialMetadata, "Deprecated, do not use.").IsSpecified)
            {
                Trace.Warning("--initial is deprecated and will be removed in a future version. Please use -s/--setting instead.");
                AddSettings(settingsDictionary, initialMetadata);
            }
            IReadOnlyList<string> settings = null;
            if (syntax.DefineOptionList("s|setting", ref settings, "Specifies a setting as a key=value pair. Use the syntax [x,y] to specify an array value.").IsSpecified)
            {
                // _configOptions.Settings = MetadataParser.Parse(settings); TODO: Use this when AddSettings() is removed

                AddSettings(settingsDictionary, settings);
            }
            if (settingsDictionary.Count > 0)
            {
                _configOptions.Settings = settingsDictionary;
            }
        }

        // TODO: Remove this method and the global/initial support in the parser
        private static void AddSettings(IDictionary<string, object> settings, IReadOnlyList<string> value)
        {
            foreach (KeyValuePair<string, object> kvp in MetadataParser.Parse(value))
            {
               settings[kvp.Key] = kvp.Value;
            }
        }

        protected override void ParseParameters(ArgumentSyntax syntax)
        {
            ParseRootPathParameter(syntax, _configOptions);
        }

        protected override ExitCode RunCommand(Preprocessor preprocessor)
        {
            // Get the standard input stream
            _configOptions.Stdin = StandardInputReader.Read();
            
            // Fix the root folder and other files
            DirectoryPath currentDirectory = Environment.CurrentDirectory;
            _configOptions.RootPath = _configOptions.RootPath == null ? currentDirectory : currentDirectory.Combine(_configOptions.RootPath);
            _logFilePath = _logFilePath == null ? null : _configOptions.RootPath.CombineFile(_logFilePath);
            _configOptions.ConfigFilePath = _configOptions.RootPath.CombineFile(_configOptions.ConfigFilePath ?? "config.wyam");

            // Set up the log file         
            if (_logFilePath != null)
            {
                Trace.AddListener(new SimpleFileTraceListener(_logFilePath.FullPath));
            }

            // Get the engine and configurator
            EngineManager engineManager = EngineManager.Get(preprocessor, _configOptions);
            if (engineManager == null)
            {
                return ExitCode.CommandLineError;
            }

            // Configure and execute
            if (!engineManager.Configure())
            {
                return ExitCode.ConfigurationError;
            }

            if (_verifyConfig)
            {
                Trace.Information("No errors. Exiting.");
                return ExitCode.Normal;
            }

            Trace.Information($"Root path:{Environment.NewLine}  {engineManager.Engine.FileSystem.RootPath}");
            Trace.Information($"Input path(s):{Environment.NewLine}  {string.Join(Environment.NewLine + "  ", engineManager.Engine.FileSystem.InputPaths)}");
            Trace.Information($"Output path:{Environment.NewLine}  {engineManager.Engine.FileSystem.OutputPath}");
            if (!engineManager.Execute())
            {
                return ExitCode.ExecutionError;
            }

            bool messagePump = false;

            // Start the preview server
            IDisposable previewServer = null;
            if (_preview)
            {
                messagePump = true;
                DirectoryPath previewPath = _previewRoot == null
                    ? engineManager.Engine.FileSystem.GetOutputDirectory().Path
                    : engineManager.Engine.FileSystem.GetOutputDirectory(_previewRoot).Path;
                previewServer = PreviewServer.Start(previewPath, _previewPort, _previewForceExtension, _previewVirtualDirectory);
            }

            // Start the watchers
            IDisposable inputFolderWatcher = null;
            IDisposable configFileWatcher = null;
            if (_watch)
            {
                messagePump = true;

                Trace.Information("Watching paths(s) {0}", string.Join(", ", engineManager.Engine.FileSystem.InputPaths));
                inputFolderWatcher = new ActionFileSystemWatcher(engineManager.Engine.FileSystem.GetOutputDirectory().Path,
                    engineManager.Engine.FileSystem.GetInputDirectories().Select(x => x.Path), true, "*.*", path =>
                    {
                        _changedFiles.Enqueue(path);
                        _messageEvent.Set();
                    });

                if (_configOptions.ConfigFilePath != null)
                {
                    Trace.Information("Watching configuration file {0}", _configOptions.ConfigFilePath);
                    configFileWatcher = new ActionFileSystemWatcher(engineManager.Engine.FileSystem.GetOutputDirectory().Path,
                        new[] { _configOptions.ConfigFilePath.Directory }, false, _configOptions.ConfigFilePath.FileName.FullPath, path =>
                        {
                            FilePath filePath = new FilePath(path);
                            if (_configOptions.ConfigFilePath.Equals(filePath))
                            {
                                _newEngine.Set();
                                _messageEvent.Set();
                            }
                        });
                }
            }

            // Start the message pump if an async process is running
            ExitCode exitCode = ExitCode.Normal;
            if (messagePump)
            {
                // Only wait for a key if console input has not been redirected, otherwise it's on the caller to exit
                if (!Console.IsInputRedirected)
                {
                    // Start the key listening thread
                    var thread = new Thread(() =>
                    {
                        Trace.Information("Hit any key to exit");
                        Console.ReadKey();
                        _exit.Set();
                        _messageEvent.Set();
                    })
                    {
                        IsBackground = true
                    };
                    thread.Start();
                }

                // Wait for activity
                while (true)
                {
                    _messageEvent.WaitOne();  // Blocks the current thread until a signal
                    if (_exit)
                    {
                        break;
                    }

                    // See if we need a new engine
                    if (_newEngine)
                    {
                        // Get a new engine
                        Trace.Information("Configuration file {0} has changed, re-running", _configOptions.ConfigFilePath);
                        engineManager.Dispose();
                        engineManager = EngineManager.Get(preprocessor, _configOptions);

                        // Configure and execute
                        if (!engineManager.Configure())
                        {
                            exitCode = ExitCode.ConfigurationError;
                            break;
                        }
                        Console.WriteLine($"Root path:{Environment.NewLine}  {engineManager.Engine.FileSystem.RootPath}");
                        Console.WriteLine($"Input path(s):{Environment.NewLine}  {string.Join(Environment.NewLine + "  ", engineManager.Engine.FileSystem.InputPaths)}");
                        Console.WriteLine($"Root path:{Environment.NewLine}  {engineManager.Engine.FileSystem.OutputPath}");
                        if (!engineManager.Execute())
                        {
                            exitCode = ExitCode.ExecutionError;
                            break;
                        }

                        // Clear the changed files since we just re-ran
                        string changedFile;
                        while (_changedFiles.TryDequeue(out changedFile))
                        {
                        }

                        _newEngine.Unset();
                    }
                    else
                    {
                        // Execute if files have changed
                        HashSet<string> changedFiles = new HashSet<string>();
                        string changedFile;
                        while (_changedFiles.TryDequeue(out changedFile))
                        {
                            if (changedFiles.Add(changedFile))
                            {
                                Trace.Verbose("{0} has changed", changedFile);
                            }
                        }
                        if (changedFiles.Count > 0)
                        {
                            Trace.Information("{0} files have changed, re-executing", changedFiles.Count);
                            if (!engineManager.Execute())
                            {
                                exitCode = ExitCode.ExecutionError;
                                break;
                            }
                        }
                    }

                    // Check one more time for exit
                    if (_exit)
                    {
                        break;
                    }
                    Trace.Information("Hit any key to exit");
                    _messageEvent.Reset();
                }

                // Shutdown
                Trace.Information("Shutting down");
                engineManager.Dispose();
                inputFolderWatcher?.Dispose();
                configFileWatcher?.Dispose();
                previewServer?.Dispose();
            }

            return exitCode;
        }
    }
}