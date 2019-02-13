using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Wyam.Common.Execution;

namespace Wyam.Common.Meta
{
    /// <summary>
    /// A dictionary with metadata type conversion superpowers.
    /// </summary>
    public class SimpleMetadataDictionary : IMetadataDictionary
    {
        private readonly Dictionary<string, object> _dictionary;
        private readonly IExecutionContext _context;

        public SimpleMetadataDictionary(IExecutionContext context)
        {
            _dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public SimpleMetadataDictionary(IExecutionContext context, IDictionary<string, object> dictionary)
        {
            _dictionary = new Dictionary<string, object>(dictionary, StringComparer.OrdinalIgnoreCase);
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public object this[string key]
        {
            get => _dictionary[key];
            set => _dictionary[key] = value;
        }

        object IReadOnlyDictionary<string, object>.this[string key] => this[key];

        public int Count => _dictionary.Count;

        public ICollection<string> Keys => _dictionary.Keys;

        public ICollection<object> Values => _dictionary.Values;

        public bool IsReadOnly => false;

        IEnumerable<string> IReadOnlyDictionary<string, object>.Keys => Keys;

        IEnumerable<object> IReadOnlyDictionary<string, object>.Values => Values;

        public void Add(string key, object value) => _dictionary.Add(key, value);

        public void Add(KeyValuePair<string, object> item) =>
            ((IDictionary<string, object>)_dictionary).Add(item);

        public void Clear() => _dictionary.Clear();

        public bool Contains(KeyValuePair<string, object> item) =>
            ((IDictionary<string, object>)_dictionary).Contains(item);

        public bool ContainsKey(string key) => _dictionary.ContainsKey(key);

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) =>
            ((IDictionary<string, object>)_dictionary).CopyTo(array, arrayIndex);

        public object Get(string key, object defaultValue = null) =>
            TryGetValue(key, out object value) ? value : defaultValue;

        public T Get<T>(string key) =>
            TryGetValue(key, out object value) && _context.TryConvert(value, out T result) ? result : default(T);

        public T Get<T>(string key, T defaultValue) =>
            TryGetValue(key, out object value) && _context.TryConvert(value, out T result) ? result : defaultValue;

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _dictionary.GetEnumerator();

        public object GetRaw(string key) =>
            _dictionary[key ?? throw new ArgumentNullException(nameof(key))];

        public bool Remove(string key) => _dictionary.Remove(key);

        public bool Remove(KeyValuePair<string, object> item) =>
            ((IDictionary<string, object>)_dictionary).Remove(item);

        public bool TryGetValue(string key, out object value) => _dictionary.TryGetValue(key, out value);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IMetadata GetMetadata(params string[] keys) =>
            throw new NotSupportedException();

        public IMetadata<T> MetadataAs<T>() =>
            throw new NotSupportedException();
    }
}
