using System.Diagnostics;
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
using Wyam.Common.Pipelines;
using Wyam.Common.Tracing;
using Wyam.Core.Caching;
using Wyam.Core.Documents;
using Wyam.Core.IO;
using Wyam.Core.Modules.IO;
using Wyam.Core.Pipelines;
using Wyam.Core.Tracing;

namespace Wyam.Core
{
    public class Engine : IDisposable
    {
        private bool _disposed;
        private readonly FileSystem _fileSystem = new FileSystem();
        private readonly Config _config;
        private readonly PipelineCollection _pipelines;

        public IConfigurableFileSystem FileSystem => _fileSystem;

        public IConfig Config => _config;

        public IPipelineCollection Pipelines => _pipelines;

        private readonly Dictionary<string, object> _metadata = new Dictionary<string, object>();

        // This is used as the initial set of metadata for each run
        public IDictionary<string, object> Metadata => _metadata;

        public IDocumentCollection Documents => DocumentCollection;

        internal DocumentCollection DocumentCollection { get; } = new DocumentCollection();


        private readonly Tracing.Trace _trace = new Tracing.Trace();

        public ITrace Trace => _trace;

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

        public bool CleanOutputFolderOnExecute { get; set; } = true;

        public Engine()
        {
            _config = new Config(this);
            _pipelines = new PipelineCollection(this);
        }

        public void Configure(string configScript = null, bool updatePackages = false, string fileName = null, bool outputScripts = false)
        {
            CheckDisposed();

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

        public void CleanOutputFolder()
        {
            try
            {
                Trace.Information("Cleaning output directory {0}", OutputFolder);
                if (System.IO.Directory.Exists(OutputFolder))
                {
                    System.IO.Directory.Delete(OutputFolder, true);
                }
                Trace.Information("Cleaned output directory.");
            }
            catch (Exception ex)
            {
                Trace.Warning("Error while cleaning output directory: {0} - {1}", ex.GetType(), ex.Message);
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
            if (CleanOutputFolderOnExecute)
            {
                CleanOutputFolder();
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
                Stopwatch engineStopwatch = Stopwatch.StartNew();
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
                        Stopwatch pipelineStopwatch = Stopwatch.StartNew();
                        using (Trace.WithIndent().Information("Executing pipeline \"{0}\" ({1}/{2}) with {3} child module(s)", pipelineName, c, _pipelines.Count, pipeline.Count))
                        {
                            try
                            {
                                pipeline.Execute();
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
                    ExecutionCacheManager.ClearUnhitEntries(this);
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
            _trace.Dispose();
            _config.Dispose();
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
