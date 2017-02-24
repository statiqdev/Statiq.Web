namespace Wyam.Common.Meta
{
    /// <summary>
    /// Implement this interface to provide lazy metadata values or values based on other metadata.
    /// </summary>
    public interface IMetadataValue
    {
        /// <summary>
        /// Lazily loads a metadata value. This method will be called
        /// for each request and the return object will
        /// be processed like any other metadata value. The implementation
        /// of this method must be thread-safe.
        /// </summary>
        /// <param name="metadata">The metadata object requesting the value.</param>
        /// <returns>The object to use as the value.</returns>
        object Get(IMetadata metadata);
    }
}
