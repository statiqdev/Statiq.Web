using System;
using System.Collections.Generic;
using Wyam.Core.Meta;

namespace Wyam.Core.Util
{
    /// <summary>
    /// Adapts a typed equality comparer to untyped metadata by attempting to convert the
    /// metadata values to the comparer type before running the comparison. If neither type
    /// can be converted to <typeparamref name="TValue"/>, the comparison returns 0 (equivalent).
    /// </summary>
    /// <typeparam name="TValue">The value type to convert to for comparisons.</typeparam>
    internal class ConvertingComparer<TValue> : IComparer<object>
    {
        private readonly IComparer<TValue> _comparer;

        public ConvertingComparer(IComparer<TValue> comparer)
        {
            _comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
        }

        public int Compare(object x, object y)
        {
            TValue xValue;
            TValue yValue;
            if (!TypeHelper.TryConvert(x, out xValue) || !TypeHelper.TryConvert(y, out yValue))
            {
                return 0;
            }
            return _comparer.Compare(xValue, yValue);
        }
    }
}