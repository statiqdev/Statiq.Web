using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Core.Documents;
using Wyam.Core.Meta;

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

            // IReadOnlyList<>
            if (typeof(T).IsConstructedGenericType && typeof(T).GetGenericTypeDefinition() == typeof(IReadOnlyList<>))
            {
                return TryConvertEnumerable(value, x => x.GetGenericArguments()[0], (x, y) => x.ToReadOnlyList(y), out result);
            }

            // IList<>
            if (typeof(T).IsConstructedGenericType 
                && (typeof(T).GetGenericTypeDefinition() == typeof(IList<>)
                    || typeof(T).GetGenericTypeDefinition() == typeof(List<>)))
            {
                return TryConvertEnumerable(value, x => x.GetGenericArguments()[0], (x, y) => x.ToList(y), out result);
            }

            // Array
            if (typeof(Array).IsAssignableFrom(typeof(T)) 
                || (typeof(T).IsArray && typeof(T).GetArrayRank() == 1))
            {
                return TryConvertEnumerable(value, x => x.GetElementType() ?? typeof(object), (x, y) => x.ToArray(y), out result);
            }

            // IEnumerable<>
            if (typeof(T).IsConstructedGenericType && typeof(T).GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                return TryConvertEnumerable(value, x => x.GetGenericArguments()[0], (x, y) => x.ToEnumerable(y), out result);
            }

            return false;
        }

        private static bool TryConvertEnumerable<T>(object value, Func<Type, Type> elementTypeFunc,
            Func<MetadataTypeConverter, IEnumerable, object> conversionFunc, out T result)
        {
            Type elementType = elementTypeFunc(typeof (T));
            IEnumerable enumerable = value is string ? null : value as IEnumerable;
            if (enumerable == null || elementType.IsInstanceOfType(value))
            {
                enumerable = new[] { value };
            }
            Type adapterType = typeof(MetadataTypeConverter<>).MakeGenericType(elementType);
            MetadataTypeConverter converter = (MetadataTypeConverter)Activator.CreateInstance(adapterType);
            result = (T)conversionFunc(converter, enumerable);
            return true;
        }
    }
}
