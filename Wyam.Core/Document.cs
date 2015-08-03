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

        public string Source { get; } = "Initial Document";
        public IMetadata Metadata => _metadata;
        public string Content { get; } = string.Empty;

        internal Document(Metadata metadata)
        {
            _metadata = metadata;
        }

        private Document(string source, Metadata metadata, string content, IEnumerable<KeyValuePair<string, object>> items = null)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (string.IsNullOrWhiteSpace(source))
            {
                throw new ArgumentException(nameof(source));
            }

            Source = source;
            _metadata = metadata.Clone(items);
            Content = content ?? string.Empty;
        }

        public override string ToString()
        {
            return Content;
        }

        public IDocument Clone(string source, string content, IEnumerable<KeyValuePair<string, object>> items = null)
        {
            return new Document(source, _metadata, content, items);
        }

        public IDocument Clone(string content, IEnumerable<KeyValuePair<string, object>> items = null)
        {
            return new Document(Source, _metadata, content, items);
        }

        public IDocument Clone(IEnumerable<KeyValuePair<string, object>> items = null)
        {
            return Clone(Content, items);
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

        public object this[string key] => _metadata[key];

        public IEnumerable<string> Keys => _metadata.Keys;

        public IEnumerable<object> Values => _metadata.Values;

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

        public string Link(string key, string defaultValue = null)
        {
            return _metadata.Link(key, defaultValue);
        }

        public int Count => _metadata.Count;
    }
}
