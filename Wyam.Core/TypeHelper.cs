using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Core.Documents;

namespace Wyam.Core
{
    public static class TypeHelper
    {
        public static bool TryConvert<T>(object value, out T result)
        {
            // Check for null
            if (value == null)
            {
                result = default(T);
                return !typeof (T).IsValueType;
            }

            // Just return if they're the same type
            if (typeof(T) == value.GetType())
            {
                result = (T)value;
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

            // IReadOnlyList<>
            if (typeof(T).IsConstructedGenericType && typeof(T).GetGenericTypeDefinition() == typeof(IReadOnlyList<>))
            {
                Type elementType = typeof(T).GetGenericArguments()[0];
                Type adapterType = typeof(MetadataTypeConverter<>).MakeGenericType(elementType);
                MetadataTypeConverter converter = (MetadataTypeConverter)Activator.CreateInstance(adapterType);
                result = (T)converter.ToReadOnlyList(enumerable);
                return true;
            }

            // IList<>
            if (typeof(T).IsConstructedGenericType && typeof(T).GetGenericTypeDefinition() == typeof(IList<>))
            {
                Type elementType = typeof(T).GetGenericArguments()[0];
                Type adapterType = typeof(MetadataTypeConverter<>).MakeGenericType(elementType);
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
