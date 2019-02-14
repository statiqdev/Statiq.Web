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
    /// <remarks>
    /// This class wraps an underlying <see cref="Dictionary{TKey, TValue}"/> but
    /// uses the provided <see cref="IExecutionContext"/> to perform type conversions
    /// when requesting values.
    /// </remarks>
    public class ConvertingDictionary : IMetadataDictionary
    {
        private readonly Dictionary<string, object> _dictionary;

        private readonly IExecutionContext _context;

        public ConvertingDictionary(IExecutionContext context)
        {
            _dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public ConvertingDictionary(IExecutionContext context, IDictionary<string, object> dictionary)
        {
            _dictionary = new Dictionary<string, object>(dictionary, StringComparer.OrdinalIgnoreCase);
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <inheritdoc />
        public object this[string key]
        {
            get => _dictionary[key];
            set => _dictionary[key] = value;
        }

        /// <inheritdoc />
        object IReadOnlyDictionary<string, object>.this[string key] => this[key];

        /// <inheritdoc />
        public int Count => _dictionary.Count;

        /// <inheritdoc />
        public ICollection<string> Keys => _dictionary.Keys;

        /// <inheritdoc />
        public ICollection<object> Values => _dictionary.Values;

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <inheritdoc />
        IEnumerable<string> IReadOnlyDictionary<string, object>.Keys => Keys;

        /// <inheritdoc />
        IEnumerable<object> IReadOnlyDictionary<string, object>.Values => Values;

        /// <inheritdoc />
        public void Add(string key, object value) => _dictionary.Add(key, value);

        /// <inheritdoc />
        public void Add(KeyValuePair<string, object> item) =>
            ((IDictionary<string, object>)_dictionary).Add(item);

        /// <inheritdoc />
        public void Clear() => _dictionary.Clear();

        /// <inheritdoc />
        public bool Contains(KeyValuePair<string, object> item) =>
            ((IDictionary<string, object>)_dictionary).Contains(item);

        /// <inheritdoc />
        public bool ContainsKey(string key) => _dictionary.ContainsKey(key);

        /// <inheritdoc />
        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) =>
            ((IDictionary<string, object>)_dictionary).CopyTo(array, arrayIndex);

        /// <inheritdoc />
        public object Get(string key, object defaultValue = null) =>
            TryGetValue(key, out object value) ? value : defaultValue;

        /// <inheritdoc />
        public T Get<T>(string key) => TryGetValue(key, out T value) ? value : default(T);

        /// <inheritdoc />
        public T Get<T>(string key, T defaultValue) => TryGetValue(key, out T value) ? value : defaultValue;

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _dictionary.GetEnumerator();

        /// <inheritdoc />
        public object GetRaw(string key) =>
            _dictionary[key ?? throw new ArgumentNullException(nameof(key))];

        /// <inheritdoc />
        public bool Remove(string key) => _dictionary.Remove(key);

        /// <inheritdoc />
        public bool Remove(KeyValuePair<string, object> item) =>
            ((IDictionary<string, object>)_dictionary).Remove(item);

        /// <inheritdoc />
        public bool TryGetValue(string key, out object value) => TryGetValue<object>(key, out value);

        /// <inheritdoc />
        public bool TryGetValue<T>(string key, out T value)
        {
            value = default(T);
            return _dictionary.TryGetValue(key, out object rawValue) && _context.TryConvert(rawValue, out value);
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        public IMetadata GetMetadata(params string[] keys) =>
            throw new NotSupportedException();

        /// <inheritdoc />
        public IMetadata<T> MetadataAs<T>() =>
            throw new NotSupportedException();
    }
}
