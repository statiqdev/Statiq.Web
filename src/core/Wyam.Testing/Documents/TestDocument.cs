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

namespace Wyam.Testing.Documents
{
    /// <summary>
    /// A simple document that stores metadata in a <c>Dictionary</c> without any built-in type conversion.
    /// Also no support for content at this time.
    /// </summary>
    public class TestDocument : IDocument
    {
        private readonly IDictionary<string, object> _metadata = new Dictionary<string, object>();

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
        public IMetadata WithoutSettings => this;

        /// <inhertdoc />
        public bool ContainsKey(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            return _metadata.ContainsKey(key);
        }

        /// <inhertdoc />
        public bool TryGetValue(string key, out object value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            if (_metadata.TryGetValue(key, out value))
            {
                value = GetValue(value);
                return true;
            }
            return false;
        }

        /// <inhertdoc />
        public object Get(string key, object defaultValue = null)
        {
            object value;
            return TryGetValue(key, out value) ? value : defaultValue;
        }

        /// <inhertdoc />
        public object GetRaw(string key) => _metadata[key];

        /// <inhertdoc />
        public T Get<T>(string key) => (T)Get(key);

        /// <inhertdoc />
        public T Get<T>(string key, T defaultValue) => (T)Get(key, (object)defaultValue);

        /// <inhertdoc />
        public string String(string key, string defaultValue = null) => Get<string>(key, defaultValue);

        /// <inhertdoc />
        public bool Bool(string key, bool defaultValue = false) => Get<bool>(key, defaultValue);

        /// <inhertdoc />
        public DateTime DateTime(string key, DateTime defaultValue = default(DateTime)) => Get<DateTime>(key, defaultValue);

        /// <inhertdoc />
        public FilePath FilePath(string key, FilePath defaultValue = null)
        {
            object value = Get(key, (object)defaultValue);
            string stringValue = value as string;
            return stringValue != null ? new FilePath(stringValue) : (FilePath)value;
        }

        /// <inhertdoc />
        public DirectoryPath DirectoryPath(string key, DirectoryPath defaultValue = null)
        {
            object value = Get(key, (object)defaultValue);
            string stringValue = value as string;
            return stringValue != null ? new DirectoryPath(stringValue) : (DirectoryPath)value;
        }

        /// <inhertdoc />
        public IReadOnlyList<T> List<T>(string key, IReadOnlyList<T> defaultValue = null) => Get<IReadOnlyList<T>>(key, defaultValue);

        /// <inhertdoc />
        public IDocument Document(string key) => Get<IDocument>(key);

        /// <inhertdoc />
        public IReadOnlyList<IDocument> DocumentList(string key) => Get<IReadOnlyList<IDocument>>(key);

        /// <inhertdoc />
        public dynamic Dynamic(string key, object defaultValue = null) => Get(key, defaultValue) ?? defaultValue;

        /// <inhertdoc />
        public object this[string key]
        {
            get
            {
                if (key == null)
                {
                    throw new ArgumentNullException(nameof(key));
                }
                object value;
                if (!TryGetValue(key, out value))
                {
                    throw new KeyNotFoundException();
                }
                return value;
            }
        }

        /// <inhertdoc />
        public IEnumerable<string> Keys => _metadata.Keys;

        /// <inhertdoc />
        public IEnumerable<object> Values => _metadata.Select(x => GetValue(x.Value));

        /// <inhertdoc />
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _metadata.Select(GetItem).GetEnumerator();

        /// <inhertdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inhertdoc />
        public int Count => _metadata.Count;

        /// <summary>
        /// This resolves the metadata value by recursivly expanding IMetadataValue.
        /// </summary>
        private object GetValue(object originalValue)
        {
            IMetadataValue metadataValue = originalValue as IMetadataValue;
            return metadataValue != null ? GetValue(metadataValue.Get(this)) : originalValue;
        }

        /// <summary>
        /// This resolves the metadata value by expanding IMetadataValue.
        /// </summary>
        private KeyValuePair<string, object> GetItem(KeyValuePair<string, object> item)
        {
            IMetadataValue metadataValue = item.Value as IMetadataValue;
            return metadataValue != null ? new KeyValuePair<string, object>(item.Key, GetValue(metadataValue.Get(this))) : item;
        }

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
        public IMetadata Metadata
        {
            get { return this; }
        }

        /// <inhertdoc />
        public IMetadata<T> MetadataAs<T>()
        {
            throw new NotImplementedException();
        }

        /// <inhertdoc />
        public void Dispose()
        {
        }
    }
}
