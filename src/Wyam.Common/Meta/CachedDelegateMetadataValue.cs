using System;
using Wyam.Common.Documents;

namespace Wyam.Common.Meta
{
    /// <summary>
    /// This class uses a delegate to get a metadata value. The result of the delegate
    /// will be cached and the cached value will be returned for subsequent calls to <see cref="Get"/>.
    /// </summary>
    public class CachedDelegateMetadataValue : DelegateMetadataValue
    {
        private object _cachedValue;
        private bool _cached;

        /// <summary>
        /// Initializes a new instance of the <see cref="CachedDelegateMetadataValue"/> class.
        /// The specified delegate should be thread-safe.
        /// </summary>
        /// <param name="value">The delegate that returns the metadata value.</param>
        public CachedDelegateMetadataValue(Func<IMetadata, object> value) : base(value)
        {
        }

        /// <summary>
        /// Lazily loads a metadata value. This method will be called 
        /// for each request and the return object will
        /// be processed like any other metadata value. The implementation 
        /// of this method must be thread-safe.
        /// </summary>
        /// <param name="metadata">The metadata object requesting the value.</param>
        /// <returns>The object to use as the value.</returns>
        public override object Get(IMetadata metadata)
        {
            if (!_cached)
            {
                _cachedValue = base.Get(metadata);
                _cached = true;
            }
            return _cachedValue;
        }
    }
}