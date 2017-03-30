using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Common.Meta
{
    /// <summary>
    /// A mutable <see cref="IMetadata"/> implementation that works like a dictionary.
    /// </summary>
    public interface IMetadataDictionary : IDictionary<string, object>, IMetadata
    {
        /// <summary>
        /// The count of metadata.
        /// </summary>
        new int Count { get; }

        /// <summary>
        /// Whether or not the metadata contains the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns><c>true</c> if the key is contained in metadata, <c>false</c> otherwise.</returns>
        new bool ContainsKey(string key);

        /// <summary>
        /// A collection of keys in the metadata.
        /// </summary>
        new ICollection<string> Keys { get; }

        /// <summary>
        /// A collection of values in the metadata.
        /// </summary>
        new ICollection<object> Values { get; }

        /// <summary>
        /// Attempts to get a value from metadata.
        /// </summary>
        /// <param name="key">The metadata key.</param>
        /// <param name="value">The metadata value.</param>
        /// <returns><c>true</c> if the key is contained in metadata, <c>false</c> otherwise.</returns>
        new bool TryGetValue(string key, out object value);

        /// <summary>
        /// Gets a metadata value given the specified metadata key.
        /// </summary>
        /// <param name="key">The metadata key.</param>
        /// <returns>The value at the specified key.</returns>
        new object this[string key] { get; set; }
    }
}