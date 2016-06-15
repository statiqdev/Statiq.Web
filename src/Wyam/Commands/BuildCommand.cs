using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.CommandLine;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.Hosting.Tracing;
using Microsoft.Owin.StaticFiles;
using Owin;
using Wyam.Common.IO;
using Wyam.Configuration.Preprocessing;
using Wyam.Owin;
using Trace = Wyam.Common.Tracing.Trace;

namespace Wyam.Commands
{
    internal class BuildCommand : Command
    {
        private readonly ConcurrentQueue<string> _changedFiles = new ConcurrentQueue<string>();
        private readonly AutoResetEvent _messageEvent = new AutoResetEvent(false);
        private readonly InterlockedBool _exit = new InterlockedBool(false);
        private readonly InterlockedBool _newEngine = new InterlockedBool(false);
        
        private bool _preview = false;
        private int _previewPort = 5080;
        private bool _previewForceExtension = false;
        private FilePath _logFilePath = null;
        private bool _verifyConfig = false;
        private DirectoryPath _previewRoot = null;
        private bool _watch = false;

        public bool NoClean = false;
        public bool NoCache = false;
        public bool UpdatePackages = false;
        public bool UseLocalPackages = false;
        public bool UseGlobalSources = false;
        public DirectoryPath PackagesPath = null;
        public bool OutputScript = false;
        public string Stdin = null;
        public DirectoryPath RootPath = null;
        public IReadOnlyList<DirectoryPath> InputPaths = null;
        public DirectoryPath OutputPath = null;
        public FilePath ConfigFilePath = null;
        public IReadOnlyDictionary<string, object> GlobalMetadata = null;
        public IReadOnlyDictionary<string, object> InitialMetadata = null;

        public override string Description => "Runs the build process (this is the default command).";

        public override string[] SupportedDirectives => new[]
        {
            "nuget",
            "nuget-source",
            "assembly",
            "assembly-name",
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
            if (syntax.DefineOption("preview-root", ref _previewRoot, DirectoryPath.FromString, "The path to the root of the preview server, if not the output folder.").IsSpecified && !_preview)
            {
                syntax.ReportError("preview-root can only be specified if the preview server is running.");
            }
            syntax.DefineOptionList("i|input", ref InputPaths, DirectoryPath.FromString, "The path(s) of input files, can be absolute or relative to the current folder.");
            syntax.DefineOption("o|output", ref OutputPath, DirectoryPath.FromString, "The path to output files, can be absolute or relative to the current folder.");
            syntax.DefineOption("c|config", ref ConfigFilePath, FilePath.FromString, "Configuration file (by default, config.wyam is used).");
            syntax.DefineOption("u|update-packages", ref UpdatePackages, "Check the NuGet server for more recent versions of each package and update them if applicable.");
            syntax.DefineOption("use-local-packages", ref UseLocalPackages, "Toggles the use of a local NuGet packages folder.");
            syntax.DefineOption("use-global-sources", ref UseGlobalSources, "Toggles the use of the global NuGet sources (default is false).");
            syntax.DefineOption("packages-path", ref PackagesPath, DirectoryPath.FromString, "The packages path to use (only if use-local is true).");
            syntax.DefineOption("output-script", ref OutputScript, "Outputs the config script after it's been processed for further debugging.");
            syntax.DefineOption("verify-config", ref _verifyConfig, false, "Compile the configuration but do not execute.");
            syntax.DefineOption("noclean", ref NoClean, "Prevents cleaning of the output path on each execution.");
            syntax.DefineOption("nocache", ref NoCache, "Prevents caching information during execution (less memory usage but slower execution).");

            _logFilePath = $"wyam-{DateTime.Now:yyyyMMddHHmmssfff}.txt";
            if (!syntax.DefineOption("l|log", ref _logFilePath, FilePath.FromString, false, "Log all trace messages to the specified log file (by default, wyam-[datetime].txt).").IsSpecified)
            {
                _logFilePath = null;
            }

            // Metadata
            IReadOnlyList<string> globalMetadata = null;
            if (syntax.DefineOptionList("g|global", ref globalMetadata, "Specifies global metadata as a sequence of key=value pairs.").IsSpecified)
            {
                GlobalMetadata = MetadataParser.Parse(globalMetadata);
            }
            IReadOnlyList<string> initialMetadata = null;
            if (syntax.DefineOptionList("initial", ref initialMetadata, "Specifies initial document metadata as a sequence of key=value pairs.").IsSpecified)
            {
                InitialMetadata = MetadataParser.Parse(initialMetadata);
            }
        }

