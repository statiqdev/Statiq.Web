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

        private readonly Engine _engine = new Engine();
        private bool _watch = false;
        private bool _clean = false;
        private bool _preview = false;
        private int _previewPort = 5080;
        private bool _previewForceExtension = false;
        private string _logFile = null;
        private bool _verbose = false;
        private bool _pause = false;
        private bool _skipPackages = false;
        private string _rootFolder = null;
        private string _configFile = null;
        private readonly Dictionary<string, object> _globalVariables = new Dictionary<string,object>();
        private readonly ConcurrentQueue<string> _changedFiles = new ConcurrentQueue<string>();
        private readonly AutoResetEvent _messageEvent = new AutoResetEvent(false);
        private readonly InterlockedBool _exit = new InterlockedBool(false);

        private void Run(string[] args)
        {
            // Add a default trace listener
            _engine.Trace.AddListener(new SimpleColorConsoleTraceListener() { TraceOutputOptions = TraceOptions.None });
            
            // Parse the command line
            if (!ParseArgs(args))
            {
                return;
            }

            // Set verbose tracing
            if (_verbose)
            {
                _engine.Trace.SetLevel(SourceLevels.Verbose);
            }

            // Make sure the root folder actually exists
            _engine.RootFolder = _rootFolder == null ? Environment.CurrentDirectory : Path.Combine(Environment.CurrentDirectory, _rootFolder);
            if (!Directory.Exists(_engine.RootFolder))
            {
                _engine.Trace.Critical("Specified folder {0} does not exist.", _engine.RootFolder);
                return;
            }

            // Set up the log file         
            if (_logFile != null)
            {
                _logFile = Path.Combine(_engine.RootFolder, _logFile);
                _engine.Trace.AddListener(new SimpleFileTraceListener(_logFile));
            }

            // Pause
            if (_pause)
            {
                _engine.Trace.Information("Pause requested, hit any key to continue.");
                Console.ReadKey();
            }

            // Configure
            if (!Configure())
            {
                return;
            }

            // Execute
            if (!Execute())
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
                    _engine.Trace.Information("Preview server running on port {0}...", _previewPort);
                    previewServer = Preview();
                }
                catch (Exception ex)
                {
                    _engine.Trace.Critical("Error while running preview server: {0}.", ex.Message);
                }
            }

            // Start the watcher
            IDisposable watcher = null;
            if (_watch)
            {
                messagePump = true;
                _engine.Trace.Information("Watching folder {0}...", _engine.InputFolder);
                watcher = new ActionFileSystemWatcher(_engine.InputFolder, path =>
                {
                    _changedFiles.Enqueue(path);
                    _messageEvent.Set();
                });
            }

            // Start the message pump if an async process is running
            if (messagePump)
            {
                // Start the key listening thread
                _engine.Trace.Information("Hit any key to exit...");
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
                    _messageEvent.WaitOne();
                    if (_exit)
                    {
                        break;
                    }

                    // Execute if files have changed
                    HashSet<string> changedFiles = new HashSet<string>();
                    string changedFile;
                    while (_changedFiles.TryDequeue(out changedFile))
                    {
                        if (changedFiles.Add(changedFile))
                        {
                            _engine.Trace.Verbose("{0} has changed.", changedFile);
                        }
                    }
                    if (changedFiles.Count > 0)
                    {
                        _engine.Trace.Information("{0} files have changed, re-executing...", changedFiles.Count);
                        Execute();
                    }

                    // Check one more time for exit
                    if (_exit)
                    {
                        break;
                    }
                    _engine.Trace.Information("Hit any key to exit...");
                    _messageEvent.Reset();
                }

                // Shutdown
                _engine.Trace.Information("Shutting down...");
                if (watcher != null)
                {
                    watcher.Dispose();
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
                else if (args[c] == "--clean")
                {
                    _clean = true;
                }
                else if (args[c] == "--preview")
                {
                    _preview = true;
                    while (c + 1 < args.Length && !args[c + 1].StartsWith("--"))
                    {
                        if (args[c] == "force-ext")
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
                        _logFile = args[c++];
                    }
                }
                else if (args[c] == "--config")
                {
                    if (c + 1 < args.Length && !args[c + 1].StartsWith("--"))
                    {
                        _configFile = args[c++];
                    }
                }
                else if (args[c] == "--skip-packages")
                {
                    _skipPackages = true;
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
                else if (c == 0)
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
            Console.WriteLine("Usage: wyam.exe [path] [--clean] [--skip-packages] [--watch] [--preview [force-ext] [port]] [--log [log file]] [--verbose] [--pause] [--help]");
        }

        private bool Configure()
        {
            try
            {
                // If we have a configuration file use it, otherwise configure with defaults  
                string configFile = string.IsNullOrWhiteSpace(_configFile)
                    ? Path.Combine(_engine.RootFolder, "config.wyam") : Path.Combine(_engine.RootFolder, _configFile);
                if (File.Exists(configFile))
                {
                    _engine.Trace.Information("Loading configuration from {0}.", configFile);
                    _engine.Configure(File.ReadAllText(configFile), !_skipPackages);
                }
                else
                {
                    _engine.Trace.Information("Could not find configuration file {0}, using default configuration.", configFile);
                    _engine.Configure(null, !_skipPackages);
                }
            }
            catch (Exception ex)
            {
                _engine.Trace.Critical("Error while loading configuration: {0}.", ex.Message);
                return false;
            }

            return true;
        }

        private bool Execute()
        {
            if (_clean)
            {
                try
                {
                    _engine.Trace.Information("Cleaning output directory {0}...", _engine.OutputFolder);
                    Directory.Delete(_engine.OutputFolder, true);
                    _engine.Trace.Information("Cleaned output directory.");
                }
                catch (Exception ex)
                {
                    _engine.Trace.Critical("Error while cleaning output directory: {0}.", ex.Message);
                    return false;
                }
            }

            try
            {
                _engine.Execute();
            }
            catch (Exception ex)
            {
                _engine.Trace.Critical("Error while executing: {0}.", ex.Message);
                return false;
            }

            return true;
        }

        private IDisposable Preview()
        {
            string url = "http://localhost:" + _previewPort;
            return WebApp.Start(url, app =>
            {
                IFileSystem outputFolder = new PhysicalFileSystem(_engine.OutputFolder);

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
    }
}
