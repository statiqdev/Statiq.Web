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

        public TestDocument(IEnumerable<MetadataItem> metadata)
        {
            foreach (KeyValuePair<string, object> item in metadata)
            {
                _metadata[item.Key] = item.Value;
            }
        }

        public IMetadata<T> MetadataAs<T>()
        {
            throw new NotSupportedException();
        }

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

        public T Get<T>(string key) => (T)Get(key);

        public T Get<T>(string key, T defaultValue) => (T)Get(key, (object)defaultValue);

        public string String(string key, string defaultValue = null) => Get<string>(key, defaultValue);

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
        /// This resolves the metadata value by expanding IMetadataValue.
        /// </summary>
        private object GetValue(object originalValue)
        {
            IMetadataValue metadataValue = originalValue as IMetadataValue;
            return metadataValue != null ? metadataValue.Get(this) : originalValue;
        }

        /// <summary>
        /// This resolves the metadata value by expanding IMetadataValue.
        /// </summary>
        private KeyValuePair<string, object> GetItem(KeyValuePair<string, object> item)
        {
            IMetadataValue metadataValue = item.Value as IMetadataValue;
            return metadataValue != null ? new KeyValuePair<string, object>(item.Key, metadataValue.Get(this)) : item;
        }

        public string Id
        {
            get { throw new NotSupportedException(); }
        }

        public FilePath Source
        {
            get { throw new NotSupportedException(); }
        }

        public string SourceString()
        {
            throw new NotSupportedException();
        }

        public IMetadata Metadata
        {
            get { throw new NotSupportedException(); }
        }

        public string Content
        {
            get { throw new NotSupportedException(); }
        }

        public Stream GetStream()
        {
            throw new NotSupportedException();
        }

        public void Dispose()
        {
        }
    }
}
