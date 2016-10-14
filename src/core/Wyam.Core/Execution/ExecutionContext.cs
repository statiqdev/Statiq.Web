using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using Wyam.Common.Caching;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Execution;
using Wyam.Core.Meta;

namespace Wyam.Core.Execution
{
    internal class ExecutionContext : IExecutionContext, IDisposable
    {
        private readonly Pipeline _pipeline;

        private bool _disposed;
        
        public Engine Engine { get; }

        public IReadOnlyCollection<byte[]> DynamicAssemblies => Engine.DynamicAssemblies;
        
        public IReadOnlyCollection<string> Namespaces => Engine.Namespaces;

        public IReadOnlyPipeline Pipeline => new ReadOnlyPipeline(_pipeline);

        public IModule Module { get; }

        public IDocumentCollection Documents => Engine.Documents;

        public IReadOnlyFileSystem FileSystem => Engine.FileSystem;

        public IReadOnlySettings Settings => Engine.Settings;

        public IExecutionCache ExecutionCache => Engine.ExecutionCacheManager.Get(Module, Settings);

        public string ApplicationInput => Engine.ApplicationInput;

        public IMetadata GlobalMetadata => Engine.GlobalMetadata; 

        public ExecutionContext(Engine engine, Pipeline pipeline)
        {
            Engine = engine;
            _pipeline = pipeline;
        }

        private ExecutionContext(ExecutionContext original, IModule module)
        {
            Engine = original.Engine;
            _pipeline = original._pipeline;
            Module = module;
        }

        internal ExecutionContext Clone(IModule module) => new ExecutionContext(this, module);


