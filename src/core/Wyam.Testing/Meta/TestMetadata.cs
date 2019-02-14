using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Wyam.Common.Meta;

namespace Wyam.Testing.Meta
{
    /// <summary>
    /// A test implementation of <see cref="IMetadata"/>.
    /// </summary>
    public class TestMetadata : IMetadata, IDictionary<string, object>, ITypeConversions
    {
        private readonly Dictionary<string, object> _dictionary;

        public TestMetadata()
        {
            _dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        public TestMetadata(IDictionary<string, object> initialMetadata)
        {
            _dictionary = new Dictionary<string, object>(initialMetadata, StringComparer.OrdinalIgnoreCase);
        }

        /// <inhertdoc />
        public bool ContainsKey(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            return _dictionary.ContainsKey(key);
        }

        /// <inhertdoc />
        public void Add(string key, object value) => _dictionary.Add(key, value);

        /// <inhertdoc />
        public bool Remove(string key) => _dictionary.Remove(key);

        object IDictionary<string, object>.this[string key]
        {
            get { return _dictionary[key]; }
            set { _dictionary[key] = value; }
        }

        /// <inhertdoc />
        public object Get(string key, object defaultValue = null) =>
            TryGetValue(key, out object value) ? value : defaultValue;

        /// <inhertdoc />
        public object GetRaw(string key) => _dictionary[key];

        /// <inhertdoc />
        public T Get<T>(string key)
        {
            object value = Get(key);

            // Check if there's a test-specific conversion
            if (TypeConversions.TryGetValue((value?.GetType() ?? typeof(object), typeof(T)), out Func<object, object> typeConversion))
            {
                return (T)typeConversion(value);
            }

            // Default conversion is just to cast
            return (T)value;
        }

        /// <inhertdoc />
        public T Get<T>(string key, T defaultValue)
        {
            if (TryGetValue(key, out object value))
            {
                // Check if there's a test-specific conversion
                if (TypeConversions.TryGetValue((value?.GetType() ?? typeof(object), typeof(T)), out Func<object, object> typeConversion))
                {
                    return (T)typeConversion(value);
                }

                // Default conversion is just to cast
                return (T)value;
            }

            // Key not found, return the default value
            return defaultValue;
        }

        /// <inheritdoc />
        public bool TryGetValue<T>(string key, out T value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            value = default(T);
            if (!_dictionary.TryGetValue(key, out object rawValue))
            {
                return false;
            }
            rawValue = GetValue(rawValue);

            // Check if there's a test-specific conversion
            if (TypeConversions.TryGetValue((rawValue?.GetType() ?? typeof(object), typeof(T)), out Func<object, object> typeConversion))
            {
                value = (T)typeConversion(rawValue);
            }
            else
            {
                // Default conversion is just to cast
                value = (T)rawValue;
            }

            return true;
        }

        /// <inhertdoc />
        public bool TryGetValue(string key, out object value) => TryGetValue<object>(key, out value);

        public Dictionary<(Type Value, Type Result), Func<object, object>> TypeConversions { get; } = new Dictionary<(Type Value, Type Result), Func<object, object>>();

        public void AddTypeConversion<T, TResult>(Func<T, TResult> typeConversion) => TypeConversions.Add((typeof(T), typeof(TResult)), x => typeConversion((T)x));

        /// <inhertdoc />
        public IMetadata GetMetadata(params string[] keys) => new TestMetadata(keys.Where(ContainsKey).ToDictionary(x => x, x => this[x]));

        /// <inhertdoc />
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
            set
            {
                _dictionary[key] = value;
            }
        }

        /// <inhertdoc />
        public IEnumerable<string> Keys => _dictionary.Keys;

        ICollection<object> IDictionary<string, object>.Values => _dictionary.Values;

        ICollection<string> IDictionary<string, object>.Keys => _dictionary.Keys;

        /// <inhertdoc />
        public IEnumerable<object> Values => _dictionary.Select(x => GetValue(x.Value));

        /// <inhertdoc />
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _dictionary.Select(GetItem).GetEnumerator();

        /// <inhertdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inhertdoc />
        public void Add(KeyValuePair<string, object> item) => ((IDictionary<string, object>)_dictionary).Add(item);

        /// <inhertdoc />
        public void Clear() => _dictionary.Clear();

        /// <inhertdoc />
        public bool Contains(KeyValuePair<string, object> item) => _dictionary.Contains(item);

        /// <inhertdoc />
        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) => ((IDictionary<string, object>)_dictionary).CopyTo(array, arrayIndex);

        /// <inhertdoc />
        public bool Remove(KeyValuePair<string, object> item) => ((IDictionary<string, object>)_dictionary).Remove(item);

        /// <inhertdoc />
        public int Count => _dictionary.Count;

        public bool IsReadOnly => ((IDictionary<string, object>)_dictionary).IsReadOnly;

        /// <inhertdoc />
        public IMetadata<T> MetadataAs<T>() => new TestMetadataAs<T>(
            _dictionary
                .Where(x => x.Value is T || TypeConversions.ContainsKey((x.Value?.GetType() ?? typeof(object), typeof(T))))
                .ToDictionary(x => x.Key, x => Get<T>(x.Key)));

        /// <summary>
        /// This resolves the metadata value by recursively expanding IMetadataValue.
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
    }
}
