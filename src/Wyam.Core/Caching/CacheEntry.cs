using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Core.Caching
{
    internal class CacheEntry<TValue>
    {
        public TValue Value { get; set; }
        public bool Hit { get; set; }
    }
}
