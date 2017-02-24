using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Wyam.Core.Meta
{
    public static class TypeHelper
    {
        public static bool TryConvert<T>(object value, out T result)
        {
            // Check for null
            if (value == null)
            {
                result = default(T);
                return !typeof (T).IsValueType
                    || (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(Nullable<>));
            }

            // Just return if they're the same type
            if (typeof(T) == value.GetType())
            {
                result = (T)value;
                return true;
            }

            // Special case if value is an enumerable that hasn't overridden .ToString() and T is a string
            // Otherwise we'd end up doing a .ToString() on the enumerable
            IEnumerable enumerableValue = value is string ? null : value as IEnumerable;
            if (typeof(T) == typeof(string) && enumerableValue != null
                && value.GetType().GetMethod("ToString").DeclaringType == typeof(object))
            {
                if (TryGetFirstConvertibleItem(enumerableValue, out result))
                {
                    return true;
                }
                enumerableValue = null;  // Don't try getting the first item again for the more general case below
            }

            // Check a normal conversion (in case it's a special type that implements a cast, IConvertible, or something)
            if (MetadataTypeConverter<T>.TryConvert(value, out result))
            {
                return true;
            }

            // If value is an enumerable but the result type is not, return the first convertible item
            if (enumerableValue != null && !typeof(IEnumerable).IsAssignableFrom(typeof(T)))
            {
                return TryGetFirstConvertibleItem(enumerableValue, out result);
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

        private static bool TryGetFirstConvertibleItem<T>(IEnumerable value, out T result)
        {
            MetadataTypeConverter<T> converter = new MetadataTypeConverter<T>();
            bool gotResult = true;
            result = ((IEnumerable<T>)converter.ToEnumerable(value))
                .Select(x =>
                {
                    gotResult = true;
                    return x;
                }).FirstOrDefault();
            return gotResult;
        }

        private static bool TryConvertEnumerable<T>(object value, Func<Type, Type> elementTypeFunc,
            Func<MetadataTypeConverter, IEnumerable, IEnumerable> conversionFunc, out T result)
        {
            Type elementType = elementTypeFunc(typeof (T));
            IEnumerable enumerable = value is string ? null : value as IEnumerable;
            if (enumerable == null || (elementType.IsInstanceOfType(value) && elementType != typeof(object)))
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
