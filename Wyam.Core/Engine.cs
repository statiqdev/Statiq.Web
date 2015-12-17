using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NuGet;
using Wyam.Core.Configuration;
using Wyam.Core.NuGet;
using Wyam.Common;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Pipelines;
using Wyam.Common.Tracing;
using Wyam.Core.Caching;
using Wyam.Core.Documents;
using Wyam.Core.Pipelines;
using Wyam.Core.Tracing;

namespace Wyam.Core
{
    public class Engine : IDisposable
    {
        private Configurator _configurator = null;
        private bool _disposed;

        private readonly Dictionary<string, object> _metadata = new Dictionary<string, object>();

        // This is used as the initial set of metadata for each run
        public IDictionary<string, object> Metadata => _metadata;

        public IDocumentCollection Documents => DocumentCollection;

        internal DocumentCollection DocumentCollection { get; } = new DocumentCollection();

        private readonly PipelineCollection _pipelines;

        public IPipelineCollection Pipelines => _pipelines;

        private readonly Tracing.Trace _trace = new Tracing.Trace();

        public ITrace Trace => _trace;

        public byte[] RawConfigAssembly => _configurator?.RawConfigAssembly;

        public IEnumerable<Assembly> Assemblies => _configurator?.Assemblies;

        public IEnumerable<string> Namespaces => _configurator?.Namespaces;

        internal ExecutionCacheManager ExecutionCacheManager { get; } = new ExecutionCacheManager();

        public bool NoCache
        {
            get { return ExecutionCacheManager.NoCache; }
            set { ExecutionCacheManager.NoCache = value; }
        }
        
        private string _rootFolder = Environment.CurrentDirectory;
        private string _inputFolder = "Input";
        private string _outputFolder = "Output";

        public string RootFolder
        {
            get { return _rootFolder; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException(nameof(value));
                }
                _rootFolder = Path.GetFullPath(PathHelper.NormalizePath(value));
            }
        }

        public string InputFolder
        {
            get
            {
                // Calculate this each time in case the root folder changes after setting it
                return Path.GetFullPath(Path.Combine(RootFolder, PathHelper.NormalizePath(_inputFolder)));
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

        public string OutputFolder
        {
            get
            {
                // Calculate this each time in case the root folder changes after setting it
                return Path.GetFullPath(Path.Combine(RootFolder, PathHelper.NormalizePath(_outputFolder)));
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
            _pipelines = new PipelineCollection(this);
        }

        public void Configure(string configScript = null, bool updatePackages = false, string fileName = null, bool outputScripts = false)
        {
            CheckDisposed();

            try
            {
                if(_configurator != null)
                {
                    throw new InvalidOperationException("This engine has already been configured.");
                }
                _configurator = new Configurator(this, fileName, outputScripts);
                _configurator.Configure(configScript, updatePackages);
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
                if (Directory.Exists(OutputFolder))
                {
                    Directory.Delete(OutputFolder, true);
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
            if (_configurator == null)
            {
                Configure();
            }

            // Clean the output folder if requested
            if (CleanOutputFolderOnExecute)
            {
                CleanOutputFolder();
            }

            // Create the input and output folders if they don't already exist
            if (!Directory.Exists(InputFolder))
            {
                Directory.CreateDirectory(InputFolder);
            }
            if (!Directory.Exists(OutputFolder))
            {
                Directory.CreateDirectory(OutputFolder);
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
                        Stopwatch pipelineStopwatch = Stopwatch.StartNew();
                        using (Trace.WithIndent().Information("Executing pipeline \"{0}\" ({1}/{2}) with {3} child module(s)", pipeline.Name, c, _pipelines.Count, pipeline.Count))
                        {
                            pipeline.Execute();
                            pipelineStopwatch.Stop();
                            Trace.Information("Executed pipeline \"{0}\" ({1}/{2}) in {3} ms resulting in {4} output document(s)",
                                pipeline.Name, c++, _pipelines.Count, pipelineStopwatch.ElapsedMilliseconds,
                                DocumentCollection.FromPipeline(pipeline.Name).Count());
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
                    Trace.Information("Executed {0} pipelines in {1} ms",
                        _pipelines.Count, engineStopwatch.ElapsedMilliseconds);
                }

            }
            catch (Exception ex)
            {
                Trace.Verbose("Exception while executing pipelines: {0}", ex);
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
            _configurator?.Dispose();
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
