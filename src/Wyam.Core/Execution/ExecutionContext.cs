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
    internal class ExecutionContext : IExecutionContext
    {
        private readonly Pipeline _pipeline;
        
        public Engine Engine { get; }

        public IEnumerable<byte[]> RawAssemblies => Engine.RawAssemblies;

        public IEnumerable<Assembly> Assemblies => Engine.Assemblies;

        public IEnumerable<string> Namespaces => Engine.Namespaces;

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

        internal ExecutionContext Clone(IModule module)
        {
            return new ExecutionContext(this, module);
        }

        public bool TryConvert<T>(object value, out T result)
        {
            return TypeHelper.TryConvert(value, out result);
        }

        // GetLink

        public string GetLink(IMetadata metadata, bool includeHost = false)
        {
            return GetLink(metadata, Common.Meta.Keys.RelativeFilePath, includeHost);
        }

        public string GetLink(IMetadata metadata, string key, bool includeHost = false)
        {
            return GetLink(metadata.FilePath(key), includeHost);
        }

        public string GetLink(NormalizedPath path, bool includeHost = false)
        {
            return GetLink(path, includeHost ? Settings.Host : null, Settings.LinkRoot, 
                Settings.LinkHideIndexPages, Settings.LinkHideExtensions);
        }

        public string GetLink(NormalizedPath path, string host, DirectoryPath root, bool hideIndexPages, bool hideExtensions)
        {
            return LinkGenerator.GetLink(path, host, root, hideIndexPages, hideExtensions);
        }

        // GetDocument

        public IDocument GetDocument(FilePath source, string content, IEnumerable<KeyValuePair<string, object>> items = null)
        {
            return GetDocument((IDocument)null, source, content, items);
        }

        public IDocument GetDocument(string content, IEnumerable<KeyValuePair<string, object>> items = null)
        {
            return GetDocument((IDocument)null, content, items);
        }

        public IDocument GetDocument(FilePath source, Stream stream, IEnumerable<KeyValuePair<string, object>> items = null, bool disposeStream = true)
        {
            return GetDocument((IDocument)null, source, stream, items, disposeStream);
        }

        public IDocument GetDocument(Stream stream, IEnumerable<KeyValuePair<string, object>> items = null, bool disposeStream = true)
        {
            return GetDocument((IDocument)null, stream, items, disposeStream);
        }

        public IDocument GetDocument(IEnumerable<KeyValuePair<string, object>> items)
        {
            return GetDocument((IDocument)null, items);
        }

        // IDocumentFactory

        public IDocument GetDocument()
        {
            IDocument document = Engine.DocumentFactory.GetDocument(this);
            _pipeline.AddClonedDocument(document);
            return document;
        }

        public IDocument GetDocument(IDocument sourceDocument, FilePath source, string content, IEnumerable<KeyValuePair<string, object>> items = null)
        {
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
            IDocument document = Engine.DocumentFactory.GetDocument(this, sourceDocument, content, items);
            _pipeline.AddClonedDocument(document);
            return document;
        }

        public IDocument GetDocument(IDocument sourceDocument, FilePath source, Stream stream, IEnumerable<KeyValuePair<string, object>> items = null, bool disposeStream = true)
        {
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
            IDocument document = Engine.DocumentFactory.GetDocument(this, sourceDocument, stream, items, disposeStream);
            _pipeline.AddClonedDocument(document);
            return document;
        }

        public IDocument GetDocument(IDocument sourceDocument, IEnumerable<KeyValuePair<string, object>> items)
        {
            IDocument document = Engine.DocumentFactory.GetDocument(this, sourceDocument, items);
            _pipeline.AddClonedDocument(document);
            return document;
        }

        public IReadOnlyList<IDocument> Execute(IEnumerable<IModule> modules, IEnumerable<IDocument> inputs)
        {
            return Execute(modules, inputs, null);
        }

        // Executes the module with an empty document containing the specified metadata items
        public IReadOnlyList<IDocument> Execute(IEnumerable<IModule> modules, IEnumerable<KeyValuePair<string, object>> items = null)
        {
            return Execute(modules, null, items);
        }

        public IReadOnlyList<IDocument> Execute(IEnumerable<IModule> modules, IEnumerable<MetadataItem> items)
        {
            return Execute(modules, items?.Select(x => x.Pair));
        }

        private IReadOnlyList<IDocument> Execute(IEnumerable<IModule> modules, IEnumerable<IDocument> inputs, IEnumerable<KeyValuePair<string, object>> items)
        {
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
