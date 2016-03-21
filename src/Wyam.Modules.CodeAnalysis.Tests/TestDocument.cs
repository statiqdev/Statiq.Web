using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Meta;

namespace Wyam.Modules.CodeAnalysis.Tests
{
    internal class TestDocument : IDocument
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
                value = GetValue(key, value);
                return true;
            }
            return false;
        }

        public object Get(string key, object defaultValue = null)
        {
            object value;
            return TryGetValue(key, out value) ? value : defaultValue;
        }

        public T Get<T>(string key)
        {
            return (T) Get(key);
        }

        public T Get<T>(string key, T defaultValue)
        {
            return (T) Get(key, (object)defaultValue);
        }

        public string String(string key, string defaultValue = null)
        {
            return Get<string>(key, defaultValue);
        }

        public FilePath FilePath(string key, FilePath defaultValue = null)
        {
             return Get<FilePath>(key, defaultValue);
        }

public DirectoryPath DirectoryPath(string key, DirectoryPath defaultValue = null)
        {
             return Get<DirectoryPath>(key, defaultValue);
        }

        public IReadOnlyList<T> List<T>(string key, IReadOnlyList<T> defaultValue = null)
        {
            return Get<IReadOnlyList<T>>(key, defaultValue);
        }

        public IDocument Document(string key)
        {
            return Get<IDocument>(key);
        }

        public IReadOnlyList<IDocument> Documents(string key)
        {
            return Get<IReadOnlyList<IDocument>>(key);
        }

        public string Link(string key, string defaultValue = null, bool pretty = true)
        {
            string value = Get<string>(key, defaultValue);
            value = string.IsNullOrWhiteSpace(value) ? "#" : PathHelper.ToRootLink(value);
            if (pretty && (value == "/index.html" || value == "/index.htm"))
            {
                return "/";
            }
            if (pretty && (value.EndsWith("/index.html") || value.EndsWith("/index.htm")))
            {
                return value.Substring(0, value.LastIndexOf("/", StringComparison.Ordinal));
            }
            return value;
        }

        public dynamic Dynamic(string key, object defaultValue = null)
        {
            return Get(key, defaultValue) ?? defaultValue;
        }

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

        public IEnumerable<string> Keys
        {
            get { return _metadata.Keys; }
        }

        public IEnumerable<object> Values
        {
            get { return _metadata.Select(x => GetValue(x.Key, x.Value)); }
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _metadata.Select(GetItem).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count
        {
            get { return _metadata.Count; }
        }

        // This resolves the metadata value by expanding IMetadataValue
        private object GetValue(string key, object value)
        {
            IMetadataValue metadataValue = value as IMetadataValue;
            return metadataValue != null ? metadataValue.Get(key, this) : value;
        }

        // This resolves the metadata value by expanding IMetadataValue
        // To reduce allocations, it returns the input KeyValuePair if value is not IMetadataValue
        private KeyValuePair<string, object> GetItem(KeyValuePair<string, object> item)
        {
            IMetadataValue metadataValue = item.Value as IMetadataValue;
            return metadataValue != null ? new KeyValuePair<string, object>(item.Key, metadataValue.Get(item.Key, this)) : item;
        }

        public string Id
        {
            get { throw new NotSupportedException(); }
        }

        public string Source
        {
            get { throw new NotSupportedException(); }
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
