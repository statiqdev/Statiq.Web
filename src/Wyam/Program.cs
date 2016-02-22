using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.Hosting.Tracing;
using Microsoft.Owin.StaticFiles;
using Owin;
using Wyam.Common.IO;
using Wyam.Common.Tracing;
using Wyam.Core;
using Wyam.Owin;
using IFileSystem = Microsoft.Owin.FileSystems.IFileSystem;
using Wyam.Core.Meta;

namespace Wyam
{
    public class Program
    {
        static int Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionEvent;
            Program program = new Program();
            return program.Run(args);
        }

        static void UnhandledExceptionEvent(object sender, UnhandledExceptionEventArgs e)
        {
            // Exit with a error exit code
            Environment.Exit((int)ExitCode.UnhandledError);
        }

        private bool _watch = false;
        private bool _noClean = false;
        private bool _noCache = false;
        private bool _preview = false;
        private int _previewPort = 5080;
        private bool _previewForceExtension = false;
        private FilePath _logFilePath = null;
        private bool _verbose = false;
        private bool _pause = false;
        private bool _updatePackages = false;
        private bool _outputScripts = false;
        private bool _verifyConfig = false;
        private string _stdin = null;
        private DirectoryPath _rootPath = null;
        private DirectoryPath _inputPath = null;
        private DirectoryPath _outputPath = null;
        private DirectoryPath _previewRoot = null;
        private FilePath _configFilePath = null;
        private IReadOnlyList<string> _globalRawMetadata = null;
        private System.Diagnostics.ConsoleTraceListener _engineListener = null;

        private readonly ConcurrentQueue<string> _changedFiles = new ConcurrentQueue<string>();
        private readonly AutoResetEvent _messageEvent = new AutoResetEvent(false);
        private readonly InterlockedBool _exit = new InterlockedBool(false);
        private readonly InterlockedBool _newEngine = new InterlockedBool(false);

