using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.Jint;
using JSPool;
using Wyam.Common.Caching;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.JavaScript;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Shortcodes;
using Wyam.Common.Tracing;
using Wyam.Common.Util;
using Wyam.Core.Documents;
using Wyam.Core.JavaScript;
using Wyam.Core.Meta;
using Wyam.Core.Shortcodes;

namespace Wyam.Core.Execution
{
    internal class ExecutionContext : IExecutionContext, IDisposable
    {
        // Cache the HttpMessageHandler (the HttpClient is really just a thin wrapper around this)
        private static readonly HttpMessageHandler _httpMessageHandler = new HttpClientHandler();

        private readonly ExecutionPipeline _pipeline;

        private bool _disposed;

        public Engine Engine { get; }

        public Guid ExecutionId { get; }

        public IReadOnlyCollection<byte[]> DynamicAssemblies => Engine.DynamicAssemblies;

        public IReadOnlyCollection<string> Namespaces => Engine.Namespaces;

        public IReadOnlyPipeline Pipeline => new ReadOnlyPipeline(_pipeline);

        public IModule Module { get; }

        public IDocumentCollection Documents => Engine.Documents;

        public IReadOnlyFileSystem FileSystem => Engine.FileSystem;

        public IReadOnlySettings Settings => Engine.Settings;

        public IReadOnlyShortcodeCollection Shortcodes => Engine.Shortcodes;

        public IExecutionCache ExecutionCache => Engine.ExecutionCacheManager.Get(Module, Settings);

        public string ApplicationInput => Engine.ApplicationInput;

        public ExecutionContext(Engine engine, Guid executionId, ExecutionPipeline pipeline)
        {
            Engine = engine;
            ExecutionId = executionId;
            _pipeline = pipeline;
        }

        private ExecutionContext(ExecutionContext original, IModule module)
        {
            Engine = original.Engine;
            ExecutionId = original.ExecutionId;
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

        public Stream GetContentStream(string content = null) => Engine.ContentStreamFactory.GetStream(this, content);

        public HttpClient CreateHttpClient() => CreateHttpClient(_httpMessageHandler);

        public HttpClient CreateHttpClient(HttpMessageHandler handler) => new HttpClient(handler, false)
        {
            Timeout = TimeSpan.FromSeconds(60)
        };

        // GetDocument

        public IDocument GetDocument(FilePath source, Stream stream, IEnumerable<KeyValuePair<string, object>> items = null, bool disposeStream = true) =>
            GetDocument((IDocument)null, source, stream, items, disposeStream);

        public IDocument GetDocument(FilePath source, IEnumerable<KeyValuePair<string, object>> items = null) =>
            GetDocument((IDocument)null, source, (Stream)null, items);

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

        public IDocument GetDocument(IDocument sourceDocument, FilePath source, IEnumerable<KeyValuePair<string, object>> items = null)
        {
            CheckDisposed();
            IDocument document = Engine.DocumentFactory.GetDocument(this, sourceDocument, source, items);
            if (sourceDocument != null && sourceDocument.Source == null)
            {
                // Only add a new source if the source document didn't already contain one (otherwise the one it contains will be used)
                _pipeline.AddDocumentSource(source);
            }
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

        public IJavaScriptEnginePool GetJavaScriptEnginePool(
            Action<IJavaScriptEngine> initializer = null,
            int startEngines = 10,
            int maxEngines = 25,
            int maxUsagesPerEngine = 100,
            TimeSpan? engineTimeout = null) =>
            new JavaScriptEnginePool(
                initializer,
                startEngines,
                maxEngines,
                maxUsagesPerEngine,
                engineTimeout ?? TimeSpan.FromSeconds(5));

        public IShortcodeResult GetShortcodeResult(string content, IEnumerable<KeyValuePair<string, object>> metadata = null)
            => GetShortcodeResult(content == null ? null : GetContentStream(content), metadata);

        public IShortcodeResult GetShortcodeResult(Stream content, IEnumerable<KeyValuePair<string, object>> metadata = null)
            => new ShortcodeResult(content, metadata);

        // IMetadata

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => Settings.GetEnumerator();

        public int Count => Settings.Count;

        public bool ContainsKey(string key) => Settings.ContainsKey(key);

        public object this[string key] => Settings[key];

        public IEnumerable<string> Keys => Settings.Keys;

        public IEnumerable<object> Values => Settings.Values;

        public IMetadata<T> MetadataAs<T>() => Settings.MetadataAs<T>();

        public object Get(string key, object defaultValue = null) => Settings.Get(key, defaultValue);

        public object GetRaw(string key) => Settings.Get(key);

        public T Get<T>(string key) => Settings.Get<T>(key);

        public T Get<T>(string key, T defaultValue) => Settings.Get(key, defaultValue);

        public bool TryGetValue(string key, out object value) => Settings.TryGetValue(key, out value);

        public bool TryGetValue<T>(string key, out T value) => Settings.TryGetValue<T>(key, out value);

        public IMetadata GetMetadata(params string[] keys) => Settings.GetMetadata(keys);
    }
}
