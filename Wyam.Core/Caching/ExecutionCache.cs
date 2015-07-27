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
        private readonly Engine _engine;
        private Dictionary<string, CacheEntry> _cache = new Dictionary<string, CacheEntry>();

        public ExecutionCache(Engine engine)
        {
            _engine = engine;
        }

        private class CacheEntry
        {
            public object Value { get; set; }
            public bool Hit { get; set; }
        }

        public bool ContainsKey(IDocument document)
        {
            return ContainsKey(document.Content);
        }

        public bool ContainsKey(string key)
        {
            return _cache.ContainsKey(key);
        }

        public bool TryGetValue(IDocument document, out object value)
        {
            return TryGetValue(document.Content, out value);
        }

        public bool TryGetValue(string key, out object value)
        {
            return TryGetValue<object>(key, out value);
        }

        public bool TryGetValue<TValue>(IDocument document, out TValue value)
        {
            return TryGetValue<TValue>(document.Content, out value);
        }

        public bool TryGetValue<TValue>(string key, out TValue value)
        {
            CacheEntry entry;
            if (_cache.TryGetValue(key, out entry))
            {
                entry.Hit = true;
                value = (TValue)entry.Value;
                _engine.Trace.Verbose("Cache hit for key {0}", key.Length <= 128 ? key : key.Substring(0, 128));
                return true;
            }
            value = default(TValue);
            _engine.Trace.Verbose("Cache miss for key {0}", key.Length <= 128 ? key : key.Substring(0, 128));
            return false;
        }

        public void Set(IDocument document, object value)
        {
            Set(document.Content, value);
        }

        public void Set(string key, object value)
        {
            _cache[key] = new CacheEntry
            {
                Value = value,
                Hit = true
            };
            _engine.Trace.Verbose("Cache set value for key {0}", key.Length <= 128 ? key : key.Substring(0, 128));
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
            int count = _cache.Count;
            _cache = _cache.Where(x => x.Value.Hit).ToDictionary(x => x.Key, x => x.Value);
            _engine.Trace.Verbose("Removed {0} stale cache entries", count - _cache.Count);
        }
    }
}
