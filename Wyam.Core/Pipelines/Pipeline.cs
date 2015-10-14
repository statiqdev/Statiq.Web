using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Wyam.Common;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Pipelines;
using Wyam.Core.Caching;
using Wyam.Core.Documents;

namespace Wyam.Core.Pipelines
{
    internal class Pipeline : IPipeline, IDisposable
    {
        private ConcurrentBag<Document> _clonedDocuments = new ConcurrentBag<Document>();
        private readonly Engine _engine;
        private readonly List<IModule> _modules = new List<IModule>();
        private readonly HashSet<string> _documentSources = new HashSet<string>(); 
        private readonly Cache<List<Document>>  _previouslyProcessedCache;
        private readonly Dictionary<string, List<Document>> _processedSources; 
        private bool _disposed;

        public string Name { get; }
        public bool ProcessDocumentsOnce => _previouslyProcessedCache != null;

        public Pipeline(string name, Engine engine, IModule[] modules)
            : this(name, false, engine, modules)
        {
        }

        public Pipeline(string name, bool processDocumentsOnce, Engine engine, IModule[] modules)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(nameof(name));
            }
            if (engine == null)
            {
                throw new ArgumentNullException(nameof(engine));
            }
            Name = name;
            if (processDocumentsOnce)
            {
                _previouslyProcessedCache = new Cache<List<Document>>();
                _processedSources = new Dictionary<string, List<Document>>();
            }
            _engine = engine;
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
                Stopwatch stopwatch = Stopwatch.StartNew();
                using(_engine.Trace.WithIndent().Verbose("Executing module {0} with {1} input document(s)", moduleName, documents.Length))
                {
                    try
                    {
                        // Execute the module
                        IEnumerable<IDocument> outputs = module.Execute(documents, context.Clone(module));
                        documents = outputs?.Where(x => x != null).ToImmutableArray() ?? ImmutableArray<IDocument>.Empty;

                        // Remove any documents that were previously processed (checking will also mark the cache entry as hit)
                        if (_previouslyProcessedCache != null)
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
                                    List<Document> processedDocuments;
                                    if (!_previouslyProcessedCache.TryGetValue(document, out processedDocuments))
                                    {
                                        // This document was not previously processed, so add it to the current result and set up a list to track final results
                                        newDocuments.Add(document);
                                        processedDocuments = new List<Document>();
                                        _previouslyProcessedCache.Set(document, processedDocuments);
                                        _processedSources.Add(document.Source, processedDocuments);
                                    }
                                    // Otherwise, this document was previously processed so don't add it to the results
                                }
                            }
                            if (newDocuments.Count != documents.Length)
                            {
                                _engine.Trace.Verbose("Removed {0} previously processed document(s)", documents.Length - newDocuments.Count);
                            }
                            documents = newDocuments.ToImmutable();
                        }

                        // Set results in engine and trace
                        _engine.DocumentCollection.Set(Name, documents);
                        stopwatch.Stop();
                        _engine.Trace.Verbose("Executed module {0} in {1} ms resulting in {2} output document(s)", 
                            moduleName, stopwatch.ElapsedMilliseconds, documents.Length);
                    }
                    catch (Exception ex)
                    {
                        _engine.Trace.Error("Error while executing module {0}: {1}", moduleName, ex.ToString());
                        documents = ImmutableArray<IDocument>.Empty;
                        _engine.DocumentCollection.Set(Name, documents);
                        break;
                    }
                }
            }
            return documents;
        }

        // This is the main execute method called by the engine
        public void Execute()
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
            ExecutionContext context = new ExecutionContext(_engine, this);
            ImmutableArray<IDocument> inputs = new IDocument[] { new Document(_engine, this) }.ToImmutableArray();
            IReadOnlyList<IDocument> resultDocuments = Execute(context, _modules, inputs);

            // Dispose documents that aren't part of the final collection for this pipeline
            foreach (Document document in _clonedDocuments.Where(x => !resultDocuments.Contains(x)))
            {
                document.Dispose();
            }
            _clonedDocuments = new ConcurrentBag<Document>(resultDocuments.OfType<Document>());

            // Check the previously processed cache for any previously processed documents that need to be added
            if (_previouslyProcessedCache != null)
            {
                // Dispose the previously processed documents that we didn't get this time around
                foreach (Document unhitDocument in _previouslyProcessedCache.ClearUnhitEntries().SelectMany(x => x))
                {
                    unhitDocument.Dispose();
                }

                // Trace the number of previously processed documents
                _engine.Trace.Verbose("{0} previously processed document(s) were not reprocessed", _previouslyProcessedCache.GetValues().Sum(x => x.Count));

                // Add new result documents to the cache
                foreach (Document resultDocument in _clonedDocuments)
                {
                    List<Document> processedResultDocuments;
                    if (_processedSources.TryGetValue(resultDocument.Source, out processedResultDocuments))
                    {
                        processedResultDocuments.Add(resultDocument);
                    }
                    else
                    {
                        _engine.Trace.Warning("Could not find processed document cache for source {0}, please report this warning to the developers", resultDocument.Source);
                    }
                }

                // Reset cloned documents (since we're tracking them in the previously processed cache now) and set new aggregate results
                _clonedDocuments = new ConcurrentBag<Document>();
                _engine.DocumentCollection.Set(Name, _previouslyProcessedCache.GetValues().SelectMany(x => x).Cast<IDocument>().ToList().AsReadOnly());
            }
        }

        public void AddClonedDocument(Document document) => _clonedDocuments.Add(document);

        public void AddDocumentSource(string source)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                throw new ArgumentException(nameof(source));
            }
            if (!_documentSources.Add(source))
            {
                throw new ArgumentException("Document source must be unique within the pipeline: " + source);
            }
        }

        public void ResetClonedDocuments()
        {
            foreach (Document document in _clonedDocuments)
            {
                document.Dispose();
            }
            _clonedDocuments = new ConcurrentBag<Document>();
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
                foreach (Document document in _previouslyProcessedCache.GetValues().SelectMany(x => x))
                {
                    document.Dispose();
                }
            }
        }
    }
}