        private int Run(string[] args)
        {
            AssemblyInformationalVersionAttribute versionAttribute
                = Attribute.GetCustomAttribute(typeof(Program).Assembly, typeof(AssemblyInformationalVersionAttribute)) as AssemblyInformationalVersionAttribute;
            Console.WriteLine("Wyam version {0}", versionAttribute == null ? "unknown" : versionAttribute.InformationalVersion);

            // Parse the command line
            bool hasParseArgsErrors;
            if (!ParseArgs(args, out hasParseArgsErrors))
            {
                return hasParseArgsErrors ? (int)ExitCode.CommandLineError : (int)ExitCode.Normal;
            }

            // It's not a serious console app unless there's some ASCII art
            OutputLogo();

            // Fix the root folder and other files
            DirectoryPath currentDirectory = Environment.CurrentDirectory;
            _rootPath = _rootPath == null ? currentDirectory : currentDirectory.Combine(_rootPath);
            _logFilePath = _logFilePath == null ? null : _rootPath.CombineFile(_logFilePath);
            _configFilePath = _rootPath.CombineFile(_configFilePath ?? "config.wyam");

            // Get the engine
            Engine engine = GetEngine();
            if (engine == null)
            {
                return (int)ExitCode.CommandLineError;
            }

            // Populate engine's metadata
            if (!_verifyConfig && _globalRawMetadata != null && _globalRawMetadata.Count > 0)
            {
                try {
                    engine.GlobalMetadata = new GlobalMetadataParser().Parse(_globalRawMetadata);
                }
                catch (MetadataParseException ex)
                {
                    Trace.Error("Error while parsing metadata: {0}", ex.Message);
                    if (Trace.Level == System.Diagnostics.SourceLevels.Verbose)
                        Trace.Error("Stack trace:{0}{1}", Environment.NewLine, ex.StackTrace);

                    return (int)ExitCode.CommandLineError;
                }
                // Not used anymore, release resources.
                _globalRawMetadata = null;
            }

            // Pause
            if (_pause)
            {
                Trace.Information("Pause requested, hit any key to continue");
                Console.ReadKey();
            }

            // Configure and execute
            if (!Configure(engine))
            {
                return (int)ExitCode.ConfigurationError;
            }

            if (_verifyConfig)
            {
                Trace.Information("No errors. Exiting.");
                return (int)ExitCode.Normal;
            }

            Console.WriteLine($"Root path:{Environment.NewLine}  {engine.FileSystem.RootPath}");
            Console.WriteLine($"Input path(s):{Environment.NewLine}  {string.Join(Environment.NewLine + "  ", engine.FileSystem.InputPaths)}");
            Console.WriteLine($"Output path:{Environment.NewLine}  {engine.FileSystem.OutputPath}");
            if (!Execute(engine))
            {
                return (int)ExitCode.ExecutionError;
            }

            bool messagePump = false;

            // Start the preview server
            IDisposable previewServer = null;
            if (_preview)
            {
                messagePump = true;
                try
                {
                    var rootPath = _previewRoot == null ? engine.FileSystem.GetOutputDirectory().Path.FullPath : _previewRoot.FullPath;
                    Trace.Information("Preview server listening on port {0} and serving from path {1}", _previewPort, rootPath);
                    previewServer = Preview(engine, rootPath);
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

                Trace.Information("Watching paths(s) {0}", string.Join(", ", engine.FileSystem.InputPaths));
                inputFolderWatcher = new ActionFileSystemWatcher(engine.FileSystem.GetOutputDirectory().Path,
                    engine.FileSystem.GetInputDirectories().Select(x => x.Path), true, "*.*", path =>
                    {
                        _changedFiles.Enqueue(path);
                        _messageEvent.Set();
                    });

                if (_configFilePath != null)
                {
                    Trace.Information("Watching configuration file {0}", _configFilePath);
                    Engine closureEngine = engine;
                    configFileWatcher = new ActionFileSystemWatcher(engine.FileSystem.GetOutputDirectory().Path,
                        new[] { _configFilePath.GetDirectory() }, false, _configFilePath.GetFilename().FullPath, path =>
                        {
                            if (closureEngine.FileSystem.PathComparer.Equals((FilePath)path, _configFilePath))
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
                // Start the key listening thread
                Trace.Information("Hit any key to exit");
                var thread = new Thread(() =>
                {
                    Console.ReadKey();
                    _exit.Set();
                    _messageEvent.Set();
                })
                {
                    IsBackground = true
                };
                thread.Start();

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
                        Trace.Information("Configuration file {0} has changed, re-running", _configFilePath);
                        engine.Dispose();
                        engine = GetEngine();

                        // Configure and execute
                        if (!Configure(engine))
                        {
                            exitCode = ExitCode.ConfigurationError;
                            break;
                        }
                        Console.WriteLine($"Root path:{Environment.NewLine}  {engine.FileSystem.RootPath}");
                        Console.WriteLine($"Input path(s):{Environment.NewLine}  {string.Join(Environment.NewLine + "  ", engine.FileSystem.InputPaths)}");
                        Console.WriteLine($"Root path:{Environment.NewLine}  {engine.FileSystem.OutputPath}");
                        if (!Execute(engine))
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
                            if (!Execute(engine))
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
                engine.Dispose();
                inputFolderWatcher?.Dispose();
                configFileWatcher?.Dispose();
                previewServer?.Dispose();
            }
            return (int)exitCode;
        }

        private bool ParseArgs(string[] args, out bool hasErrors)
        {
            System.CommandLine.ArgumentSyntax parsed = System.CommandLine.ArgumentSyntax.Parse(args, syntax =>
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
                syntax.DefineOption("i|input", ref _inputPath, DirectoryPath.FromString, "The path of input files, can be absolute or relative to the current folder.");
                syntax.DefineOption("verify-config", ref _verifyConfig, false, "Compile the configuration but do not execute.");
                syntax.DefineOption("o|output", ref _outputPath, DirectoryPath.FromString, "The path to output files, can be absolute or relative to the current folder.");
                syntax.DefineOption("c|config", ref _configFilePath, FilePath.FromString, "Configuration file (by default, config.wyam is used).");
                syntax.DefineOption("u|update-packages", ref _updatePackages, "Check the NuGet server for more recent versions of each package and update them if applicable.");
                syntax.DefineOption("output-scripts", ref _outputScripts, "Outputs the config scripts after they've been processed for further debugging.");
                syntax.DefineOption("noclean", ref _noClean, "Prevents cleaning of the output path on each execution.");
                syntax.DefineOption("nocache", ref _noCache, "Prevents caching information during execution (less memory usage but slower execution).");
                syntax.DefineOption("v|verbose", ref _verbose, "Turns on verbose output showing additional trace message useful for debugging.");
                syntax.DefineOption("pause", ref _pause, "Pause execution at the start of the program until a key is pressed (useful for attaching a debugger).");
                syntax.DefineOptionList("meta", ref _globalRawMetadata, "Specifies global metadata which can be accessed from the engine or config file (--meta key=value).");
                _logFilePath = $"wyam-{DateTime.Now:yyyyMMddHHmmssfff}.txt";
                if (!syntax.DefineOption("l|log", ref _logFilePath, FilePath.FromString, false, "Log all trace messages to the specified log file (by default, wyam-[datetime].txt).").IsSpecified)
                {
                    _logFilePath = null;
                }
                if (syntax.DefineParameter("root", ref _rootPath, DirectoryPath.FromString, "The folder (or config file) to use.").IsSpecified)
                {
                    // If a root folder was defined, but it actually points to a file, set the root folder to the directory
                    // and use the specified file as the config file (if a config file was already specified, it's an error)
                    FilePath rootDirectoryPathAsConfigFile = new DirectoryPath(Environment.CurrentDirectory).CombineFile(_rootPath.FullPath);
                    if (File.Exists(rootDirectoryPathAsConfigFile.FullPath))
                    {
                        // The specified root actually points to a file...
                        if (_configFilePath != null)
                        {
                            syntax.ReportError("A config file was both explicitly specified and specified in the root folder.");
                        }
                        else
                        {
                            _configFilePath = rootDirectoryPathAsConfigFile.GetFilename();
                            _rootPath = rootDirectoryPathAsConfigFile.GetDirectory();
                        }
                    }
                }
            });

            hasErrors = parsed.HasErrors;

            if (Console.IsInputRedirected)
            {
                using (var reader = new StreamReader(Console.OpenStandardInput(), Console.InputEncoding))
                {
                    _stdin = reader.ReadToEnd();
                }
            }

            return !(parsed.IsHelpRequested() || hasErrors);
        }

        private Engine GetEngine()
        {
            try
            {
                Engine engine = new Engine();

                // Add a default trace listener
                if (_engineListener != null)
                    Trace.RemoveListener(_engineListener);
                _engineListener = new SimpleColorConsoleTraceListener() { TraceOutputOptions = System.Diagnostics.TraceOptions.None };
                Trace.AddListener(_engineListener);

                // Set verbose tracing
                if (_verbose)
                {
                    Trace.Level = System.Diagnostics.SourceLevels.Verbose;
                }

                // Set no cache if requested
                if (_noCache)
                {
                    engine.NoCache = true;
                }

                // Set folders
                engine.FileSystem.RootPath = _rootPath;
                if (_inputPath != null)
                {
                    engine.FileSystem.InputPaths.Add(_inputPath);
                }
                if (_outputPath != null)
                {
                    engine.FileSystem.OutputPath = _outputPath;
                }
                if (_noClean)
                {
                    engine.CleanOutputPathOnExecute = false;
                }

                engine.ApplicationInput = _stdin;

                // Set up the log file         
                if (_logFilePath != null)
                {
                    Trace.AddListener(new SimpleFileTraceListener(_logFilePath.FullPath));
                }

                return engine;
            }
            catch (Exception ex)
            {
                Trace.Critical("Error while instantiating engine: {0}", ex.Message);
                return null;
            }
        }

        private bool Configure(Engine engine)
        {
            try
            {
                // If we have a configuration file use it, otherwise configure with defaults  
                IFile configFile = engine.FileSystem.GetRootFile(_configFilePath);
                if (configFile.Exists)
                {
                    Trace.Information("Loading configuration from {0}", configFile.Path);
                    engine.Configure(configFile, _updatePackages, _outputScripts);
                }
                else
                {
                    Trace.Information("Could not find configuration file {0}, using default configuration", _configFilePath);
                    engine.Configure(GetDefaultConfigScript(), _updatePackages);
                }
            }
            catch (Exception ex)
            {
                Trace.Critical("Error while loading configuration: {0}", ex.Message);
                return false;
            }

            return true;
        }

        private bool Execute(Engine engine)
        {
            try
            {
                engine.Execute();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private IDisposable Preview(Engine engine, string root)
        {
            StartOptions options = new StartOptions("http://localhost:" + _previewPort);

            // Disable built-in owin tracing by using a null trace output
            // http://stackoverflow.com/questions/17948363/tracelistener-in-owin-self-hosting
            options.Settings.Add(typeof(ITraceOutputFactory).FullName, typeof(NullTraceOutputFactory).AssemblyQualifiedName);

            return WebApp.Start(options, app =>
            {
                IFileSystem outputFolder = new PhysicalFileSystem(root);

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

        // This is a hack until recipes are implemented, at which point it should be removed
        private string GetDefaultConfigScript()
        {
            return @"
                Pipelines.Add(""Content"",
	                ReadFiles(""*.md""),
	                FrontMatter(Yaml()),
	                Markdown(),
	                Concat(
		                ReadFiles(""*.cshtml"").Where(x => Path.GetFileName(x)[0] != '_'),
		                FrontMatter(Yaml())		
	                ),
	                Razor(),
	                WriteFiles("".html"")
                );

                Pipelines.Add(""Resources"",
	                CopyFiles(""*"").Where(x => Path.GetExtension(x) != "".cshtml"" && Path.GetExtension(x) != "".md"")
                );
            ";
        }

        private void OutputLogo()
        {
            Console.WriteLine(@"
   ,@@@@@       /@\        @@@@@       |                                        
   @@@@@@      @@@@@|     $@@@@@h      |                                        
  $@@@@@     ,@@@@@@@    g@@@@@P       |                                        
 ]@@@@@M    g@@@@@@@    g@@@@@P        |     @@P  @@@ ,@@%@  g$r,g@p   ,@@   ,@g
 $@@@@@    @@@@@@@@@   g@@@@@P         |    ]@@ ,@@@ ,$@` $@@@ g@P$@  ,@@@gg@@@@
j@@@@@   g@@@@@@@@@p ,@@@@@@@          |    $@g@@@9@@@@`  g@P g@$@$@@,@@ *P^`]@h
$@@@@@g@@@@@@@@B@@@@@@@@@@@P           |     *R^`  `BP   ?@`  B`  ?0` 0      ?P 
`$@@@@@@@@@@@`  ]@@@@@@@@@`            |                                        
  $@@@@@@@P`     ?$@@@@@P              |                                        
    `^``           *P*`                |                                        ");
        }
    }
}
