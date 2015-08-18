using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common;

namespace Wyam.Core.Caching
{
    internal class Cache<TValue>
    {
        private ConcurrentDictionary<string, CacheEntry<TValue>> _cache
            = new ConcurrentDictionary<string, CacheEntry<TValue>>();

        protected static string GetDocumentKey(IDocument document)
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

        public bool TryGetValue(IDocument document, out TValue value)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            return TryGetValue(GetDocumentKey(document), out value);
        }

        public virtual bool TryGetValue(string key, out TValue value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            CacheEntry<TValue> entry;
            if (_cache.TryGetValue(key, out entry))
            {
                entry.Hit = true;
                value = entry.Value;
                return true;
            }
            value = default(TValue);
            return false;
        }

        public void Set(IDocument document, TValue value)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            Set(GetDocumentKey(document), value);
        }

        public virtual void Set(string key, TValue value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            _cache[key] = new CacheEntry<TValue>
            {
                Value = value,
                Hit = true
            };
        }

        public IEnumerable<TValue> GetValues()
        {
            return _cache.Values.Select(x => x.Value);
        }

        public int Count => _cache.Count;

        public void ResetEntryHits()
        {
            foreach (CacheEntry<TValue> entry in _cache.Values)
            {
                entry.Hit = false;
            }
        }

        public List<TValue> ClearUnhitEntries()
        {
            // Faster just to reset the dictionary then iterate twice, once to get keys to remove and again to actually remove
            List<TValue> clearedValues = new List<TValue>();
            ConcurrentDictionary<string, CacheEntry<TValue>> newCache = new ConcurrentDictionary<string, CacheEntry<TValue>>();
            foreach (KeyValuePair<string, CacheEntry<TValue>> item in _cache)
            {
                if (item.Value.Hit)
                {
                    newCache[item.Key] = item.Value;
                }
                else
                {
                    clearedValues.Add(item.Value.Value);
                }
            }
            _cache = newCache;
            return clearedValues;
        }
    }
}
