using System;
using System.Collections.Generic;
using System.Linq;
using Wyam.Common.Util;

namespace Wyam.Common.Documents
{
    /// <summary>
    /// Extensions for working with specific types of collections.
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// Returns all documents that contain the specified metadata key.
        /// </summary>
        /// <param name="documents">The documents.</param>
        /// <param name="metadataKey">The key.</param>
        /// <returns>All documents that contain the specified metadata key.</returns>
        public static IEnumerable<IDocument> WhereContainsKey(this IEnumerable<IDocument> documents, string metadataKey)
        {
            return documents.Where(x => x.ContainsKey(metadataKey));
        }

        /// <summary>
        /// Returns all documents that contain all of the specified metadata keys.
        /// </summary>
        /// <param name="documents">The documents.</param>
        /// <param name="metadataKeys">The metadata keys.</param>
        /// <returns>All documents that contain all of the specified metadata keys.</returns>
        public static IEnumerable<IDocument> WhereContainsAllKeys(this IEnumerable<IDocument> documents, params string[] metadataKeys)
        {
            return documents.Where(x => metadataKeys.All(x.ContainsKey));
        }

        /// <summary>
        /// Returns all documents that contain any of the specified metadata keys.
        /// </summary>
        /// <param name="documents">The documents.</param>
        /// <param name="metadataKeys">The metadata keys.</param>
        /// <returns>All documents that contain any of the specified metadata keys.</returns>
        public static IEnumerable<IDocument> WhereContainsAnyKeys(this IEnumerable<IDocument> documents, params string[] metadataKeys)
        {
            return documents.Where(x => metadataKeys.Any(x.ContainsKey));
        }
    }
}
