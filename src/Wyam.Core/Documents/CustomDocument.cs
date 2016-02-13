using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.Meta;

namespace Wyam.Core.Documents
{
    public class CustomDocument : IDocument
    {
        private IDocument _document;

        internal void SetDocument(IDocument document) => 
            _document = document;

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => 
            _document.GetEnumerator();

        public int Count => _document.Count;

        public bool ContainsKey(string key) => _document.ContainsKey(key);

        public bool TryGetValue(string key, out object value) => 
            _document.TryGetValue(key, out value);

        public object this[string key] => _document[key];

        public IEnumerable<string> Keys => _document.Keys;

        public IEnumerable<object> Values => _document.Values;

        public IMetadata<T> MetadataAs<T>() => _document.MetadataAs<T>();

        public object Get(string key, object defaultValue = null) => 
            _document.Get(key, defaultValue);

        public T Get<T>(string key) => _document.Get<T>(key);

        public T Get<T>(string key, T defaultValue) => 
            _document.Get(key, defaultValue);

        public string String(string key, string defaultValue = null) => 
            _document.String(key, defaultValue);

        public IReadOnlyList<T> List<T>(string key, IReadOnlyList<T> defaultValue = null) => 
            _document.List(key, defaultValue);

        public IDocument Document(string key) => 
            _document.Document(key);

        public IReadOnlyList<IDocument> Documents(string key) => _document.Documents(key);

        public string Link(string key, string defaultValue = null, bool pretty = true) => 
            _document.Link(key, defaultValue, pretty);

        public dynamic Dynamic(string key, object defaultValue = null) => 
            _document.Dynamic(key, defaultValue);

        public void Dispose() => _document.Dispose();

        public string Source => _document.Source;

        public string Id => _document.Id;

        public IMetadata Metadata => _document.Metadata;

        public string Content => _document.Content;

        public Stream GetStream() => _document.GetStream();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
