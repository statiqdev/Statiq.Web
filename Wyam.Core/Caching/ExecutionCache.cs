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
            return TryGetValue(document.Content, out value);
        }

        public bool TryGetValue<TValue>(IDocument document, out TValue value)
        {
            return TryGetValue<TValue>(document.Content, out value);
        }

        public void Set(IDocument document, object value)
        {
            Set(document.Content, value);
        }

        public bool TryGetValue(string key, out object value)
        {
            return TryGetValue<object>(key, out value);
        }

        public bool TryGetValue<TValue>(string key, out TValue value)
        {
            CacheEntry entry;
            if (_cache.TryGetValue(key, out entry))
            {
                entry.Hit = true;
                value = (TValue)entry.Value;
                return true;
            }
            value = default(TValue);
            return false;
        }

        public void Set(string key, object value)
        {
            _cache[key] = new CacheEntry
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
