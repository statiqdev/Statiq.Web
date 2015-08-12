using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common;

namespace Wyam.Core.Caching
{
    internal class ExecutionCacheManager
    {
        private readonly ConcurrentDictionary<IModule, ExecutionCache> _executionCaches 
            = new ConcurrentDictionary<IModule, ExecutionCache>();
        private bool _noCache;

        public bool NoCache
        {
            get { return _noCache; }
            set
            {
                _executionCaches.Clear();
                _noCache = value;
            }
        }

        // Creates one if it doesn't exist
        public IExecutionCache Get(IModule module, Engine engine)
        {
            return _noCache
                ? (IExecutionCache) new NoCache()
                : _executionCaches.GetOrAdd(module, new ExecutionCache(engine));
        }

        public void ResetEntryHits()
        {
            foreach (ExecutionCache cache in _executionCaches.Values)
            {
                cache.ResetEntryHits();
            }
        }

        public void ClearUnhitEntries()
        {
            foreach (KeyValuePair<IModule, ExecutionCache> item in _executionCaches)
            {
                item.Value.ClearUnhitEntries(item.Key);
            }
        }
    }
}
