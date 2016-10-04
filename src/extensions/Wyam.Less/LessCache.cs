using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dotless.Core.Cache;
using Wyam.Common;
using Wyam.Common.Caching;

namespace Wyam.Less
{
    internal class LessCache : ICache
    {
        private readonly IExecutionCache _cache;

        public LessCache(IExecutionCache cache)
        {
            _cache = cache;
        }

        public void Insert(string cacheKey, IEnumerable<string> fileDependancies, string css)
        {
            _cache.Set(cacheKey, css);
        }

        public bool Exists(string cacheKey)
        {
            return _cache.ContainsKey(cacheKey);
        }

        public string Retrieve(string cacheKey)
        {
            string css;
            if (!_cache.TryGetValue(cacheKey, out css))
            {
                throw new KeyNotFoundException("Cache key was not found");
            }
            return css;
        }
    }
}
