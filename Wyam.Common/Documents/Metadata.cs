using System;
using System.Collections.Generic;

namespace Wyam.Common.Documents
{
    public static class Metadata
    {
        public static KeyValuePair<string, object> Create(string key, object value)
        {
            return new KeyValuePair<string, object>(key, value);
        }

        // This creates a new DelegateMetadataValue that will get evaluated on every value request
        // Note that the delegate function should be thread-safe
        public static KeyValuePair<string, object> Create(string key, Func<string, IMetadata, object> value, bool cacheValue = false)
        {
            return new KeyValuePair<string, object>(key,
                cacheValue ? new CachedDelegateMetadataValue(value) : new DelegateMetadataValue(value));
        } 
    }
}
