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

namespace Wyam.Core.Documents
{
    /// <summary>
    /// Derive custom document types from this class to get built-in support.
    /// </summary>
    public abstract class CustomDocument : IDocument
    {
        internal IDocument Document { get; set; }

        /// <summary>
        /// Clones this instance of the document. You must return a new instance of your
        /// custom document type, even if nothing will change, otherwise the document factory
        /// will throw an exception. The default implementation of this method performs a
        /// <code>object.MemberwiseClone()</code>.
        /// </summary>
        /// <returns>A new custom document instance with the same values as the current instance.</returns>
        protected internal virtual CustomDocument Clone() => (CustomDocument)MemberwiseClone();

        /// <inheritdoc />
        public IMetadata WithoutSettings => Document.WithoutSettings;

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => Document.GetEnumerator();

        /// <inheritdoc />
        public int Count => Document.Count;

        /// <inheritdoc />
        public bool ContainsKey(string key) => Document.ContainsKey(key);

        /// <inheritdoc />
        public bool TryGetValue(string key, out object value) => Document.TryGetValue(key, out value);

        /// <inheritdoc />
        public object this[string key] => Document[key];

        /// <inheritdoc />
        public IEnumerable<string> Keys => Document.Keys;

        /// <inheritdoc />
        public IEnumerable<object> Values => Document.Values;

        /// <inheritdoc />
        public IMetadata<T> MetadataAs<T>() => Document.MetadataAs<T>();

        /// <inheritdoc />
        public object Get(string key, object defaultValue = null) => Document.Get(key, defaultValue);

        /// <inheritdoc />
        public object GetRaw(string key) => Document.GetRaw(key);

        /// <inheritdoc />
        public T Get<T>(string key) => Document.Get<T>(key);

        /// <inheritdoc />
        public T Get<T>(string key, T defaultValue) => Document.Get(key, defaultValue);

        /// <inheritdoc />
        public IMetadata GetMetadata(params string[] keys) => Document.GetMetadata(keys);

        /// <inheritdoc />
        public void Dispose() => Document.Dispose();

        /// <inheritdoc />
        public FilePath Source => Document.Source;

        /// <inheritdoc />
        public string SourceString() => Document.SourceString();

        /// <inheritdoc />
        public string Id => Document.Id;

        /// <inheritdoc />
        public IMetadata Metadata => Document.Metadata;

        /// <inheritdoc />
        public string Content => Document.Content;

        /// <inheritdoc />
        public Stream GetStream() => Document.GetStream();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
