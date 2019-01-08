using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Testing.Meta;

namespace Wyam.Testing.Documents
{
    /// <summary>
    /// A simple document that stores metadata in a <c>Dictionary</c> without any built-in type conversion.
    /// Also no support for content at this time.
    /// </summary>
    public class TestDocument : IDocument, ITypeConversions
    {
        private readonly TestMetadata _metadata = new TestMetadata();

        /// <inhertdoc />
        public TestDocument()
        {
            Id = Guid.NewGuid().ToString();
        }

        /// <inhertdoc />
        public TestDocument(string content)
            : this()
        {
            Content = content;
        }

        /// <inhertdoc />
        public TestDocument(Stream stream)
            : this()
        {
            Stream = stream;
        }

        /// <inhertdoc />
        public TestDocument(IEnumerable<KeyValuePair<string, object>> metadata)
            : this()
        {
            foreach (KeyValuePair<string, object> item in metadata)
            {
                _metadata[item.Key] = item.Value;
            }
        }

        /// <inhertdoc />
        public TestDocument(string content, IEnumerable<KeyValuePair<string, object>> metadata)
            : this(metadata)
        {
            Content = content;
        }

        /// <inhertdoc />
        public TestDocument(Stream stream, IEnumerable<KeyValuePair<string, object>> metadata)
            : this(metadata)
        {
            Stream = stream;
        }

        /// <inhertdoc />
        public IMetadata WithoutSettings => this;

        /// <inhertdoc />
        public bool ContainsKey(string key) => _metadata.ContainsKey(key);

        /// <inhertdoc />
        public bool TryGetValue(string key, out object value) => _metadata.TryGetValue(key, out value);

        /// <inhertdoc />
        public IMetadata<T> MetadataAs<T>() => _metadata.MetadataAs<T>();

        /// <inhertdoc />
        public object Get(string key, object defaultValue = null) => _metadata.Get(key, defaultValue);

        /// <inhertdoc />
        public object GetRaw(string key) => _metadata[key];

        /// <inhertdoc />
        public T Get<T>(string key) => _metadata.Get<T>(key);

        /// <inhertdoc />
        public T Get<T>(string key, T defaultValue) => _metadata.Get<T>(key, defaultValue);

        public IMetadata GetMetadata(params string[] keys) => _metadata.GetMetadata(keys);

        /// <inhertdoc />
        public object this[string key] => _metadata[key];

        /// <inhertdoc />
        public IEnumerable<string> Keys => _metadata.Keys;

        /// <inhertdoc />
        public IEnumerable<object> Values => _metadata.Values;

        /// <inhertdoc />
        public string Id { get; set; }

        /// <inhertdoc />
        public FilePath Source { get; set; }

        /// <inhertdoc />
        public string SourceString() => Source?.FullPath;

        /// <inhertdoc />
        public string Content { get; set; }

        /// <summary>
        /// Lets you set the document stream directly
        /// </summary>
        public Stream Stream { get; set; }

        /// <inhertdoc />
        public Stream GetStream()
        {
            return Stream ?? (string.IsNullOrEmpty(Content)
                ? new MemoryStream()
                : new MemoryStream(Encoding.UTF8.GetBytes(Content)));
        }

        /// <inhertdoc />
        public IMetadata Metadata => this;

        /// <inhertdoc />
        public void Dispose()
        {
        }

        /// <inhertdoc />
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _metadata.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_metadata).GetEnumerator();
        }

        /// <inhertdoc />
        public int Count => _metadata.Count;

        public Dictionary<(Type Value, Type Result), Func<object, object>> TypeConversions => _metadata.TypeConversions;

        public void AddTypeConversion<T, TResult>(Func<T, TResult> typeConversion) => _metadata.AddTypeConversion(typeConversion);
    }
}