        protected override void ParseParameters(ArgumentSyntax syntax)
        {
            // Root
            if (syntax.DefineParameter("root", ref RootPath, DirectoryPath.FromString, "The folder (or config file) to use.").IsSpecified)
            {
                // If a root folder was defined, but it actually points to a file, set the root folder to the directory
                // and use the specified file as the config file (if a config file was already specified, it's an error)
                FilePath rootDirectoryPathAsConfigFile = new DirectoryPath(Environment.CurrentDirectory).CombineFile(RootPath.FullPath);
                if (File.Exists(rootDirectoryPathAsConfigFile.FullPath))
                {
                    // The specified root actually points to a file...
                    if (ConfigFilePath != null)
                    {
                        syntax.ReportError("A config file was both explicitly specified and specified in the root folder.");
                    }
                    else
                    {
                        ConfigFilePath = rootDirectoryPathAsConfigFile.FileName;
                        RootPath = rootDirectoryPathAsConfigFile.Directory;
                    }
                }
            }
        }

        protected override ExitCode RunCommand(Preprocessor preprocessor)
        {
            // Get the standard input stream
            Stdin = StandardInputReader.Read();
            
            // Fix the root folder and other files
            DirectoryPath currentDirectory = Environment.CurrentDirectory;
            RootPath = RootPath == null ? currentDirectory : currentDirectory.Combine(RootPath);
            _logFilePath = _logFilePath == null ? null : RootPath.CombineFile(_logFilePath);
            ConfigFilePath = RootPath.CombineFile(ConfigFilePath ?? "config.wyam");

            // Set up the log file         
            if (_logFilePath != null)
            {
                Trace.AddListener(new SimpleFileTraceListener(_logFilePath.FullPath));
            }

            // Get the engine and configurator
            EngineManager engineManager = GetEngineManager(preprocessor);
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
                try
                {
                    DirectoryPath previewPath = _previewRoot == null
                        ? engineManager.Engine.FileSystem.GetOutputDirectory().Path
                        : engineManager.Engine.FileSystem.GetOutputDirectory(_previewRoot).Path;
                    Trace.Information("Preview server listening on port {0} and serving from path {1}", _previewPort, previewPath);
                    previewServer = GetPreviewServer(previewPath);
                }
                catch (Exception ex)
                {
                    Trace.Critical("Error while running preview server: {0}", ex.Message);
                }
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

                if (ConfigFilePath != null)
                {
                    Trace.Information("Watching configuration file {0}", ConfigFilePath);
                    configFileWatcher = new ActionFileSystemWatcher(engineManager.Engine.FileSystem.GetOutputDirectory().Path,
                        new[] { ConfigFilePath.Directory }, false, ConfigFilePath.FileName.FullPath, path =>
                        {
                            FilePath filePath = new FilePath(path);
                            if (ConfigFilePath.Equals(filePath))
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
                        Trace.Information("Configuration file {0} has changed, re-running", ConfigFilePath);
                        engineManager.Dispose();
                        engineManager = GetEngineManager(preprocessor);

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

        private EngineManager GetEngineManager(Preprocessor preprocessor)
        {
            try
            {
                return new EngineManager(preprocessor, this);
            }
            catch (Exception ex)
            {
                Trace.Critical("Error while instantiating engine: {0}", ex.Message);
                return null;
            }
        }

        private IDisposable GetPreviewServer(DirectoryPath root)
        {
            StartOptions options = new StartOptions("http://localhost:" + _previewPort);

            // Disable built-in owin tracing by using a null trace output
            // http://stackoverflow.com/questions/17948363/tracelistener-in-owin-self-hosting
            options.Settings.Add(typeof(ITraceOutputFactory).FullName, typeof(NullTraceOutputFactory).AssemblyQualifiedName);

            return WebApp.Start(options, app =>
            {
                Microsoft.Owin.FileSystems.IFileSystem outputFolder = new PhysicalFileSystem(root.FullPath);

                // Disable caching
                app.Use((c, t) =>
                {
                    c.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
                    c.Response.Headers.Append("Pragma", "no-cache");
                    c.Response.Headers.Append("Expires", "0");
                    return t();
                });

                // Support for extensionless URLs
                if (!_previewForceExtension)
                {
                    app.UseExtensionlessUrls(new ExtensionlessUrlsOptions
                    {
                        FileSystem = outputFolder
                    });
                }

                // Serve up all static files
                app.UseDefaultFiles(new DefaultFilesOptions
                {
                    RequestPath = PathString.Empty,
                    FileSystem = outputFolder,
                    DefaultFileNames = new List<string> { "index.html", "index.htm", "home.html", "home.htm", "default.html", "default.html" }
                });
                app.UseStaticFiles(new StaticFileOptions
                {
                    RequestPath = PathString.Empty,
                    FileSystem = outputFolder,
                    ServeUnknownFileTypes = true
                });
            });
        }

        private class NullTraceOutputFactory : ITraceOutputFactory
        {
            public TextWriter Create(string outputFile)
            {
                return StreamWriter.Null;
            }
        }
    }
}