using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using TB.ComponentModel;

namespace Wyam.Core.Meta
{
    // These are used by MetadataAs for enumerable conversions, but must be declared outside for easier reflection instantiation
    internal abstract class MetadataTypeConverter
    {
        public abstract IEnumerable ToReadOnlyList(IEnumerable enumerable);
        public abstract IEnumerable ToList(IEnumerable enumerable);
        public abstract IEnumerable ToArray(IEnumerable enumerable);
        public abstract IEnumerable ToEnumerable(IEnumerable enumerable);
    }
}
