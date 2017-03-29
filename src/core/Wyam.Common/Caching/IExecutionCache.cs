using Wyam.Common.Documents;

namespace Wyam.Common.Caching
{
    /// <summary>
    /// Provides a cache that can be used by modules during execution to persist data
    /// between engine executions. Cached data is keyed by both a document and an
    /// optional key string.
    /// </summary>
    public interface IExecutionCache
    {
        /// <summary>
        /// Checks if the document key is in the cache.
        /// </summary>
        /// <param name="document">The document key.</param>
        /// <returns><c>true</c> if the key is in the cache, <c>false</c> otherwise.</returns>
        bool ContainsKey(IDocument document);

        /// <summary>
        /// Checks if the document and string key is in the cache.
        /// </summary>
        /// <param name="document">The document key.</param>
        /// <param name="key">The string key.</param>
        /// <returns><c>true</c> if the key is in the cache, <c>false</c> otherwise.</returns>
        bool ContainsKey(IDocument document, string key);

        /// <summary>
        /// Checks if the string key is in the cache.
        /// </summary>
        /// <param name="key">The string key.</param>
        /// <returns><c>true</c> if the key is in the cache, <c>false</c> otherwise.</returns>
        bool ContainsKey(string key);

        /// <summary>
        /// Attempts to get a cached value from a document key.
        /// </summary>
        /// <param name="document">The document key.</param>
        /// <param name="value">The cached value.</param>
        /// <returns><c>true</c> for a cache hit, <c>false</c> for a cache miss.</returns>
        bool TryGetValue(IDocument document, out object value);

        /// <summary>
        /// Attempts to get a cached value from a document and string key.
        /// </summary>
        /// <param name="document">The document key.</param>
        /// <param name="key">The string key.</param>
        /// <param name="value">The cached value.</param>
        /// <returns><c>true</c> for a cache hit, <c>false</c> for a cache miss.</returns>
        bool TryGetValue(IDocument document, string key, out object value);

        /// <summary>
        /// Attempts to get a cached value from a string key.
        /// </summary>
        /// <param name="key">The string key.</param>
        /// <param name="value">The cached value.</param>
        /// <returns><c>true</c> for a cache hit, <c>false</c> for a cache miss.</returns>
        bool TryGetValue(string key, out object value);

        /// <summary>
        /// Attempts to get a typed cached value from a document key.
        /// </summary>
        /// <typeparam name="TValue">The type of the cached value.</typeparam>
        /// <param name="document">The document key.</param>
        /// <param name="value">The cached value.</param>
        /// <returns><c>true</c> for a cache hit, <c>false</c> for a cache miss.</returns>
        bool TryGetValue<TValue>(IDocument document, out TValue value);

        /// <summary>
        /// Attempts to get a typed cached value from a document and string key.
        /// </summary>
        /// <typeparam name="TValue">The type of the cached value.</typeparam>
        /// <param name="document">The document key.</param>
        /// <param name="key">The string key.</param>
        /// <param name="value">The cached value.</param>
        /// <returns><c>true</c> for a cache hit, <c>false</c> for a cache miss.</returns>
        bool TryGetValue<TValue>(IDocument document, string key, out TValue value);

        /// <summary>
        /// Attempts to get a typed cached value from a string key.
        /// </summary>
        /// <typeparam name="TValue">The type of the cached value.</typeparam>
        /// <param name="key">The string key.</param>
        /// <param name="value">The cached value.</param>
        /// <returns><c>true</c> for a cache hit, <c>false</c> for a cache miss.</returns>
        bool TryGetValue<TValue>(string key, out TValue value);

        /// <summary>
        /// Sets a cached value from a document key.
        /// </summary>
        /// <param name="document">The document key.</param>
        /// <param name="value">The cached value.</param>
        void Set(IDocument document, object value);

        /// <summary>
        /// Sets a cached value from a document and string key.
        /// </summary>
        /// <param name="document">The document key.</param>
        /// <param name="key">The string key.</param>
        /// <param name="value">The cached value.</param>
        void Set(IDocument document, string key, object value);

        /// <summary>
        /// Sets a cached value from a document key.
        /// </summary>
        /// <param name="key">The string key.</param>
        /// <param name="value">The cached value.</param>
        void Set(string key, object value);
    }
}
