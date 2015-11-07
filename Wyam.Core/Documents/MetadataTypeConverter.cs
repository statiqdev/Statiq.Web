using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using TB.ComponentModel;

namespace Wyam.Core.Documents
{
    // These are used by MetadataAs for enumerable conversions, but must be declared outside for easier reflection instantiation
    internal abstract class MetadataTypeConverter
    {
        public abstract object ToReadOnlyList(IEnumerable enumerable);
        public abstract object ToList(IEnumerable enumerable);
        public abstract object ToArray(IEnumerable enumerable);
        public abstract object ToEnumerable(IEnumerable enumerable);
    }

    internal class MetadataTypeConverter<T> : MetadataTypeConverter
    {
        public override object ToReadOnlyList(IEnumerable enumerable)
        {
            return ConvertEnumerable(enumerable).ToImmutableArray();
        }

        public override object ToList(IEnumerable enumerable)
        {
            return ConvertEnumerable(enumerable).ToList();
        }

        public override object ToArray(IEnumerable enumerable)
        {
            return ConvertEnumerable(enumerable).ToArray();
        }

        public override object ToEnumerable(IEnumerable enumerable)
        {
            return ConvertEnumerable(enumerable);
        }

        public static bool TryConvert(object value, out T result)
        {
            return UniversalTypeConverter.TryConvertTo(value, out result);
        }

        private static IEnumerable<T> ConvertEnumerable(IEnumerable enumerable)
        {
            foreach (object value in enumerable)
            {
                T result;
                if (TryConvert(value, out result))
                {
                    yield return result;
                }
            }
        }
    }
}
