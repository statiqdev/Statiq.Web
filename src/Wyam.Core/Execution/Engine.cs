using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Tracing;
using Wyam.Core.Caching;
using Wyam.Core.Configuration;
using Wyam.Core.Documents;
using Wyam.Core.IO;
using Wyam.Core.Meta;
using Wyam.Core.Tracing;

namespace Wyam.Core.Execution
{
    public class Engine : IDisposable
    {
        private readonly FileSystem _fileSystem = new FileSystem();
        private readonly Settings _settings = new Settings();
        private readonly PipelineCollection _pipelines = new PipelineCollection();
        private readonly DiagnosticsTraceListener _diagnosticsTraceListener = new DiagnosticsTraceListener();
        private readonly MetadataDictionary _initialMetadata = new MetadataDictionary();
        private bool _disposed;

        public IFileSystem FileSystem => _fileSystem;

        public ISettings Settings => _settings;

        public IPipelineCollection Pipelines => _pipelines;

        public IMetadataDictionary InitialMetadata => _initialMetadata;
        
        public IMetadataDictionary GlobalMetadata { get; } = new MetadataDictionary();

        public IDocumentCollection Documents => DocumentCollection;

        internal DocumentCollection DocumentCollection { get; } = new DocumentCollection();

        /// <summary>
        /// Gets the assemblies that should be referenced by modules that support dynamic compilation.
        /// </summary>
        public IAssemblyCollection Assemblies { get; } = new AssemblyCollection();

        /// <summary>
        /// Gets the namespaces that should be brought in scope by modules that support dynamic compilation.
        /// </summary>
        public INamespacesCollection Namespaces { get; } = new NamespaceCollection();

        /// <summary>
        /// Gets a collection of all the raw assemblies that should be referenced by modules 
        /// that support dynamic compilation (such as configuration assemblies).
        /// </summary>
        public IRawAssemblyCollection RawAssemblies { get; } = new RawAssemblyCollection();

        internal ExecutionCacheManager ExecutionCacheManager { get; } = new ExecutionCacheManager();
        
        public string ApplicationInput { get; set; }

        private IDocumentFactory _documentFactory;

        public IDocumentFactory DocumentFactory
        {
            get { return _documentFactory; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(DocumentFactory));
                }
                _documentFactory = value;
            }
        }

        public Engine()
        {
            System.Diagnostics.Trace.Listeners.Add(_diagnosticsTraceListener);
            _documentFactory = new DocumentFactory(_initialMetadata);
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
                Trace.Information("Cleaned output directory");
            }
            catch (Exception ex)
            {
                Trace.Warning("Error while cleaning output path {0}: {1} - {2}", FileSystem.OutputPath, ex.GetType(), ex.Message);
            }
        }

        public void Execute()
        {
            CheckDisposed();

            // Clean the output folder if requested
            if (Settings.CleanOutputPath)
            {
                CleanOutputPath();
            }

            // Create the output folder if it doesn't already exist
            IDirectory outputDirectory = FileSystem.GetOutputDirectory();
            if (!outputDirectory.Exists)
            {
                outputDirectory.Create();
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
                    foreach (Pipeline pipeline in _pipelines.Pipelines)
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
                        c - 1, _pipelines.Count, engineStopwatch.ElapsedMilliseconds);
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
