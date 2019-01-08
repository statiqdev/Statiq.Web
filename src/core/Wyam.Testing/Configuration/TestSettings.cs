using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Testing.Meta;

namespace Wyam.Testing.Configuration
{
    public class TestSettings : ISettings
    {
        private readonly TestMetadata _metadata = new TestMetadata();

        /// <inheritdoc />
        public void Add(KeyValuePair<string, object> item) => _metadata.Add(item);

        /// <inheritdoc />
        public void Clear() => _metadata.Clear();

        /// <inheritdoc />
        public bool Contains(KeyValuePair<string, object> item) => _metadata.Contains(item);

        /// <inheritdoc />
        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) => _metadata.CopyTo(array, arrayIndex);

        /// <inheritdoc />
        public bool Remove(KeyValuePair<string, object> item) => _metadata.Remove(item);

        /// <inheritdoc />
        int ICollection<KeyValuePair<string, object>>.Count => _metadata.Count;

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <inheritdoc />
        public bool ContainsKey(string key) => _metadata.ContainsKey(key);

        /// <inheritdoc />
        bool IReadOnlyDictionary<string, object>.TryGetValue(string key, out object value) => _metadata.TryGetValue(key, out value);

        /// <inheritdoc />
        public object this[string key] => _metadata[key];

        /// <inheritdoc />
        public IEnumerable<string> Keys => _metadata.Keys;

        /// <inheritdoc />
        ICollection<object> IDictionary<string, object>.Values => _metadata.Values.ToList();

        /// <inheritdoc />
        ICollection<string> IDictionary<string, object>.Keys => _metadata.Keys.ToList();

        /// <inheritdoc />
        public IEnumerable<object> Values => _metadata.Values;

        /// <inheritdoc />
        public void Add(string key, object value) => _metadata.Add(key, value);

        /// <inheritdoc />
        public bool Remove(string key) => _metadata.Remove(key);

        /// <inheritdoc />
        bool IReadOnlyDictionary<string, object>.ContainsKey(string key) => _metadata.ContainsKey(key);

        /// <inheritdoc />
        public bool TryGetValue(string key, out object value) => _metadata.TryGetValue(key, out value);

        /// <inheritdoc />
        object IDictionary<string, object>.this[string key]
        {
            get { return _metadata[key]; }
            set { _metadata[key] = value; }
        }

        /// <inheritdoc />
        public IMetadata<T> MetadataAs<T>() => _metadata.MetadataAs<T>();

        /// <inheritdoc />
        public object Get(string key, object defaultValue = null) => _metadata.Get(key, defaultValue);

        /// <inheritdoc />
        public object GetRaw(string key) => _metadata.GetRaw(key);

        /// <inheritdoc />
        public T Get<T>(string key) => _metadata.Get<T>(key);

        /// <inheritdoc />
        public T Get<T>(string key, T defaultValue) => _metadata.Get(key, defaultValue);

        /// <inheritdoc />
        public IMetadata GetMetadata(params string[] keys) => _metadata.GetMetadata(keys);

        /// <inheritdoc />
        ICollection<string> IMetadataDictionary.Keys
        {
            get { throw new NotImplementedException(); }
        }

        /// <inheritdoc />
        ICollection<object> IMetadataDictionary.Values
        {
            get { throw new NotImplementedException(); }
        }

        /// <inheritdoc />
        object IMetadataDictionary.this[string key]
        {
            get { return _metadata[key]; }
            set { _metadata[key] = value; }
        }

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _metadata.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_metadata).GetEnumerator();

        /// <inheritdoc />
        public int Count => _metadata.Count;
    }
}
