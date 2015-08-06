using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common;

namespace Wyam.Core.Caching
{
    internal class ExecutionCacheManager
    {
        private readonly Dictionary<IModule, ExecutionCache> _executionCaches = new Dictionary<IModule, ExecutionCache>();
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
            if (_noCache)
            {
                return new NoCache();
            }

            ExecutionCache cache;
            if (!_executionCaches.TryGetValue(module, out cache))
            {
                cache = new ExecutionCache(engine);
                _executionCaches.Add(module, cache);
            }
            return cache;
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
