using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common;

namespace Wyam.Core.Caching
{
    internal class ExecutionCache : IExecutionCache
    {
        private readonly Engine _engine;
        private ConcurrentDictionary<string, CacheEntry> _cache 
            = new ConcurrentDictionary<string, CacheEntry>();

        public ExecutionCache(Engine engine)
        {
            _engine = engine;
        }

        private class CacheEntry
        {
            public object Value { get; set; }
            public bool Hit { get; set; }
        }

        private string GetDocumentKey(IDocument document)
        {
            return document.Source + " " + Crc32.Calculate(document.Stream);
        }

        public bool ContainsKey(IDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            return ContainsKey(GetDocumentKey(document));
        }

        public bool ContainsKey(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return _cache.ContainsKey(key);
        }

        public bool TryGetValue(IDocument document, out object value)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            return TryGetValue(GetDocumentKey(document), out value);
        }

        public bool TryGetValue(string key, out object value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return TryGetValue<object>(key, out value);
        }

        public bool TryGetValue<TValue>(IDocument document, out TValue value)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            return TryGetValue<TValue>(GetDocumentKey(document), out value);
        }

        public bool TryGetValue<TValue>(string key, out TValue value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            CacheEntry entry;
            if (_cache.TryGetValue(key, out entry))
            {
                entry.Hit = true;
                value = (TValue)entry.Value;
                _engine.Trace.Verbose("Cache hit for key: {0}", key);
                return true;
            }
            value = default(TValue);
            _engine.Trace.Verbose("Cache miss for key: {0}", key);
            return false;
        }

        public void Set(IDocument document, object value)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            Set(GetDocumentKey(document), value);
        }

        public void Set(string key, object value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            _cache[key] = new CacheEntry
            {
                Value = value,
                Hit = true
            };
            _engine.Trace.Verbose("Cache set for key: {0}", key);
        }

        public void ResetEntryHits()
        {
            foreach (CacheEntry entry in _cache.Values)
            {
                entry.Hit = false;
            }
        }

        public void ClearUnhitEntries(IModule module)
        {
            // Faster just to reset the dictionary then iterate twice, once to get keys to remove and again to actually remove
            int count = _cache.Count;
            _cache = new ConcurrentDictionary<string, CacheEntry>(
                _cache.Where(x => x.Value.Hit).ToDictionary(x => x.Key, x => x.Value));
            _engine.Trace.Verbose("Removed {0} stale cache entries for module {1}", count - _cache.Count, module.GetType().Name);
        }
    }
}
