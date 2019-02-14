using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Wyam.Common.Meta;

namespace Wyam.Core.Meta
{
    // This wraps the Metadata class and provides strongly-typed access
    // See http://www.codeproject.com/Articles/248440/Universal-Type-Converter for conversion library
    // Only values that can be converted to the requested type are considered part of the dictionary
    internal class MetadataAs<T> : IMetadata<T>
    {
        private readonly IMetadata _metadata;

        public MetadataAs(IMetadata metadata)
        {
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        }

        public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
        {
            return _metadata
                .Select(x => TypeHelper.TryConvert(x.Value, out T value)
                    ? new KeyValuePair<string, T>?(new KeyValuePair<string, T>(x.Key, value))
                    : null)
                .Where(x => x.HasValue)
                .Select(x => x.Value)
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => _metadata.Count(x => TypeHelper.TryConvert(x.Value, out T value));

        public bool ContainsKey(string key) => TryGetValue(key, out T value);

        public T this[string key]
        {
            get
            {
                if (!TryGetValue(key, out T value))
                {
                    throw new KeyNotFoundException("The key " + key + " was not found in metadata, use Get() to provide a default value.");
                }
                return value;
            }
        }

        public IEnumerable<string> Keys => this.Select(x => x.Key);

        public IEnumerable<T> Values => this.Select(x => x.Value);

        public T Get(string key) => TryGetValue(key, out T value) ? value : default(T);

        public T Get(string key, T defaultValue) => TryGetValue(key, out T value) ? value : defaultValue;

        public bool TryGetValue(string key, out T value) => _metadata.TryGetValue(key, out value);
    }
}
