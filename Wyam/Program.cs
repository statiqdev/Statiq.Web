using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.StaticFiles;
using Owin;
using Wyam.Core;
using Wyam.Owin;

namespace Wyam
{
    public class Program
    {
        static void Main(string[] args)
        {
            Program program = new Program();
            program.Run(args);
        }

        private bool _watch = false;
        private bool _noClean = false;
        private bool _preview = false;
        private int _previewPort = 5080;
        private bool _previewForceExtension = false;
        private string _logFile = null;
        private bool _verbose = false;
        private bool _pause = false;
        private bool _updatePackages = false;
        private string _rootFolder = null;
        private string _inputFolder = null;
        private string _outputFolder = null;
        private string _configFile = null;

        private readonly ConcurrentQueue<string> _changedFiles = new ConcurrentQueue<string>();
        private readonly AutoResetEvent _messageEvent = new AutoResetEvent(false);
        private readonly InterlockedBool _exit = new InterlockedBool(false);
        private readonly InterlockedBool _newEngine = new InterlockedBool(false);
        
        private void Run(string[] args)
        {
            // Parse the command line
            if (!ParseArgs(args))
            {
                return;
            }

            // Fix the root folder and other files
            _rootFolder = _rootFolder == null ? Environment.CurrentDirectory : Path.Combine(Environment.CurrentDirectory, _rootFolder);
            _logFile = _logFile == null ? null : Path.Combine(_rootFolder, _logFile);
            _configFile = string.IsNullOrWhiteSpace(_configFile)
                ? Path.Combine(_rootFolder, "config.wyam") : Path.Combine(_rootFolder, _configFile);

            // Get the engine
            Engine engine = GetEngine();
            if (engine == null)
            {
                return;
            }

            // Pause
            if (_pause)
            {
                engine.Trace.Information("Pause requested, hit any key to continue.");
                Console.ReadKey();
            }

            // Configure and execute
            if (!Configure(engine))
            {
                return;
            }
            if (!Execute(engine))
            {
                return;
            }

            bool messagePump = false;

            // Start the preview server
            IDisposable previewServer = null;
            if (_preview)
            {
                messagePump = true;
                try
                {
                    engine.Trace.Information("Preview server listening on port {0} and serving from {1}...", _previewPort, engine.OutputFolder);
                    previewServer = Preview(engine);
                }
                catch (Exception ex)
                {
                    engine.Trace.Critical("Error while running preview server: {0}.", ex.Message);
                }
            }

            // Start the watchers
            IDisposable inputFolderWatcher = null;
            IDisposable configFileWatcher = null;
            if (_watch)
            {
                messagePump = true;
                
                engine.Trace.Information("Watching folder {0}...", engine.InputFolder);
                inputFolderWatcher = new ActionFileSystemWatcher(engine.InputFolder, true, "*.*", path =>
                {
                    _changedFiles.Enqueue(path);
                    _messageEvent.Set();
                });

                if (_configFile != null)
                {
                    engine.Trace.Information("Watching configuration file {0}...", _configFile);
                    configFileWatcher = new ActionFileSystemWatcher(Path.GetDirectoryName(_configFile), false, Path.GetFileName(_configFile), path =>
                    {
                        if (path == _configFile)
                        {
                            _newEngine.Set();
                            _messageEvent.Set();
                        }
                    });
                }
            }

            // Start the message pump if an async process is running
            if (messagePump)
            {
                // Start the key listening thread
                engine.Trace.Information("Hit any key to exit...");
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
                        engine.Trace.Information("Configuration file {0} has changed, re-running...", _configFile);
                        engine = GetEngine();

                        // Configure and execute
                        if (!Configure(engine))
                        {
                            break;
                        }
                        if (!Execute(engine))
                        {
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
                                engine.Trace.Verbose("{0} has changed.", changedFile);
                            }
                        }
                        if (changedFiles.Count > 0)
                        {
                            engine.Trace.Information("{0} files have changed, re-executing...", changedFiles.Count);
                            if (!Execute(engine))
                            {
                                break;
                            }
                        }
                    }

                    // Check one more time for exit
                    if (_exit)
                    {
                        break;
                    }
                    engine.Trace.Information("Hit any key to exit...");
                    _messageEvent.Reset();
                }

