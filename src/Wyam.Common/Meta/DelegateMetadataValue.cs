using System;
using Wyam.Common.Documents;

namespace Wyam.Common.Meta
{
    /// <summary>
    /// This class uses a delegate to get a metadata value.
    /// </summary>
    public class DelegateMetadataValue : IMetadataValue
    {
        private readonly Func<string, IMetadata, object> _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateMetadataValue"/> class. 
        /// The specified delegate should be thread-safe.
        /// </summary>
        /// <param name="value">The delegate that returns the metadata value.</param>
        public DelegateMetadataValue(Func<string, IMetadata, object> value)
        {
            _value = value;
        }

        /// <summary>
        /// Gets the value for the specified key. This method will be called
        /// for each request of this metadata value and the return object will
        /// be processed like any other metadata value. The implementation
        /// of this method must be thread-safe.
        /// </summary>
        /// <param name="key">The metadata key.</param>
        /// <param name="metadata">The metadata value.</param>
        /// <returns>
        /// The object to use as the value for the requested key.
        /// </returns>
        public virtual object Get(string key, IMetadata metadata)
        {
            return _value(key, metadata);
        }
    }
}
