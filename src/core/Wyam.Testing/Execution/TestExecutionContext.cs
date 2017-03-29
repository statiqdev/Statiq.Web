using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Common.Caching;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.JavaScript;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Tracing;
using Wyam.Common.Util;
using Wyam.Testing.Caching;
using Wyam.Testing.Configuration;
using Wyam.Testing.Documents;
using Wyam.Testing.IO;
using Wyam.Testing.Meta;

namespace Wyam.Testing.Execution
{
    /// <summary>
    /// An <see cref="IExecutionContext"/> that can be used for testing.
    /// </summary>
    public class TestExecutionContext : IExecutionContext
    {
        private readonly TestSettings _settings = new TestSettings();

        /// <inheritdoc/>
        public IDocument GetDocument() => new TestDocument();

        /// <inheritdoc/>
        public IDocument GetDocument(FilePath source, string content, IEnumerable<KeyValuePair<string, object>> items = null)
        {
            throw new NotSupportedException("This method is obsolete, please use the stream version");
        }

        /// <inheritdoc/>
        public IDocument GetDocument(string content, IEnumerable<KeyValuePair<string, object>> items = null)
        {
            throw new NotSupportedException("This method is obsolete, please use the stream version");
        }

        /// <inheritdoc/>
        public IDocument GetDocument(IDocument sourceDocument, FilePath source, string content, IEnumerable<KeyValuePair<string, object>> items = null)
        {
            throw new NotSupportedException("This method is obsolete, please use the stream version");
        }

        /// <inheritdoc/>
        public IDocument GetDocument(IDocument sourceDocument, string content, IEnumerable<KeyValuePair<string, object>> items = null)
        {
            throw new NotSupportedException("This method is obsolete, please use the stream version");
        }

        /// <inheritdoc/>
        public IDocument GetDocument(IEnumerable<KeyValuePair<string, object>> items) => new TestDocument(items);

        /// <inheritdoc/>
        public IDocument GetDocument(IDocument sourceDocument, IEnumerable<KeyValuePair<string, object>> items)
        {
            return new TestDocument(items == null ? sourceDocument : sourceDocument.Concat(items))
            {
                Id = sourceDocument.Id,
                Source = sourceDocument.Source,
                Content = sourceDocument.Content
            };
        }

        /// <inheritdoc/>
        public IDocument GetDocument(
            IDocument sourceDocument,
            FilePath source,
            Stream stream,
            IEnumerable<KeyValuePair<string, object>> items = null,
            bool disposeStream = true)
        {
            return new TestDocument(items == null ? sourceDocument : sourceDocument.Concat(items))
            {
                Id = sourceDocument.Id,
                Source = source,
                Content = GetContent(stream)
            };
        }

        /// <inheritdoc/>
        public IDocument GetDocument(FilePath source, Stream stream, IEnumerable<KeyValuePair<string, object>> items = null, bool disposeStream = true)
        {
            return new TestDocument(items)
            {
                Source = source,
                Content = GetContent(stream)
            };
        }

        /// <inheritdoc/>
        public IDocument GetDocument(IDocument sourceDocument, Stream stream, IEnumerable<KeyValuePair<string, object>> items = null, bool disposeStream = true)
        {
            return new TestDocument(items == null ? sourceDocument : sourceDocument.Concat(items))
            {
                Id = sourceDocument.Id,
                Source = sourceDocument.Source,
                Content = GetContent(stream)
            };
        }

        /// <inheritdoc/>
        public IDocument GetDocument(Stream stream, IEnumerable<KeyValuePair<string, object>> items = null, bool disposeStream = true)
        {
            return new TestDocument(items)
            {
                Content = GetContent(stream)
            };
        }

        /// <inheritdoc/>
        public IDocument GetDocument(IDocument sourceDocument, FilePath source, IEnumerable<KeyValuePair<string, object>> items = null)
        {
            return GetDocument(sourceDocument, source, (Stream)null, items);
        }

