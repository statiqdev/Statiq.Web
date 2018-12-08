using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using TB.ComponentModel;

namespace Wyam.Core.Meta
{
    internal class MetadataTypeConverter<T> : MetadataTypeConverter
    {
        public override IEnumerable ToReadOnlyList(IEnumerable enumerable)
        {
            return ConvertEnumerable(enumerable).ToImmutableArray();
        }

        public override IEnumerable ToList(IEnumerable enumerable)
        {
            return ConvertEnumerable(enumerable).ToList();
        }

        public override IEnumerable ToArray(IEnumerable enumerable)
        {
            return ConvertEnumerable(enumerable).ToArray();
        }

        public override IEnumerable ToEnumerable(IEnumerable enumerable)
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
