using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Meta;

namespace Wyam.Core.Meta
{
    internal abstract class Metadata : IMetadata
    {
        protected internal Stack<IReadOnlyDictionary<string, object>> Stack { get; }

        protected Metadata()
        {
            Stack = new Stack<IReadOnlyDictionary<string, object>>();
        }

        protected Metadata(Stack<IReadOnlyDictionary<string, object>> stack)
        {
            Stack = stack ?? throw new ArgumentNullException(nameof(stack));
        }

        public IMetadata<T> MetadataAs<T>() => new MetadataAs<T>(this);

        public bool ContainsKey(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            return Stack.Any(x => x.ContainsKey(key));
        }

        public object Get(string key, object defaultValue = null) =>
            TryGetValue(key, out object value) ? value : defaultValue;

        public object GetRaw(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            IReadOnlyDictionary<string, object> meta = Stack.FirstOrDefault(x => x.ContainsKey(key));
            if (meta == null)
            {
                throw new KeyNotFoundException(nameof(key));
            }
            return meta[key];
        }

        public T Get<T>(string key) => MetadataAs<T>().Get(key);

        public T Get<T>(string key, T defaultValue) => MetadataAs<T>().Get(key, defaultValue);

        public bool TryGetValue<T>(string key, out T value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            value = default(T);
            IReadOnlyDictionary<string, object> meta = Stack.FirstOrDefault(x => x.ContainsKey(key));
            if (meta == null)
            {
                return false;
            }
            object rawValue = GetValue(meta[key]);
            return TypeHelper.TryConvert(rawValue, out value);
        }

        public bool TryGetValue(string key, out object value) => TryGetValue<object>(key, out value);

        public object this[string key]
        {
            get
            {
                if (key == null)
                {
                    throw new ArgumentNullException(nameof(key));
                }
                if (!TryGetValue(key, out object value))
                {
                    throw new KeyNotFoundException("The key " + key + " was not found in metadata, use Get() to provide a default value.");
                }
                return value;
            }
        }

        public IEnumerable<string> Keys => Stack.SelectMany(x => x.Keys);

        public IEnumerable<object> Values => Stack.SelectMany(x => x.Select(y => GetValue(y.Value)));

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() =>
            Stack.SelectMany(x => x.Select(GetItem)).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => Stack.Sum(x => x.Count);

        public IMetadata GetMetadata(params string[] keys) => new MetadataDictionary(keys.Where(ContainsKey).ToDictionary(x => x, x => this[x]));

        /// <summary>
        /// This resolves the metadata value by recursively expanding IMetadataValue.
        /// </summary>
        private object GetValue(object originalValue) =>
            originalValue is IMetadataValue metadataValue ? GetValue(metadataValue.Get(this)) : originalValue;

        /// <summary>
        /// This resolves the metadata value by expanding IMetadataValue.
        /// </summary>
        private KeyValuePair<string, object> GetItem(KeyValuePair<string, object> item) =>
            item.Value is IMetadataValue metadataValue
                ? new KeyValuePair<string, object>(item.Key, GetValue(metadataValue.Get(this)))
                : item;
    }
}