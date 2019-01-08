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
        private readonly IDictionary<string, object> _metadata;

        public TestMetadata()
        {
            _metadata = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        public TestMetadata(IDictionary<string, object> initialMetadata)
        {
            _metadata = new Dictionary<string, object>(initialMetadata, StringComparer.OrdinalIgnoreCase);
        }

        /// <inhertdoc />
        public bool ContainsKey(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            return _metadata.ContainsKey(key);
        }

        /// <inhertdoc />
        public void Add(string key, object value) => _metadata.Add(key, value);

        /// <inhertdoc />
        public bool Remove(string key) => _metadata.Remove(key);

        /// <inhertdoc />
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

        object IDictionary<string, object>.this[string key]
        {
            get { return _metadata[key]; }
            set { _metadata[key] = value; }
        }

        /// <inhertdoc />
        public object Get(string key, object defaultValue = null) =>
            TryGetValue(key, out object value) ? value : defaultValue;

        /// <inhertdoc />
        public object GetRaw(string key) => _metadata[key];

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
                _metadata[key] = value;
            }
        }

        /// <inhertdoc />
        public IEnumerable<string> Keys => _metadata.Keys;

        ICollection<object> IDictionary<string, object>.Values => _metadata.Values;

        ICollection<string> IDictionary<string, object>.Keys => _metadata.Keys;

        /// <inhertdoc />
        public IEnumerable<object> Values => _metadata.Select(x => GetValue(x.Value));

        /// <inhertdoc />
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _metadata.Select(GetItem).GetEnumerator();

        /// <inhertdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inhertdoc />
        public void Add(KeyValuePair<string, object> item) => _metadata.Add(item);

        /// <inhertdoc />
        public void Clear() => _metadata.Clear();

        /// <inhertdoc />
        public bool Contains(KeyValuePair<string, object> item) => _metadata.Contains(item);

        /// <inhertdoc />
        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) => _metadata.CopyTo(array, arrayIndex);

        /// <inhertdoc />
        public bool Remove(KeyValuePair<string, object> item) => _metadata.Remove(item);

        /// <inhertdoc />
        public int Count => _metadata.Count;

        public bool IsReadOnly => _metadata.IsReadOnly;

        /// <inhertdoc />
        public IMetadata<T> MetadataAs<T>() => new TestMetadataAs<T>(
            _metadata
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
