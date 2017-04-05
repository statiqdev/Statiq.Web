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
        /// <summary>
        /// Gets the underlying <c>KeyValuePair&lt;TKey, TValue&gt;</c>.
        /// </summary>
        public KeyValuePair<string, object> Pair { get; }

        /// <summary>
        /// Gets the key of the item.
        /// </summary>
        public string Key => Pair.Key;

        /// <summary>
        /// Gets the value of the item.
        /// </summary>
        public object Value => Pair.Value;

        /// <summary>
        /// Creates a new metadata item with a specified key-value pair.
        /// </summary>
        /// <param name="pair">The key-value pair.</param>
        public MetadataItem(KeyValuePair<string, object> pair)
        {
            Pair = pair;
        }

        /// <summary>
        /// Creates a new metadata item with the specified key and value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public MetadataItem(string key, object value)
        {
            Pair = new KeyValuePair<string, object>(key, value);
        }

        /// <summary>
        /// This creates a new metadata value based on the specified delegate that will get
        /// evaluated on every value request. Note that the delegate function should be thread-safe.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value delegate.</param>
        /// <param name="cacheValue">if set to <c>true</c>, cache the value after the first request.</param>
        public MetadataItem(string key, Func<IMetadata, object> value, bool cacheValue = false)
        {
            Pair = new KeyValuePair<string, object>(
                key,
                cacheValue ? new CachedDelegateMetadataValue(value) : new DelegateMetadataValue(value));
        }

        /// <summary>
        /// Converts a key-value pair to a <see cref="MetadataItem"/>.
        /// </summary>
        /// <param name="pair">The key-value pair to convert.</param>
        public static implicit operator MetadataItem(KeyValuePair<string, object> pair)
        {
            return new MetadataItem(pair);
        }

        /// <summary>
        /// Converts a <see cref="MetadataItem"/> to a key-value pair.
        /// </summary>
        /// <param name="item">The metadata item to convert.</param>
        public static implicit operator KeyValuePair<string, object>(MetadataItem item)
        {
            return item.Pair;
        }
    }
}
