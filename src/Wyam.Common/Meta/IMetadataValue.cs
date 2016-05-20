namespace Wyam.Common.Meta
{
    /// <summary>
    /// Implement this interface to provide lazy metadata values or values based on other metadata.
    /// </summary>
    public interface IMetadataValue
    {
        /// <summary>
        /// Gets the value for the specified key. This method will be called 
        /// for each request of this metadata value and the return object will
        /// be processed like any other metadata value. The implementation 
        /// of this method must be thread-safe.
        /// </summary>
        /// <param name="key">The metadata key.</param>
        /// <param name="metadata">The metadata value.</param>
        /// <returns>The object to use as the value for the requested key.</returns>
        object Get(string key, IMetadata metadata);
    }
}