        private string GetContent(Stream stream)
        {
            if (stream == null)
            {
                return string.Empty;
            }
            stream.Position = 0;
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8, true, 4096, true))
            {
                return reader.ReadToEnd();
            }
        }

        /// <inheritdoc/>
        public IReadOnlyCollection<byte[]> DynamicAssemblies { get; set; } = new List<byte[]>();

        /// <inheritdoc/>
        public IReadOnlyCollection<string> Namespaces { get; set; } = new List<string>();

        /// <inheritdoc/>
        public IReadOnlyPipeline Pipeline { get; set; }

        /// <inheritdoc/>
        public IModule Module { get; set; }

        /// <inheritdoc/>
        public IExecutionCache ExecutionCache { get; set; } = new TestExecutionCache();

        /// <inheritdoc/>
        public IReadOnlyFileSystem FileSystem { get; set; } = new TestFileSystem();

        /// <inheritdoc/>
        public IDocumentCollection Documents { get; set; }

        /// <inheritdoc/>
        [Obsolete]
        public IMetadata GlobalMetadata { get; set; }

        /// <inheritdoc/>
        public string ApplicationInput { get; set; }

        /// <inheritdoc/>
        public ISettings Settings => _settings;

        IReadOnlySettings IExecutionContext.Settings => Settings;

        /// <inheritdoc/>
        public Stream GetContentStream(string content = null) =>
            string.IsNullOrEmpty(content) ? new MemoryStream() : new MemoryStream(Encoding.UTF8.GetBytes(content));

        /// <inheritdoc/>
        public bool TryConvert<T>(object value, out T result)
        {
            if (value is T)
            {
                result = (T)value;
                return true;
            }
            result = default(T);
            return value == null;
        }

        /// <inheritdoc/>
        public IReadOnlyList<IDocument> Execute(IEnumerable<IModule> modules, IEnumerable<IDocument> inputs) =>
            Execute(modules, inputs, null);

        // Executes the module with an empty document containing the specified metadata items
        /// <inheritdoc/>
        public IReadOnlyList<IDocument> Execute(IEnumerable<IModule> modules, IEnumerable<KeyValuePair<string, object>> items = null) =>
            Execute(modules, null, items);

        /// <inheritdoc/>
        public IReadOnlyList<IDocument> Execute(IEnumerable<IModule> modules, IEnumerable<MetadataItem> items) =>
            Execute(modules, items?.Select(x => x.Pair));

        private IReadOnlyList<IDocument> Execute(IEnumerable<IModule> modules, IEnumerable<IDocument> inputs, IEnumerable<KeyValuePair<string, object>> items)
        {
            if (modules == null)
            {
                return Array.Empty<IDocument>();
            }
            foreach (IModule module in modules)
            {
                inputs = module.Execute(inputs.ToList(), this);
            }
            return inputs.ToList();
        }

        public Func<IJsEngine> JsEngineFunc = () =>
        {
            throw new NotImplementedException("JavaScript test engine not initialized. Wyam.Testing.JavaScript can be used to return a working JavaScript engine");
        };

        /// <inheritdoc/>
        public IJsEnginePool GetJsEnginePool(Action<IJsEngine> initializer = null,
            int startEngines = 10, int maxEngines = 25,
            int maxUsagesPerEngine = 100, TimeSpan? engineTimeout = null) =>
            new TestJsEnginePool(JsEngineFunc, initializer);

        private class TestJsEnginePool : IJsEnginePool
        {
            private readonly Func<IJsEngine> _engineFunc;
            private readonly Action<IJsEngine> _initializer;

            public TestJsEnginePool(Func<IJsEngine> engineFunc, Action<IJsEngine> initializer)
            {
                _engineFunc = engineFunc;
                _initializer = initializer;
            }

            public IJsEngine GetEngine(TimeSpan? timeout = null)
            {
                IJsEngine engine = _engineFunc();
                _initializer?.Invoke(engine);
                return engine;
            }

            public void Dispose()
            {
            }

            public void RecycleEngine(IJsEngine engine)
            {
                throw new NotImplementedException();
            }

            public void RecycleAllEngines()
            {
                throw new NotImplementedException();
            }
        }

        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _settings.GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _settings).GetEnumerator();
        }

        /// <inheritdoc/>
        public int Count => _settings.Count;

        /// <inheritdoc/>
        public bool ContainsKey(string key) => _settings.ContainsKey(key);

        /// <inheritdoc/>
        public bool TryGetValue(string key, out object value) => _settings.TryGetValue(key, out value);

        /// <inheritdoc/>
        public object this[string key] => _settings[key];

        /// <inheritdoc/>
        public IEnumerable<string> Keys => _settings.Keys;

        /// <inheritdoc/>
        public IEnumerable<object> Values => _settings.Values;

        /// <inheritdoc/>
        public IMetadata<T> MetadataAs<T>() => _settings.MetadataAs<T>();

        /// <inheritdoc/>
        public object Get(string key, object defaultValue = null) => _settings.Get(key, defaultValue);

        /// <inheritdoc/>
        public object GetRaw(string key) => _settings.GetRaw(key);

        /// <inheritdoc/>
        public T Get<T>(string key) => _settings.Get<T>(key);

        /// <inheritdoc/>
        public T Get<T>(string key, T defaultValue) => _settings.Get(key, defaultValue);

        /// <inheritdoc/>
        public string String(string key, string defaultValue = null) => _settings.String(key, defaultValue);

        /// <inheritdoc/>
        public bool Bool(string key, bool defaultValue = false) => _settings.Bool(key, defaultValue);

        /// <inheritdoc/>
        public DateTime DateTime(string key, DateTime defaultValue = new DateTime()) => _settings.DateTime(key, defaultValue);

        /// <inheritdoc/>
        public FilePath FilePath(string key, FilePath defaultValue = null) => _settings.FilePath(key, defaultValue);

        /// <inheritdoc/>
        public DirectoryPath DirectoryPath(string key, DirectoryPath defaultValue = null) => _settings.DirectoryPath(key, defaultValue);

        /// <inheritdoc/>
        public IReadOnlyList<T> List<T>(string key, IReadOnlyList<T> defaultValue = null) => _settings.List(key, defaultValue);

        /// <inheritdoc/>
        public IDocument Document(string key) => _settings.Document(key);

        /// <inheritdoc/>
        public IReadOnlyList<IDocument> DocumentList(string key) => _settings.DocumentList(key);

        /// <inheritdoc/>
        public dynamic Dynamic(string key, object defaultValue = null) => _settings.Dynamic(key, defaultValue);
    }
}
