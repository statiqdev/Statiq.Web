using System;
using System.Collections.Generic;
using System.Linq;

namespace Wyam.Common.Meta
{
    /// <summary>
    /// Provides slightly nicer syntax than <c>KeyValuePair&lt;string, object&gt;</c> for working
    /// with metadata. Also contains a constructor that makes adding delegate-based metadata easier.
    /// </summary>
    public struct MetadataItem
    {
        public KeyValuePair<string, object> Pair { get; }

        public string Key => Pair.Key;

        public object Value => Pair.Value;

        public MetadataItem(KeyValuePair<string, object> pair)
        {
            Pair = pair;
        }

        public MetadataItem(string key, object value)
        {
            Pair = new KeyValuePair<string, object>(key, value);
        }

        // This creates a new DelegateMetadataValue that will get evaluated on every value request
        // Note that the delegate function should be thread-safe
        public MetadataItem(string key, Func<string, IMetadata, object> value, bool cacheValue = false)
        {
            Pair = new KeyValuePair<string, object>(key,
                cacheValue ? new CachedDelegateMetadataValue(value) : new DelegateMetadataValue(value));
        }

        public static implicit operator MetadataItem(KeyValuePair<string, object> pair)
        {
            return new MetadataItem(pair);
        }

        public static implicit operator KeyValuePair<string, object>(MetadataItem item)
        {
            return item.Pair;
        }
    }
}
