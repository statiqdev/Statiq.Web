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
                .Select(x =>
                {
                    T value;
                    return TypeHelper.TryConvert(x.Value, out value)
                        ? new KeyValuePair<string, T>?(new KeyValuePair<string, T>(x.Key, value))
                        : null;
                })
                .Where(x => x.HasValue)
                .Select(x => x.Value)
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count
        {
            get
            {
                return _metadata.Count(x =>
                {
                    T value;
                    return TypeHelper.TryConvert(x.Value, out value);
                });
            }
        }

        public bool ContainsKey(string key)
        {
            T value;
            return TryGetValue(key, out value);
        }

        public T this[string key]
        {
            get
            {
                T value;
                if (!TryGetValue(key, out value))
                {
                    throw new KeyNotFoundException("The key " + key + " was not found in metadata, use Get() to provide a default value.");
                }
                return value;
            }
        }

        public IEnumerable<string> Keys
        {
            get { return this.Select(x => x.Key); }
        }

        public IEnumerable<T> Values
        {
            get { return this.Select(x => x.Value); }
        }

        public T Get(string key)
        {
            T value;
            return TryGetValue(key, out value) ? value : default(T);
        }

        public T Get(string key, T defaultValue)
        {
            T value;
            return TryGetValue(key, out value) ? value : defaultValue;
        }

        public bool TryGetValue(string key, out T value)
        {
            value = default(T);
            object untypedValue;
            return _metadata.TryGetValue(key, out untypedValue) && TypeHelper.TryConvert(untypedValue, out value);
        }
    }
}
