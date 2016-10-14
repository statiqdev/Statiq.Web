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
    internal class Pipeline : IPipeline, IDisposable
    {
        private ConcurrentBag<IDocument> _clonedDocuments = new ConcurrentBag<IDocument>();
        private readonly List<IModule> _modules = new List<IModule>();
        private readonly ConcurrentHashSet<FilePath> _documentSources = new ConcurrentHashSet<FilePath>();
        private readonly Cache<List<IDocument>>  _previouslyProcessedCache;
        private readonly Dictionary<FilePath, List<IDocument>> _processedSources; 
        private bool _disposed;

        public string Name { get; }
        public bool ProcessDocumentsOnce => _previouslyProcessedCache != null;

        public Pipeline(string name, IModule[] modules)
            : this(name, false, modules)
        {
        }

        public Pipeline(string name, bool processDocumentsOnce, IModule[] modules)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(nameof(name));
            }
            Name = name;
            if (processDocumentsOnce)
            {
                _previouslyProcessedCache = new Cache<List<IDocument>>();
                _processedSources = new Dictionary<FilePath, List<IDocument>>();
            }
            if (modules != null)
            {
                _modules.AddRange(modules);
            }
        }

        public void Add(IModule item)
        {
            _modules.Add(item);
        }

        public void Add(params IModule[] items)
        {
            _modules.AddRange(items);
        }

        public void Clear()
        {
            _modules.Clear();
        }

        public bool Contains(IModule item)
        {
            return _modules.Contains(item);
        }

        public void CopyTo(IModule[] array, int arrayIndex)
        {
            _modules.CopyTo(array, arrayIndex);
        }

        public bool Remove(IModule item)
        {
            return _modules.Remove(item);
        }

        public int Count => _modules.Count;

        public bool IsReadOnly => false;

        public IEnumerator<IModule> GetEnumerator()
        {
            return _modules.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int IndexOf(IModule item)
        {
            return _modules.IndexOf(item);
        }

        public void Insert(int index, IModule item)
        {
            _modules.Insert(index, item);
        }

        public void Insert(int index, params IModule[] items)
        {
            _modules.InsertRange(index, items);
        }

        public void RemoveAt(int index)
        {
            _modules.RemoveAt(index);
        }

        public IModule this[int index]
        {
            get { return _modules[index]; }
            set { _modules[index] = value; }
        }

        // This is the main execute method called by the engine
        public void Execute(Engine engine)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(Pipeline));
            }

            // Setup for pipeline execution
            _documentSources.Clear();
            ResetClonedDocuments();
            _previouslyProcessedCache?.ResetEntryHits();
            _processedSources?.Clear();

            // Execute all modules in the pipeline
            IReadOnlyList<IDocument> resultDocuments;
            using (ExecutionContext context = new ExecutionContext(engine, this))
            {
                ImmutableArray<IDocument> inputs = new[] {engine.DocumentFactory.GetDocument(context)}.ToImmutableArray();
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
                    List<IDocument> processedResultDocuments;
                    if (_processedSources.TryGetValue(resultDocument.Source, out processedResultDocuments))
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
        public IReadOnlyList<IDocument> Execute(ExecutionContext context, IEnumerable<IModule> modules, ImmutableArray<IDocument> documents)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(Pipeline));
            }

            foreach (IModule module in modules.Where(x => x != null))
            {
                string moduleName = module.GetType().Name;
                System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
                using (Trace.WithIndent().Verbose("Executing module {0} with {1} input document(s)", moduleName, documents.Length))
                {
                    try
                    {
                        // Execute the module
                        IEnumerable<IDocument> outputs;
                        using (ExecutionContext moduleContext = context.Clone(module))
                        {
                            outputs = module.Execute(documents, moduleContext);
                        }
                        documents = outputs?.Where(x => x != null).ToImmutableArray() ?? ImmutableArray<IDocument>.Empty;

                        // Remove any documents that were previously processed (checking will also mark the cache entry as hit)
                        if (_previouslyProcessedCache != null && _processedSources != null)
                        {
                            ImmutableArray<IDocument>.Builder newDocuments = ImmutableArray.CreateBuilder<IDocument>();
                            foreach (IDocument document in documents)
                            {
                                if (_processedSources.ContainsKey(document.Source))
                                {
                                    // We've seen this source before and already added it to the processed cache
                                    newDocuments.Add(document);
                                }
                                else
                                {
                                    List<IDocument> processedDocuments;
                                    if (!_previouslyProcessedCache.TryGetValue(document, out processedDocuments))
                                    {
                                        // This document was not previously processed, so add it to the current result and set up a list to track final results
                                        newDocuments.Add(document);
                                        processedDocuments = new List<IDocument>();
                                        _previouslyProcessedCache.Set(document, processedDocuments);
                                        _processedSources.Add(document.Source, processedDocuments);
                                    }
                                    // Otherwise, this document was previously processed so don't add it to the results
                                }
                            }
                            if (newDocuments.Count != documents.Length)
                            {
                                Trace.Verbose("Removed {0} previously processed document(s)", documents.Length - newDocuments.Count);
                            }
                            documents = newDocuments.ToImmutable();
                        }

                        // Set results in engine and trace
                        context.Engine.DocumentCollection.Set(Name, documents);
                        stopwatch.Stop();
                        Trace.Verbose("Executed module {0} in {1} ms resulting in {2} output document(s)",
                            moduleName, stopwatch.ElapsedMilliseconds, documents.Length);
                    }
                    catch (Exception)
                    {
                        Trace.Error("Error while executing module {0}", moduleName);
                        documents = ImmutableArray<IDocument>.Empty;
                        context.Engine.DocumentCollection.Set(Name, documents);
                        throw;
                    }
                }
            }
            return documents;
        }

        private void FlattenResultDocuments(IEnumerable<IDocument> documents, HashSet<IDocument> flattenedResultDocuments)
        {
            foreach(IDocument document in documents)
            {
                if (document == null || !flattenedResultDocuments.Add(document))
                {
                    return;
                }
                
                FlattenResultDocuments(
                    document.Keys.SelectMany(x =>
                    {
                        object value = document.GetRaw(x);
                        IEnumerable<IDocument> children = value as IEnumerable<IDocument>;
                        if (children == null && value is IDocument)
                        {
                            children = new[] {(IDocument) value};
                        }
                        return children ?? Enumerable.Empty<IDocument>();
                    }),
                    flattenedResultDocuments);
            };
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
                throw new ObjectDisposedException(nameof(Pipeline));
            }
            _disposed = true;
            ResetClonedDocuments();
            if (_previouslyProcessedCache != null)
            {
                Parallel.ForEach(_previouslyProcessedCache.GetValues().SelectMany(x => x), x => x.Dispose());
            }
        }
    }
}
