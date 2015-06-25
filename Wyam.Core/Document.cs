using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Abstractions;

namespace Wyam.Core
{
    internal class Document : IDocument
    {
        private readonly Metadata _metadata;
        private readonly string _content = string.Empty;

        internal Document(Metadata metadata)
        {
            _metadata = metadata;
        }

        private Document(Metadata metadata, string content, IEnumerable<KeyValuePair<string, object>> items = null)
        {
            _metadata = metadata.Clone(items);
            _content = content ?? string.Empty;
        }

        public IMetadata Metadata
        {
            get { return _metadata; }
        }

        public string Content
        {
            get { return _content; }
        }
        
        public override string ToString()
        {
            return Content;
        }

        public IDocument Clone(string content, IEnumerable<KeyValuePair<string, object>> items = null)
        {
            return new Document(_metadata, content, items);
        }

        public IDocument Clone(IEnumerable<KeyValuePair<string, object>> items = null)
        {
            return Clone(_content, items);
        }

        // IMetadata

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _metadata.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool ContainsKey(string key)
        {
            return _metadata.ContainsKey(key);
        }

        public bool TryGetValue(string key, out object value)
        {
            return _metadata.TryGetValue(key, out value);
        }

        public object this[string key]
        {
            get { return _metadata[key]; }
        }

        public IEnumerable<string> Keys
        {
            get { return _metadata.Keys; }
        }

        public IEnumerable<object> Values
        {
            get { return _metadata.Values; }
        }

        public IMetadata<T> MetadataAs<T>()
        {
            return _metadata.MetadataAs<T>();
        }

        public object Get(string key, object defaultValue)
        {
            return _metadata.Get(key, defaultValue);
        }

        public T Get<T>(string key)
        {
            return _metadata.Get<T>(key);
        }

        public T Get<T>(string key, T defaultValue)
        {
            return _metadata.Get<T>(key, defaultValue);
        }

        public string String(string key, string defaultValue = null)
        {
            return _metadata.String(key, defaultValue);
        }

        public int Count
        {
            get { return _metadata.Count; }
        }
    }
}
