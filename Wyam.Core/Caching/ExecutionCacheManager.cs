using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Abstractions;

namespace Wyam.Core.Caching
{
    internal class ExecutionCacheManager
    {
        private readonly Dictionary<IModule, ExecutionCache> _executionCaches = new Dictionary<IModule, ExecutionCache>();

        // Creates one if it doesn't exist
        public IExecutionCache Get(IModule module)
        {
            ExecutionCache cache;
            if (!_executionCaches.TryGetValue(module, out cache))
            {
                cache = new ExecutionCache();
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
            foreach (ExecutionCache cache in _executionCaches.Values)
            {
                cache.ClearUnhitEntries();
            }
        }
    }
}
