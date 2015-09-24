using System;

namespace Wyam.Common
{
    // This uses a delegate to get the metadata value and caches the result
    // This results in a tradeoff between memory consumption and performance (both potentially greater if the value cached)
    // Note that the provided delegate should be thread-safe
    public class CachedDelegateMetadataValue : DelegateMetadataValue
    {
        private object _cachedValue;
        private bool _cached;

        public CachedDelegateMetadataValue(Func<string, IMetadata, object> value) : base(value)
        {
        }

        public override object Get(string key, IMetadata metadata)
        {
            if (!_cached)
            {
                _cachedValue = base.Get(key, metadata);
                _cached = true;
            }
            return _cachedValue;
        }
    }
}