                // Shutdown
                engine.Trace.Information("Shutting down...");
                if (inputFolderWatcher != null)
                {
                    inputFolderWatcher.Dispose();
                }
                if (configFileWatcher != null)
                {
                    configFileWatcher.Dispose();
                }
                if (previewServer != null)
                {
                    previewServer.Dispose();
                }
            }
        }

        // Very simple command line parsing
        private bool ParseArgs(string[] args)
        {
            for (int c = 0; c < args.Length; c++)
            {
                if (args[c] == "--watch")
                {
                    _watch = true;
                }
                else if (args[c] == "--no-clean")
                {
                    _noClean = true;
                }
                else if (args[c] == "--preview")
                {
                    _preview = true;
                    while (c + 1 < args.Length && !args[c + 1].StartsWith("--"))
                    {
                        if (args[c + 1] == "force-ext")
                        {
                            _previewForceExtension = true;
                            c++;
                        }
                        else if (!int.TryParse(args[c++], out _previewPort))
                        {
                            // Invalid port number
                            Help(true);
                            return false;
                        }
                    }
                }
                else if (args[c] == "--log")
                {
                    _logFile = string.Format("wyam-{0:yyyyMMddHHmmssfff}.txt", DateTime.Now);
                    if (c + 1 < args.Length && !args[c + 1].StartsWith("--"))
                    {
                        _logFile = args[++c];
                    }
                }
                else if (args[c] == "--input")
                {
                    if (c + 1 >= args.Length || args[c + 1].StartsWith("--"))
                    {
                        Help(true);
                        return false;
                    }
                    _inputFolder = args[++c];
                }
                else if (args[c] == "--output")
                {
                    if (c + 1 >= args.Length || args[c + 1].StartsWith("--"))
                    {
                        Help(true);
                        return false;
                    }
                    _outputFolder = args[++c];
                }
                else if (args[c] == "--config")
                {
                    if (c + 1 >= args.Length || args[c + 1].StartsWith("--"))
                    {
                        Help(true);
                        return false;
                    }
                    _configFile = args[++c];
                }
                else if (args[c] == "--update-packages")
                {
                    _updatePackages = true;
                }
                else if (args[c] == "--verbose")
                {
                    _verbose = true;
                }
                else if (args[c] == "--pause")
                {
                    _pause = true;
                }
                else if (args[c] == "--help")
                {
                    Help(false);
                    return false;
                }
                else if (c == 0 && !args[c].StartsWith("--"))
                {
                    _rootFolder = args[c];
                }
                else
                {
                    // Invalid argument
                    Help(true);
                    return false;
                }
            }
            return true;
        }

        private void Help(bool invalid)
        {
            if (invalid)
            {
                Console.WriteLine("Invalid arguments.");
            }
            Console.WriteLine("Usage: wyam.exe [path] [--input path] [--output path] [--config file] [--no-clean] [--update-packages] [--watch] [--preview [force-ext] [port]] [--log [log file]] [--verbose] [--pause] [--help]");
        }

        private Engine GetEngine()
        {
            Engine engine = new Engine();

            // Add a default trace listener
            engine.Trace.AddListener(new SimpleColorConsoleTraceListener() { TraceOutputOptions = TraceOptions.None });

            // Set verbose tracing
            if (_verbose)
            {
                engine.Trace.SetLevel(SourceLevels.Verbose);
            }

            // Make sure the root folder actually exists
            if (!Directory.Exists(_rootFolder))
            {
                engine.Trace.Critical("Specified folder {0} does not exist.", _rootFolder);
                return null;
            }
            engine.RootFolder = _rootFolder;

            // Set folders
            if (_inputFolder != null)
            {
                engine.InputFolder = _inputFolder;
            }
            if (_outputFolder != null)
            {
                engine.OutputFolder = _outputFolder;
            }

            // Set up the log file         
            if (_logFile != null)
            {
                engine.Trace.AddListener(new SimpleFileTraceListener(_logFile));
            }

            return engine;
        }

        private bool Configure(Engine engine)
        {
            try
            {
                // If we have a configuration file use it, otherwise configure with defaults  
                if (File.Exists(_configFile))
                {
                    engine.Trace.Information("Loading configuration from {0}...", _configFile);
                    engine.Configure(File.ReadAllText(_configFile), _updatePackages);
                }
                else
                {
                    engine.Trace.Information("Could not find configuration file {0}, using default configuration...", _configFile);
                    engine.Configure(GetDefaultConfigScript(), _updatePackages);
                }
            }
            catch (Exception ex)
            {
                engine.Trace.Critical("Error while loading configuration: {0}.", ex.Message);
                return false;
            }

            return true;
        }

        private bool Execute(Engine engine)
        {
            if (!_noClean)
            {
                try
                {
                    engine.Trace.Information("Cleaning output directory {0}...", engine.OutputFolder);
                    if (Directory.Exists(engine.OutputFolder))
                    {
                        Directory.Delete(engine.OutputFolder, true);
                    }
                    engine.Trace.Information("Cleaned output directory.");
                }
                catch (Exception ex)
                {
                    engine.Trace.Critical("Error while cleaning output directory: {0}.", ex.Message);
                    return false;
                }
            }

            try
            {
                engine.Execute();
            }
            catch (Exception ex)
            {
                engine.Trace.Critical("Error while executing: {0}.", ex.Message);
                return false;
            }

            return true;
        }

        private IDisposable Preview(Engine engine)
        {
            string url = "http://localhost:" + _previewPort;
            return WebApp.Start(url, app =>
            {
                IFileSystem outputFolder = new PhysicalFileSystem(engine.OutputFolder);

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
                    DefaultFileNames = new List<string> {"index.html", "index.htm", "home.html", "home.htm", "default.html", "default.html"}
                });
                app.UseStaticFiles(new StaticFileOptions
                {
                    RequestPath = PathString.Empty,
                    FileSystem = outputFolder, 
                });
            });
        }

        // This is a hack until recipes are implemented, at which point it should be removed
        private string GetDefaultConfigScript()
        {
            return @"
                Pipelines.Add(""Markdown"",
	                ReadFiles(@""*.md""),
	                FrontMatter(Yaml()),
	                Markdown(),
	                Razor(),
	                WriteFiles("".html"")
                );

                Pipelines.Add(""Razor"",
	                ReadFiles(@""*.cshtml"").Where(x => Path.GetFileName(x)[0] != '_'),
	                FrontMatter(Yaml()),
	                Razor(),
	                WriteFiles("".html"")
                );

                Pipelines.Add(""Resources"",
	                CopyFiles(@""*"").Where(x => Path.GetExtension(x) != "".cshtml"" && Path.GetExtension(x) != "".md"")
                );
            ";
        }
    }
}
