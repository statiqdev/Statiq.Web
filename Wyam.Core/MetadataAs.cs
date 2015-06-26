using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TB.ComponentModel;
using Wyam.Abstractions;

namespace Wyam.Core
{
    // This wraps the Metadata class and provides strongly-typed access
    // See http://www.codeproject.com/Articles/248440/Universal-Type-Converter for conversion library
    // Only values that can be converted to the requested type are considered part of the dictionary
    internal class MetadataAs<T> : IMetadata<T>
    {
        private readonly IMetadata _metadata;

        public MetadataAs(IMetadata metadata)
        {
            if (metadata == null)
            {
                throw new ArgumentNullException("metadata");
            }
            _metadata = metadata;
        }

        public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
        {
            return _metadata
                .Select(x =>
                {
                    T value;
                    return TryConvert(x.Value, out value)
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
                    return TryConvert(x.Value, out value);
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
                    throw new KeyNotFoundException();
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
            return _metadata.TryGetValue(key, out untypedValue) && TryConvert(untypedValue, out value);
        }

        private static bool TryConvert(object value, out T result)
        {
            // Just return if they're the same type
            if (typeof (T) == value.GetType())
            {
                result = (T) value;
                return true;
            }

            // Check a normal conversion (in case it's a special type that implements a cast, IConvertible, or something)
            if (MetadataTypeConverter<T>.TryConvert(value, out result))
            {
                return true;
            }

            // Check for enumerable conversions (but don't treat string as an enumerable)
            IEnumerable enumerable = value is string ? null : value as IEnumerable;
            enumerable = enumerable ?? new[] { value };

            // IList<>
            if (typeof(T).IsConstructedGenericType && typeof(T).GetGenericTypeDefinition() == typeof(IList<>))
            {
                Type elementType = typeof (T).GetGenericArguments()[0];
                Type adapterType = typeof (MetadataTypeConverter<>).MakeGenericType(elementType);
                MetadataTypeConverter converter = (MetadataTypeConverter)Activator.CreateInstance(adapterType);
                result = (T)converter.ToList(enumerable);
                return true;
            }

            // Array
            if (typeof(T).IsArray && typeof(T).GetArrayRank() == 1)
            {
                Type elementType = typeof(T).GetElementType();
                Type adapterType = typeof(MetadataTypeConverter<>).MakeGenericType(elementType);
                MetadataTypeConverter converter = (MetadataTypeConverter)Activator.CreateInstance(adapterType);
                result = (T)converter.ToArray(enumerable);
                return true;
            }

            // IEnumerable<>
            if (typeof(T).IsConstructedGenericType && typeof(T).GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                Type elementType = typeof(T).GetGenericArguments()[0];
                Type adapterType = typeof(MetadataTypeConverter<>).MakeGenericType(elementType);
                MetadataTypeConverter converter = (MetadataTypeConverter)Activator.CreateInstance(adapterType);
                result = (T)converter.ToEnumerable(enumerable);
                return true;
            }

            return false;
        }
    }
}
