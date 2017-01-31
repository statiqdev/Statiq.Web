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

        public TestDocument()
        {
            Id = Guid.NewGuid().ToString();
        }

        public TestDocument(string content)
            : this()
        {
            Content = content;
        }

        public TestDocument(IEnumerable<MetadataItem> metadata)
            : this()
        {
            foreach (KeyValuePair<string, object> item in metadata)
            {
                _metadata[item.Key] = item.Value;
            }
        }

        public TestDocument(string content, IEnumerable<MetadataItem> metadata)
            : this(metadata)
        {
            Content = content;
        }

        public TestDocument(IEnumerable<KeyValuePair<string, object>> metadata)
            : this()
        {
            foreach (KeyValuePair<string, object> item in metadata)
            {
                _metadata[item.Key] = item.Value;
            }
        }

        public TestDocument(string content, IEnumerable<KeyValuePair<string, object>> metadata)
            : this(metadata)
        {
            Content = content;
        }

        public IMetadata WithoutSettings => this;

        public bool ContainsKey(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            return _metadata.ContainsKey(key);
        }

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

        public object Get(string key, object defaultValue = null)
        {
            object value;
            return TryGetValue(key, out value) ? value : defaultValue;
        }

        public object GetRaw(string key) => _metadata[key];

        public T Get<T>(string key) => (T)Get(key);

        public T Get<T>(string key, T defaultValue) => (T)Get(key, (object)defaultValue);

        public string String(string key, string defaultValue = null) => Get<string>(key, defaultValue);

        public bool Bool(string key, bool defaultValue = false) => Get<bool>(key, defaultValue);

        public DateTime DateTime(string key, DateTime defaultValue = default(DateTime)) => Get<DateTime>(key, defaultValue);

        public FilePath FilePath(string key, FilePath defaultValue = null)
        {
            object value = Get(key, (object)defaultValue);
            string stringValue = value as string;
            return stringValue != null ? new FilePath(stringValue) : (FilePath)value;
        }

        public DirectoryPath DirectoryPath(string key, DirectoryPath defaultValue = null)
        {
            object value = Get(key, (object)defaultValue);
            string stringValue = value as string;
            return stringValue != null ? new DirectoryPath(stringValue) : (DirectoryPath)value;
        }

        public IReadOnlyList<T> List<T>(string key, IReadOnlyList<T> defaultValue = null) => Get<IReadOnlyList<T>>(key, defaultValue);

        public IDocument Document(string key) => Get<IDocument>(key);

        public IReadOnlyList<IDocument> DocumentList(string key) => Get<IReadOnlyList<IDocument>>(key);

        public dynamic Dynamic(string key, object defaultValue = null) => Get(key, defaultValue) ?? defaultValue;

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

        public IEnumerable<string> Keys => _metadata.Keys;

        public IEnumerable<object> Values => _metadata.Select(x => GetValue(x.Value));

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _metadata.Select(GetItem).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

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

        public string Id { get; set; }

        public FilePath Source { get; set; }

        public string SourceString() => Source?.FullPath;

        public string Content { get; set; }

        public Stream GetStream() => new MemoryStream(Encoding.UTF8.GetBytes(Content));

        public IMetadata Metadata
        {
            get { throw new NotImplementedException(); }
        }

        public IMetadata<T> MetadataAs<T>()
        {
            throw new NotImplementedException();
        }


        public void Dispose()
        {
        }
    }
}
