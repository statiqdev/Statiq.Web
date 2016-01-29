using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Wyam.Core.Configuration;
using Wyam.Core.NuGet;
using Wyam.Common;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Pipelines;
using Wyam.Common.Tracing;
using Wyam.Core.Caching;
using Wyam.Core.Documents;
using Wyam.Core.IO;
using Wyam.Core.Meta;
using Wyam.Core.Modules.IO;
using Wyam.Core.Pipelines;
using Wyam.Core.Tracing;

namespace Wyam.Core
{
    public class Engine : IDisposable
    {
        private readonly FileSystem _fileSystem = new FileSystem();
        private readonly PipelineCollection _pipelines = new PipelineCollection();
        private readonly DiagnosticsTraceListener _diagnosticsTraceListener = new DiagnosticsTraceListener();
        private readonly Config _config;
        private bool _disposed;

        public IConfigurableFileSystem FileSystem => _fileSystem;

        public IConfig Config => _config;

        public IPipelineCollection Pipelines => _pipelines;

        public IInitialMetadata InitialMetadata { get; } = new InitialMetadata();

        public IDocumentCollection Documents => DocumentCollection;

        internal DocumentCollection DocumentCollection { get; } = new DocumentCollection();

        public byte[] RawConfigAssembly => _config.RawConfigAssembly;

        public IEnumerable<Assembly> Assemblies => _config.Assemblies;

        public IEnumerable<string> Namespaces => _config.Namespaces;

        internal ExecutionCacheManager ExecutionCacheManager { get; } = new ExecutionCacheManager();
        
        public bool NoCache
        {
            get { return ExecutionCacheManager.NoCache; }
            set { ExecutionCacheManager.NoCache = value; }
        }
        
        private string _rootFolder = Environment.CurrentDirectory;
        private string _inputFolder = "Input";
        private string _outputFolder = "Output";

        [Obsolete]
        public string RootFolder
        {
            get { return _rootFolder; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException(nameof(value));
                }
                _rootFolder = System.IO.Path.GetFullPath(PathHelper.NormalizePath(value));
            }
        }