        /// <summary>
        /// The context is disposed after use by each module to ensure modules aren't accessing stale data
        /// if they continue to create documents or perform other operations after the module is done
        /// executing. A disposed context can no longer be used.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                _disposed = true;
            }
        }

        private void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ExecutionContext));
            }
        }

        public bool TryConvert<T>(object value, out T result) => TypeHelper.TryConvert(value, out result);

        // GetLink

        public string GetLink() => 
            GetLink(null, Settings.Host, Settings.LinkRoot, Settings.LinksUseHttps, false, false);

        public string GetLink(IMetadata metadata, bool includeHost = false) => 
            GetLink(metadata, Common.Meta.Keys.RelativeFilePath, includeHost);

        public string GetLink(IMetadata metadata, string key, bool includeHost = false)
        {
            FilePath filePath = metadata?.FilePath(key);
            return filePath != null ? GetLink(filePath, includeHost) : null;
        }

        public string GetLink(NormalizedPath path, bool includeHost = false) => 
            GetLink(path, includeHost ? Settings.Host : null, Settings.LinkRoot, Settings.LinksUseHttps, Settings.LinkHideIndexPages, Settings.LinkHideExtensions);

        public string GetLink(NormalizedPath path, string host, DirectoryPath root, bool useHttps, bool hideIndexPages, bool hideExtensions) => 
            LinkGenerator.GetLink(path, host, root, useHttps, hideIndexPages, hideExtensions);

        // GetDocument

        public IDocument GetDocument(FilePath source, string content, IEnumerable<KeyValuePair<string, object>> items = null) => 
            GetDocument((IDocument)null, source, content, items);

        public IDocument GetDocument(string content, IEnumerable<KeyValuePair<string, object>> items = null) => 
            GetDocument((IDocument)null, content, items);

        public IDocument GetDocument(FilePath source, Stream stream, IEnumerable<KeyValuePair<string, object>> items = null, bool disposeStream = true) => 
            GetDocument((IDocument)null, source, stream, items, disposeStream);

        public IDocument GetDocument(Stream stream, IEnumerable<KeyValuePair<string, object>> items = null, bool disposeStream = true) => 
            GetDocument((IDocument)null, stream, items, disposeStream);

        public IDocument GetDocument(IEnumerable<KeyValuePair<string, object>> items) => 
            GetDocument((IDocument)null, items);

        // IDocumentFactory

        public IDocument GetDocument()
        {
            CheckDisposed();
            IDocument document = Engine.DocumentFactory.GetDocument(this);
            _pipeline.AddClonedDocument(document);
            return document;
        }

        public IDocument GetDocument(IDocument sourceDocument, FilePath source, string content, IEnumerable<KeyValuePair<string, object>> items = null)
        {
            CheckDisposed();
            IDocument document = Engine.DocumentFactory.GetDocument(this, sourceDocument, source, content, items);
            if (sourceDocument != null && sourceDocument.Source == null)
            {
                // Only add a new source if the source document didn't already contain one (otherwise the one it contains will be used)
                _pipeline.AddDocumentSource(source);
            }
            _pipeline.AddClonedDocument(document);
            return document;
        }

        public IDocument GetDocument(IDocument sourceDocument, string content, IEnumerable<KeyValuePair<string, object>> items = null)
        {
            CheckDisposed();
            IDocument document = Engine.DocumentFactory.GetDocument(this, sourceDocument, content, items);
            _pipeline.AddClonedDocument(document);
            return document;
        }

        public IDocument GetDocument(IDocument sourceDocument, FilePath source, Stream stream, IEnumerable<KeyValuePair<string, object>> items = null, bool disposeStream = true)
        {
            CheckDisposed();
            IDocument document = Engine.DocumentFactory.GetDocument(this, sourceDocument, source, stream, items, disposeStream);
            if (sourceDocument != null && sourceDocument.Source == null)
            {
                // Only add a new source if the source document didn't already contain one (otherwise the one it contains will be used)
                _pipeline.AddDocumentSource(source);
            }
            _pipeline.AddClonedDocument(document);
            return document;
        }

        public IDocument GetDocument(IDocument sourceDocument, Stream stream, IEnumerable<KeyValuePair<string, object>> items = null, bool disposeStream = true)
        {
            CheckDisposed();
            IDocument document = Engine.DocumentFactory.GetDocument(this, sourceDocument, stream, items, disposeStream);
            _pipeline.AddClonedDocument(document);
            return document;
        }

        public IDocument GetDocument(IDocument sourceDocument, IEnumerable<KeyValuePair<string, object>> items)
        {
            CheckDisposed();
            IDocument document = Engine.DocumentFactory.GetDocument(this, sourceDocument, items);
            _pipeline.AddClonedDocument(document);
            return document;
        }

        public IReadOnlyList<IDocument> Execute(IEnumerable<IModule> modules, IEnumerable<IDocument> inputs) => 
            Execute(modules, inputs, null);

        // Executes the module with an empty document containing the specified metadata items
        public IReadOnlyList<IDocument> Execute(IEnumerable<IModule> modules, IEnumerable<KeyValuePair<string, object>> items = null) => 
            Execute(modules, null, items);

        public IReadOnlyList<IDocument> Execute(IEnumerable<IModule> modules, IEnumerable<MetadataItem> items) => 
            Execute(modules, items?.Select(x => x.Pair));

        private IReadOnlyList<IDocument> Execute(IEnumerable<IModule> modules, IEnumerable<IDocument> inputs, IEnumerable<KeyValuePair<string, object>> items)
        {
            CheckDisposed();

            if (modules == null)
            {
                return ImmutableArray<IDocument>.Empty;
            }

            // Store the document list before executing the child modules and restore it afterwards
            IReadOnlyList<IDocument> originalDocuments = Engine.DocumentCollection.Get(_pipeline.Name);
            ImmutableArray<IDocument> documents = inputs?.ToImmutableArray()
                ?? new[] { GetDocument(items) }.ToImmutableArray();
            IReadOnlyList<IDocument> results = _pipeline.Execute(this, modules, documents);
            Engine.DocumentCollection.Set(_pipeline.Name, originalDocuments);
            return results;
        }

        // IMetadata

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => GlobalMetadata.GetEnumerator();

        public int Count => GlobalMetadata.Count;

        public bool ContainsKey(string key) => GlobalMetadata.ContainsKey(key);

        public bool TryGetValue(string key, out object value) => GlobalMetadata.TryGetValue(key, out value);

        public object this[string key] => GlobalMetadata[key];

        public IEnumerable<string> Keys => GlobalMetadata.Keys;

        public IEnumerable<object> Values => GlobalMetadata.Values;

        public IMetadata<T> MetadataAs<T>() => GlobalMetadata.MetadataAs<T>();

        public object Get(string key, object defaultValue = null) => GlobalMetadata.Get(key, defaultValue);

        public object GetRaw(string key) => GlobalMetadata.Get(key);

        public T Get<T>(string key) => GlobalMetadata.Get<T>(key);

        public T Get<T>(string key, T defaultValue) => GlobalMetadata.Get(key, defaultValue);

        public string String(string key, string defaultValue = null) => GlobalMetadata.String(key, defaultValue);

        public FilePath FilePath(string key, FilePath defaultValue = null) => GlobalMetadata.FilePath(key, defaultValue);

        public DirectoryPath DirectoryPath(string key, DirectoryPath defaultValue = null) => GlobalMetadata.DirectoryPath(key, defaultValue);

        public IReadOnlyList<T> List<T>(string key, IReadOnlyList<T> defaultValue = null) => GlobalMetadata.List(key, defaultValue);

        public IDocument Document(string key) => GlobalMetadata.Document(key);

        public IReadOnlyList<IDocument> DocumentList(string key) => GlobalMetadata.DocumentList(key);

        public dynamic Dynamic(string key, object defaultValue = null) => GlobalMetadata.Dynamic(key, defaultValue);
    }
}
