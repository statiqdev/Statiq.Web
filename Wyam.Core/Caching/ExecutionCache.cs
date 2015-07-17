using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Abstractions;

namespace Wyam.Core.Caching
{
    internal class ExecutionCache : IExecutionCache
    {
        private Dictionary<string, CacheEntry> _cache = new Dictionary<string, CacheEntry>(); 

        private class CacheEntry
        {
            public object Value { get; set; }
            public bool Hit { get; set; }
        }

        public bool TryGetValue(IDocument document, out object value)
        {
            CacheEntry entry;
            if (_cache.TryGetValue(document.Content, out entry))
            {
                entry.Hit = true;
                value = entry.Value;
                return true;
            }
            value = null;
            return false;
        }

        public void Set(IDocument document, object value)
        {
            _cache[document.Content] = new CacheEntry
            {
                Value = value,
                Hit = true
            };
        }

        public void ResetEntryHits()
        {
            foreach (CacheEntry entry in _cache.Values)
            {
                entry.Hit = false;
            }
        }

        public void ClearUnhitEntries()
        {
            // Faster just to reset the dictionary then iterate twice, once to get keys to remove and again to actually remove
            _cache = _cache.Where(x => x.Value.Hit).ToDictionary(x => x.Key, x => x.Value);
        }
    }
}
