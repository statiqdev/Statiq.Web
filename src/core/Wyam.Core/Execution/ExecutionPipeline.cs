using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using ConcurrentCollections;
using Wyam.Common;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Modules;
using Wyam.Common.Execution;
using Wyam.Common.Tracing;
using Wyam.Common.Util;
using Wyam.Core.Caching;
using Wyam.Core.Documents;
using Wyam.Core.Meta;

namespace Wyam.Core.Execution
{
    internal class ExecutionPipeline : IPipeline, IDisposable
    {
        private readonly ConcurrentHashSet<FilePath> _documentSources = new ConcurrentHashSet<FilePath>();
        private readonly IModuleList _modules;
        private ConcurrentBag<IDocument> _clonedDocuments = new ConcurrentBag<IDocument>();
        private Cache<List<IDocument>> _previouslyProcessedCache;
        private Dictionary<FilePath, List<IDocument>> _processedSources;
        private bool _disposed;

        public string Name { get; }

        public ExecutionPipeline(string name, params IModule[] modules)
            : this(name, new ModuleList(modules))
        {
        }

        public ExecutionPipeline(string name, IModuleList modules)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(nameof(name));
            }
            Name = name;
            _modules = modules ?? new ModuleList();
        }

        public bool ProcessDocumentsOnce
        {
            get
            {
                return _previouslyProcessedCache != null;
            }

            set
            {
                if (!value)
                {
                    _previouslyProcessedCache = null;
                    _processedSources = null;
                }
                else if (_previouslyProcessedCache == null)
                {
                    _previouslyProcessedCache = new Cache<List<IDocument>>();
                    _processedSources = new Dictionary<FilePath, List<IDocument>>();
                }
            }
        }

        // This is the main execute method called by the engine
        public void Execute(Engine engine, Guid executionId)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ExecutionPipeline));
            }

            // Setup for pipeline execution
            _documentSources.Clear();
            ResetClonedDocuments();
            _previouslyProcessedCache?.ResetEntryHits();
            _processedSources?.Clear();

            // Execute all modules in the pipeline
            IReadOnlyList<IDocument> resultDocuments;
            using (ExecutionContext context = new ExecutionContext(engine, executionId, this))
            {
                ImmutableArray<IDocument> inputs = new[] { engine.DocumentFactory.GetDocument(context) }.ToImmutableArray();
                resultDocuments = Execute(context, _modules, inputs);
            }

            // Dispose documents that aren't part of the final collection for this pipeline,
            // but don't dispose any documents that are referenced directly or indirectly from the final ones
            HashSet<IDocument> flattenedResultDocuments = new HashSet<IDocument>();
            FlattenResultDocuments(resultDocuments, flattenedResultDocuments);
            Parallel.ForEach(_clonedDocuments.Where(x => !flattenedResultDocuments.Contains(x)), x => x.Dispose());
            _clonedDocuments = new ConcurrentBag<IDocument>(flattenedResultDocuments);

            // Check the previously processed cache for any previously processed documents that need to be added
            if (_previouslyProcessedCache != null && _processedSources != null)
            {
                // Dispose the previously processed documents that we didn't get this time around
                Parallel.ForEach(_previouslyProcessedCache.ClearUnhitEntries().SelectMany(x => x), x => x.Dispose());

                // Trace the number of previously processed documents
                Trace.Verbose("{0} previously processed document(s) were not reprocessed", _previouslyProcessedCache.GetValues().Sum(x => x.Count));

                // Add new result documents to the cache
                foreach (IDocument resultDocument in _clonedDocuments)
                {
                    if (_processedSources.TryGetValue(resultDocument.Source, out List<IDocument> processedResultDocuments))
                    {
                        processedResultDocuments.Add(resultDocument);
                    }
                    else
                    {
                        Trace.Warning("Could not find processed document cache for source {0}, please report this warning to the developers", resultDocument.SourceString());
                    }
                }

                // Reset cloned documents (since we're tracking them in the previously processed cache now) and set new aggregate results
                _clonedDocuments = new ConcurrentBag<IDocument>();
                engine.DocumentCollection.Set(Name, _previouslyProcessedCache.GetValues().SelectMany(x => x).ToList().AsReadOnly());
            }
        }

        // This executes the specified modules with the specified input documents
        public IReadOnlyList<IDocument> Execute(ExecutionContext context, IEnumerable<IModule> modules, ImmutableArray<IDocument> inputDocuments)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ExecutionPipeline));
            }

            ImmutableArray<IDocument> resultDocuments = ImmutableArray<IDocument>.Empty;
            foreach (IModule module in modules.Where(x => x != null))
            {
                string moduleName = module.GetType().Name;
                System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
                using (Trace.WithIndent().Verbose("Executing module {0} with {1} input document(s)", moduleName, inputDocuments.Length))
                {
                    try
                    {
                        // Execute the module
                        using (ExecutionContext moduleContext = context.Clone(module))
                        {
                            resultDocuments = module.Execute(inputDocuments, moduleContext)?.Where(x => x != null).ToImmutableArray() ?? ImmutableArray<IDocument>.Empty;
                        }

                        // Remove any documents that were previously processed (checking will also mark the cache entry as hit)
                        if (_previouslyProcessedCache != null && _processedSources != null)
                        {
                            ImmutableArray<IDocument>.Builder newDocuments = ImmutableArray.CreateBuilder<IDocument>();
                            foreach (IDocument resultDocument in resultDocuments)
                            {
                                if (_processedSources.ContainsKey(resultDocument.Source))
                                {
                                    // We've seen this source before and already added it to the processed cache
                                    newDocuments.Add(resultDocument);
                                }
                                else
                                {
                                    if (!_previouslyProcessedCache.TryGetValue(resultDocument, out List<IDocument> processedDocuments))
                                    {
                                        // This document was not previously processed, so add it to the current result and set up a list to track final results
                                        newDocuments.Add(resultDocument);
                                        processedDocuments = new List<IDocument>();
                                        _previouslyProcessedCache.Set(resultDocument, processedDocuments);
                                        _processedSources.Add(resultDocument.Source, processedDocuments);
                                    }

                                    // Otherwise, this document was previously processed so don't add it to the results
                                }
                            }
                            if (newDocuments.Count != resultDocuments.Length)
                            {
                                Trace.Verbose("Removed {0} previously processed document(s)", resultDocuments.Length - newDocuments.Count);
                            }
                            resultDocuments = newDocuments.ToImmutable();
                        }

                        // Set results in engine and trace
                        context.Engine.DocumentCollection.Set(Name, resultDocuments);
                        stopwatch.Stop();
                        Trace.Verbose(
                            "Executed module {0} in {1} ms resulting in {2} output document(s)",
                            moduleName,
                            stopwatch.ElapsedMilliseconds,
                            resultDocuments.Length);
                        inputDocuments = resultDocuments;
                    }
                    catch (Exception)
                    {
                        Trace.Error("Error while executing module {0}", moduleName);
                        resultDocuments = ImmutableArray<IDocument>.Empty;
                        context.Engine.DocumentCollection.Set(Name, resultDocuments);
                        throw;
                    }
                }
            }

            // Set the document collection result one more time just to be sure it matches the result documents
            context.Engine.DocumentCollection.Set(Name, resultDocuments);
            return resultDocuments;
        }

        private void FlattenResultDocuments(IEnumerable<IDocument> documents, HashSet<IDocument> flattenedResultDocuments)
        {
            foreach (IDocument document in documents)
            {
                if (document == null || !flattenedResultDocuments.Add(document))
                {
                    continue;
                }

                FlattenResultDocuments(
                    document.Keys.SelectMany(x =>
                    {
                        object value = document.GetRaw(x);
                        IEnumerable<IDocument> children = value as IEnumerable<IDocument>;
                        if (children == null && value is IDocument)
                        {
                            children = new[] { (IDocument)value };
                        }
                        return children ?? Enumerable.Empty<IDocument>();
                    }),
                    flattenedResultDocuments);
            }
        }

        public void AddClonedDocument(IDocument document) => _clonedDocuments.Add(document);

        public void AddDocumentSource(FilePath source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (!_documentSources.Add(source))
            {
                throw new ArgumentException("Document source must be unique within the pipeline: " + source);
            }
        }

        public void ResetClonedDocuments()
        {
            Parallel.ForEach(_clonedDocuments, x => x.Dispose());
            _clonedDocuments = new ConcurrentBag<IDocument>();
        }

        public void Dispose()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ExecutionPipeline));
            }
            _disposed = true;

            // Clean up the documents
            ResetClonedDocuments();
            if (_previouslyProcessedCache != null)
            {
                Parallel.ForEach(_previouslyProcessedCache.GetValues().SelectMany(x => x), x => x.Dispose());
            }

            // Clean up the modules
            DisposeModules(_modules);
        }

        private void DisposeModules(IEnumerable<IModule> modules)
        {
            foreach (IModule module in modules)
            {
                (module as IDisposable)?.Dispose();
                IEnumerable<IModule> childModules = module as IEnumerable<IModule>;
                if (childModules != null)
                {
                    DisposeModules(childModules);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<IModule> GetEnumerator() => _modules.GetEnumerator();

        public void Add(IModule item) => _modules.Add(item);

        public void Clear() => _modules.Clear();

        public bool Contains(IModule item) => _modules.Contains(item);

        public void CopyTo(IModule[] array, int arrayIndex) => _modules.CopyTo(array, arrayIndex);

        public bool Remove(IModule item) => _modules.Remove(item);

        public bool Remove(string name) => _modules.Remove(name);

        public int Count => _modules.Count;

        public void Add(params IModule[] modules) => _modules.Add(modules);

        public void Insert(int index, params IModule[] modules) => _modules.Insert(index, modules);

        public int IndexOf(string name) => _modules.IndexOf(name);

        public bool IsReadOnly => _modules.IsReadOnly;

        public int IndexOf(IModule item) => _modules.IndexOf(item);

        public void Insert(int index, IModule item) => _modules.Insert(index, item);

        public void RemoveAt(int index) => _modules.RemoveAt(index);

        public IModule this[int index]
        {
            get { return _modules[index]; }
            set { _modules[index] = value; }
        }

        public bool Contains(string name) => _modules.Contains(name);

        public bool TryGetValue(string name, out IModule value) => _modules.TryGetValue(name, out value);

        public IModule this[string name] => _modules[name];

        public void Add(string name, IModule module) => _modules.Add(name, module);

        public void Insert(int index, string name, IModule module) => _modules.Insert(index, name, module);

        public IEnumerable<KeyValuePair<string, IModule>> AsKeyValuePairs() => _modules.AsKeyValuePairs();
    }
}
