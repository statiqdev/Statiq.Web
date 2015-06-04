using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.StaticFiles;
using Owin;
using Wyam.Core;

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
        private bool _preview = false;
        private int _previewPort = 5080;
        private string _logFile = null;
        private bool _verbose = false;
        private bool _pause = false;
        private string _rootFolder = null;
        private string _configFile = null;
        private readonly Dictionary<string, object> _globalVariables = new Dictionary<string,object>();

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

            // Pause if requested
            if (_pause)
            {
                _engine.Trace.Information("Pause requested, hit any key to continue.");
                Console.ReadKey();
            }

            // Configure
            try
            {
                Configure();
            }
            catch (Exception ex)
            {
                _engine.Trace.Critical("Error while loading configuration: {0}.", ex.Message);
                return;
            }

            // Execute
            try
            {
                _engine.Execute();
            }
            catch (Exception ex)
            {
                _engine.Trace.Critical("Error while executing: {0}.", ex.Message);
                return;
            }

            bool pauseBeforeExit = false;

            // Start the preview server
            IDisposable previewServer = null;
            if (_preview)
            {
                pauseBeforeExit = true;
                try
                {
                    previewServer = Preview();
                    _engine.Trace.Information("Preview server running on port {0}...", _previewPort);
                }
                catch (Exception ex)
                {
                    _engine.Trace.Critical("Error while running preview server: {0}.", ex.Message);
                }
            }

            // Pause if an async process is running
            if (pauseBeforeExit)
            {
                _engine.Trace.Information("Hit any key to exit.");
                Console.ReadKey();
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
                else if (args[c] == "--preview")
                {
                    _preview = true;
                    if (c + 1 < args.Length && !args[c + 1].StartsWith("--"))
                    {
                        if (!int.TryParse(args[c++], out _previewPort))
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
            Console.WriteLine("Usage: wyam.exe [path] [--watch] [--preview [port]] [--log [log file]] [--verbose] [--pause] [--help]");
        }

        private void Configure()
        {
            // If we have a configuration file use it, otherwise configure with defaults  
            string configFile = string.IsNullOrWhiteSpace(_configFile)
                ? Path.Combine(_engine.RootFolder, "config.wyam") : Path.Combine(_engine.RootFolder, _configFile);
            if (File.Exists(configFile))
            {
                _engine.Trace.Information("Loading configuration from {0}.", configFile);
                _engine.Configure(File.ReadAllText(configFile));
            }
            else
            {
                _engine.Configure();
            }
        }

        private IDisposable Preview()
        {
            string url = "http://localhost:" + _previewPort;
            return WebApp.Start(url, app =>
            {
                IFileSystem outputFolder = new PhysicalFileSystem(_engine.OutputFolder);
                app.UseDefaultFiles(new DefaultFilesOptions()
                {
                    RequestPath = PathString.Empty,
                    FileSystem = outputFolder,
                    DefaultFileNames = new List<string> {"Index.html", "Index.htm", "Home.html", "Home.htm"}
                });
                app.UseStaticFiles(new StaticFileOptions
                {
                    RequestPath = PathString.Empty,
                    FileSystem = outputFolder
                });
            });
        }
    }
}
