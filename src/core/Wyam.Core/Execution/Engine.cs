using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JavaScriptEngineSwitcher.Core;
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
    /// <summary>
    /// The engine is the primary entry point for the generation process.
    /// </summary>
    public class Engine : IEngine, IDisposable
    {
        public static string Version
        {
            get
            {
                AssemblyInformationalVersionAttribute versionAttribute
                    = Attribute.GetCustomAttribute(typeof(Engine).Assembly, typeof(AssemblyInformationalVersionAttribute)) as AssemblyInformationalVersionAttribute;
                if (versionAttribute == null)
                {
                    throw new Exception("Something went terribly wrong, could not determine Wyam version");
                }
                return versionAttribute.InformationalVersion;
            }
        }

        private readonly FileSystem _fileSystem = new FileSystem();
        private readonly Settings _settings = new Settings();
        private readonly PipelineCollection _pipelines = new PipelineCollection();
        private readonly DiagnosticsTraceListener _diagnosticsTraceListener = new DiagnosticsTraceListener();
        private bool _disposed;

        /// <summary>
        /// Gets the file system.
        /// </summary>
        public IFileSystem FileSystem => _fileSystem;

        /// <summary>
        /// Gets the settings.
        /// </summary>
        public ISettings Settings => _settings;

        /// <summary>
        /// Gets the pipelines.
        /// </summary>
        public IPipelineCollection Pipelines => _pipelines;

        /// <summary>
        /// Gets the initial metadata.
        /// </summary>
        [Obsolete]
        public IMetadataDictionary InitialMetadata
        {
            get
            {
                Trace.Warning("InitialMetadata is deprecated and will be removed in a future version. Please use Settings instead.");
                return Settings;
            }
        }

        /// <summary>
        /// Gets the global metadata.
        /// </summary>
        [Obsolete]
        public IMetadataDictionary GlobalMetadata
        {
            get
            {
                Trace.Warning("GlobalMetadata is deprecated and will be removed in a future version. Please use Settings instead.");
                return Settings;
            }
        }

        /// <summary>
        /// Gets the documents.
        /// </summary>
        public IDocumentCollection Documents => DocumentCollection;

        internal DocumentCollection DocumentCollection { get; } = new DocumentCollection();
        
        /// <summary>
        /// Gets the namespaces that should be brought in scope by modules that support dynamic compilation.
        /// </summary>
        public INamespacesCollection Namespaces { get; } = new NamespaceCollection();

        /// <summary>
        /// Gets a collection of all the raw assemblies that should be referenced by modules 
        /// that support dynamic compilation (such as configuration assemblies).
        /// </summary>
        public IRawAssemblyCollection DynamicAssemblies { get; } = new RawAssemblyCollection();

        internal ExecutionCacheManager ExecutionCacheManager { get; } = new ExecutionCacheManager();

        /// <summary>
        /// Gets or sets the application input.
        /// </summary>
        public string ApplicationInput { get; set; }

        private IDocumentFactory _documentFactory;

        /// <summary>
        /// Gets or sets the document factory.
        /// </summary>
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
            _documentFactory = new DocumentFactory(_settings);
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

        /// <summary>
        /// Resets the JavaScript Engine pool and clears the JavaScript Engine Switcher
        /// to an empty list of engine factories and default engine. Useful on configuration
        /// change where we might have a new configuration.
        /// </summary>
        public static void ResetJsEngines()
        {
            JsEngineSwitcher.Instance.EngineFactories.Clear();
            JsEngineSwitcher.Instance.DefaultEngineName = string.Empty;            
        }

        public void Execute()
        {
            CheckDisposed();

            Trace.Information($"Using {JsEngineSwitcher.Instance.DefaultEngineName} as the JavaScript engine");

            // Make sure we've actually configured some pipelines
            if (_pipelines.Count == 0)
            {
                Trace.Error("No pipelines are configured. Please supply a configuration file, specify a recipe, or configure programmatically");
                return;
            }

            // Clean the output folder if requested
            if (Settings.Bool(Keys.CleanOutputPath))
            {
                CleanOutputPath();
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