        [Obsolete]
        public string InputFolder
        {
            get
            {
                // Calculate this each time in case the root folder changes after setting it
                return System.IO.Path.GetFullPath(System.IO.Path.Combine(RootFolder, PathHelper.NormalizePath(_inputFolder)));
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException(nameof(value));
                }
                _inputFolder = PathHelper.NormalizePath(value);;
            }
        }

        [Obsolete]
        public string OutputFolder
        {
            get
            {
                // Calculate this each time in case the root folder changes after setting it
                return System.IO.Path.GetFullPath(System.IO.Path.Combine(RootFolder, PathHelper.NormalizePath(_outputFolder)));
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("OutputFolder");
                }
                _outputFolder = PathHelper.NormalizePath(value);
            }
        }

        public bool CleanOutputPathOnExecute { get; set; } = true;

        public Engine()
        {
            System.Diagnostics.Trace.Listeners.Add(_diagnosticsTraceListener);
            _config = new Config(FileSystem, InitialMetadata, Pipelines);
        }

        public void Configure(IFile configFile, bool updatePackages = false, bool outputScripts = false)
        {
            Configure(configFile.ReadAllText(), updatePackages, configFile.Path.GetFilename().FullPath, outputScripts);
        }

        public void Configure(string configScript = null, bool updatePackages = false)
        {
            Configure(configScript, updatePackages, null, false);
        }

        private void Configure(string configScript, bool updatePackages, string fileName, bool outputScripts)
        {
            CheckDisposed();

            // Make sure the root path exists
            if (!FileSystem.GetRootDirectory().Exists)
            {
                throw new InvalidOperationException($"The root path {FileSystem.RootPath.FullPath} does not exist.");
            }

            try
            {
                _config.Configure(configScript, updatePackages, fileName, outputScripts);
            }
            catch (Exception ex)
            {
                Trace.Verbose("Exception: {0}", ex);
                throw;
            }
        }

        public void CleanOutputPath()
        {
            try
            {
                Trace.Information("Cleaning output path {0}", FileSystem.OutputPath);
                IDirectory outputDirectory = FileSystem.GetOutputDirectory();
                if (outputDirectory.Exists)
                {
                    outputDirectory.Delete(true);
                }
                Trace.Information("Cleaned output directory.");
            }
            catch (Exception ex)
            {
                Trace.Warning("Error while cleaning output path {0}: {1} - {2}", FileSystem.OutputPath, ex.GetType(), ex.Message);
            }
        }

        public void Execute()
        {
            CheckDisposed();

            // Configure with defaults if not already configured
            if (!_config.Configured)
            {
                Configure();
            }

            // Clean the output folder if requested
            if (CleanOutputPathOnExecute)
            {
                CleanOutputPath();
            }

            // Create the input and output folders if they don't already exist
            if (!System.IO.Directory.Exists(InputFolder))
            {
                System.IO.Directory.CreateDirectory(InputFolder);
            }
            if (!System.IO.Directory.Exists(OutputFolder))
            {
                System.IO.Directory.CreateDirectory(OutputFolder);
            }
            
            try
            {
                System.Diagnostics.Stopwatch engineStopwatch = System.Diagnostics.Stopwatch.StartNew();
                using (Trace.WithIndent().Information("Executing {0} pipelines", _pipelines.Count))
                {
                    // Setup (clear the document collection and reset cache counters)
                    DocumentCollection.Clear();
                    ExecutionCacheManager.ResetEntryHits();

                    // Enumerate pipelines and execute each in order
                    int c = 1;
                    foreach(Pipeline pipeline in _pipelines.Pipelines)
                    {
                        string pipelineName = pipeline.Name;
                        System.Diagnostics.Stopwatch pipelineStopwatch = System.Diagnostics.Stopwatch.StartNew();
                        using (Trace.WithIndent().Information("Executing pipeline \"{0}\" ({1}/{2}) with {3} child module(s)", pipelineName, c, _pipelines.Count, pipeline.Count))
                        {
                            try
                            {
                                pipeline.Execute(this);
                                pipelineStopwatch.Stop();
                                Trace.Information("Executed pipeline \"{0}\" ({1}/{2}) in {3} ms resulting in {4} output document(s)",
                                    pipelineName, c++, _pipelines.Count, pipelineStopwatch.ElapsedMilliseconds,
                                    DocumentCollection.FromPipeline(pipelineName).Count());
                            }
                            catch (Exception)
                            {
                                Trace.Error("Error while executing pipeline {0}", pipelineName);
                                throw;
                            }
                        }
                    }

                    // Clean up (clear unhit cache entries, dispose documents)
                    // Note that disposing the documents immediately after engine execution will ensure write streams get flushed and released
                    // but will also mean that callers (and tests) can't access documents and document content after the engine finishes
                    // Easiest way to access content after engine execution is to add a final Meta module and copy content to metadata
                    ExecutionCacheManager.ClearUnhitEntries();
                    foreach (Pipeline pipeline in _pipelines.Pipelines)
                    {
                        pipeline.ResetClonedDocuments();
                    }

                    engineStopwatch.Stop();
                    Trace.Information("Executed {0}/{1} pipelines in {2} ms",
                        c, _pipelines.Count, engineStopwatch.ElapsedMilliseconds);
                }

            }
            catch (Exception ex)
            {
                Trace.Critical("Exception during execution: {0}", ex.ToString());
                throw;
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            foreach (Pipeline pipeline in _pipelines.Pipelines)
            {
                pipeline.Dispose();
            }
            _config.Dispose();
            System.Diagnostics.Trace.Listeners.Remove(_diagnosticsTraceListener);
            _disposed = true;
        }

        private void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(Engine));
            }
        }
    }
}
