using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.Hosting.Tracing;
using Microsoft.Owin.StaticFiles;
using Owin;
using Wyam.Common.IO;
using Wyam.Common.Tracing;
using Wyam.Configuration.Preprocessing;
using Wyam.Owin;

namespace Wyam
{
    public class Program
    {
        private static int Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionEvent;
            Program program = new Program();
            return program.Run(args);
        }

        private static void UnhandledExceptionEvent(object sender, UnhandledExceptionEventArgs e)
        {
            // Exit with a error exit code
            Environment.Exit((int)ExitCode.UnhandledError);
        }

        private readonly Preprocessor _preprocessor = new Preprocessor();
        private readonly Settings _settings = new Settings();
        private readonly ConcurrentQueue<string> _changedFiles = new ConcurrentQueue<string>();
        private readonly AutoResetEvent _messageEvent = new AutoResetEvent(false);
        private readonly InterlockedBool _exit = new InterlockedBool(false);
        private readonly InterlockedBool _newEngine = new InterlockedBool(false);

        private int Run(string[] args)
        {
            // Add a default trace listener
            Trace.AddListener(new SimpleColorConsoleTraceListener { TraceOutputOptions = System.Diagnostics.TraceOptions.None });
            
            // Output version info
            AssemblyInformationalVersionAttribute versionAttribute
                = Attribute.GetCustomAttribute(typeof(Program).Assembly, typeof(AssemblyInformationalVersionAttribute)) as AssemblyInformationalVersionAttribute;
            Trace.Information("Wyam version {0}", versionAttribute == null ? "unknown" : versionAttribute.InformationalVersion);

            // It's not a serious console app unless there's some ASCII art
            OutputLogo();

            // Parse the command line
            try
            {
                bool hasParseArgsErrors;
                if (!_settings.ParseArgs(args, _preprocessor, out hasParseArgsErrors))
                {
                    return hasParseArgsErrors ? (int)ExitCode.CommandLineError : (int)ExitCode.Normal;
                }

                // Was help for the preprocessor directives requested?
                if (_settings.HelpDirectives)
                {
                    Console.WriteLine("Available preprocessor directives:");
                    foreach (IDirective directive in _preprocessor.Directives)
                    {
                        Console.WriteLine();
                        Console.WriteLine($"{directive.Description}:");
                        Console.WriteLine(string.Join(", ", directive.DirectiveNames.Select(x => "#" + x)));
                        Console.WriteLine(directive.GetHelpText());
                    }
                    return (int) ExitCode.Normal;
                }
            }
            catch (Exception ex)
            {
                Trace.Error("Error while parsing command line: {0}", ex.Message);
                if (Trace.Level == System.Diagnostics.SourceLevels.Verbose)
                    Trace.Error("Stack trace:{0}{1}", Environment.NewLine, ex.StackTrace);

                return (int)ExitCode.CommandLineError;
            }

            // Fix the root folder and other files
            DirectoryPath currentDirectory = Environment.CurrentDirectory;
            _settings.RootPath = _settings.RootPath == null ? currentDirectory : currentDirectory.Combine(_settings.RootPath);
            _settings.LogFilePath = _settings.LogFilePath == null ? null : _settings.RootPath.CombineFile(_settings.LogFilePath);
            _settings.ConfigFilePath = _settings.RootPath.CombineFile(_settings.ConfigFilePath ?? "config.wyam");

            // Set up the log file         
            if (_settings.LogFilePath != null)
            {
                Trace.AddListener(new SimpleFileTraceListener(_settings.LogFilePath.FullPath));
            }

            // Prepare engine metadata
            if (!_settings.VerifyConfig && _settings.GlobalMetadataArgs != null && _settings.GlobalMetadataArgs.Count > 0)
            {
                try
                {
                    _settings.GlobalMetadata = GlobalMetadataParser.Parse(_settings.GlobalMetadataArgs);
                }
                catch (MetadataParseException ex)
                {
                    Trace.Error("Error while parsing metadata: {0}", ex.Message);
                    if (Trace.Level == System.Diagnostics.SourceLevels.Verbose)
                        Trace.Error("Stack trace:{0}{1}", Environment.NewLine, ex.StackTrace);

                    return (int)ExitCode.CommandLineError;
                }
                // Not used anymore, release resources.
                _settings.GlobalMetadataArgs = null;
            }

            // Get the engine and configurator
            EngineManager engineManager = GetEngineManager();
            if (engineManager == null)
            {
                return (int)ExitCode.CommandLineError;
            }

            // Pause
            if (_settings.Pause)
            {
                Trace.Information("Pause requested, hit any key to continue");
                Console.ReadKey();
            }

            // Configure and execute
            if (!engineManager.Configure())
            {
                return (int)ExitCode.ConfigurationError;
            }

            if (_settings.VerifyConfig)
            {
                Trace.Information("No errors. Exiting.");
                return (int)ExitCode.Normal;
            }

            Trace.Information($"Root path:{Environment.NewLine}  {engineManager.Engine.FileSystem.RootPath}");
            Trace.Information($"Input path(s):{Environment.NewLine}  {string.Join(Environment.NewLine + "  ", engineManager.Engine.FileSystem.InputPaths)}");
            Trace.Information($"Output path:{Environment.NewLine}  {engineManager.Engine.FileSystem.OutputPath}");
            if (!engineManager.Execute())
            {
                return (int)ExitCode.ExecutionError;
            }

            bool messagePump = false;

            // Start the preview server
            IDisposable previewServer = null;
            if (_settings.Preview)
            {
                messagePump = true;
                try
                {
                    DirectoryPath previewPath = _settings.PreviewRoot == null
                        ? engineManager.Engine.FileSystem.GetOutputDirectory().Path
                        : engineManager.Engine.FileSystem.GetOutputDirectory(_settings.PreviewRoot).Path;
                    Trace.Information("Preview server listening on port {0} and serving from path {1}", _settings.PreviewPort, previewPath);
                    previewServer = Preview(previewPath);
                }
                catch (Exception ex)
                {
                    Trace.Critical("Error while running preview server: {0}", ex.Message);
                }
            }

            // Start the watchers
            IDisposable inputFolderWatcher = null;
            IDisposable configFileWatcher = null;
            if (_settings.Watch)
            {
                messagePump = true;

                Trace.Information("Watching paths(s) {0}", string.Join(", ", engineManager.Engine.FileSystem.InputPaths));
                inputFolderWatcher = new ActionFileSystemWatcher(engineManager.Engine.FileSystem.GetOutputDirectory().Path,
                    engineManager.Engine.FileSystem.GetInputDirectories().Select(x => x.Path), true, "*.*", path =>
                    {
                        _changedFiles.Enqueue(path);
                        _messageEvent.Set();
                    });

                if (_settings.ConfigFilePath != null)
                {
                    Trace.Information("Watching configuration file {0}", _settings.ConfigFilePath);
                    configFileWatcher = new ActionFileSystemWatcher(engineManager.Engine.FileSystem.GetOutputDirectory().Path,
                        new[] { _settings.ConfigFilePath.Directory }, false, _settings.ConfigFilePath.FileName.FullPath, path =>
                        {
                            FilePath filePath = new FilePath(path);
                            if (_settings.ConfigFilePath.Equals(filePath))
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
                        Trace.Information("Configuration file {0} has changed, re-running", _settings.ConfigFilePath);
                        engineManager.Dispose();
                        engineManager = GetEngineManager();

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
            return (int)exitCode;
        }

        private EngineManager GetEngineManager()
        {
            try
            {
                return new EngineManager(_preprocessor, _settings);
            }
            catch (Exception ex)
            {
                Trace.Critical("Error while instantiating engine: {0}", ex.Message);
                return null;
            }
        }

        private IDisposable Preview(DirectoryPath root)
        {
            StartOptions options = new StartOptions("http://localhost:" + _settings.PreviewPort);

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
                if (!_settings.PreviewForceExtension)
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

        private void OutputLogo()
        {
            Console.WriteLine(@"
   ,@@@@@       /@\        @@@@@  
   @@@@@@      @@@@@|     $@@@@@h 
  $@@@@@     ,@@@@@@@    g@@@@@P  
 ]@@@@@M    g@@@@@@@    g@@@@@P   
 $@@@@@    @@@@@@@@@   g@@@@@P    
j@@@@@   g@@@@@@@@@p ,@@@@@@@     
$@@@@@g@@@@@@@@B@@@@@@@@@@@P      
`$@@@@@@@@@@@`  ]@@@@@@@@@`       
  $@@@@@@@P`     ?$@@@@@P         
    `^``           *P*`           ");
        }
    }
}
