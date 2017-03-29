using System;
using System.Collections.Generic;
using System.Linq;
using Wyam.Common.Meta;
using Wyam.Common.Util;

namespace Wyam.Common.Documents
{
    /// <summary>
    /// Extensions grouping document sequences.
    /// </summary>
    public static class GroupByExtensions
    {
        /// <summary>
        /// Groups the elements of a sequence of documents using the values of a specified metadata key.
        /// If a document does not contain the specified metadata key, it is not included in the result set.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <param name="documents">The documents.</param>
        /// <param name="keyMetadataKey">The key metadata key.</param>
        /// <returns>A sequence of groups.</returns>
        public static IEnumerable<IGrouping<TKey, IDocument>> GroupBy<TKey>(
            this IEnumerable<IDocument> documents,
            string keyMetadataKey)
        {
            return documents.GroupBy<TKey>(keyMetadataKey, null);
        }

        /// <summary>
        /// Groups the elements of a sequence of documents and the values of a specified metadata key
        /// and compares the keys by using a specified comparer.
        /// If a document does not contain the specified metadata key, it is not included in the result set.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <param name="documents">The documents.</param>
        /// <param name="keyMetadataKey">The key metadata key.</param>
        /// <param name="comparer">The comparer.</param>
        /// <returns>A sequence of groups.</returns>
        public static IEnumerable<IGrouping<TKey, IDocument>> GroupBy<TKey>(
            this IEnumerable<IDocument> documents,
            string keyMetadataKey,
            IEqualityComparer<TKey> comparer)
        {
            if (documents == null)
            {
                throw new ArgumentNullException(nameof(documents));
            }
            if (keyMetadataKey == null)
            {
                throw new ArgumentNullException(nameof(keyMetadataKey));
            }

            return documents
                .Distinct()
                .Where(x => x.ContainsKey(keyMetadataKey))
                .GroupBy(x => x.Get<TKey>(keyMetadataKey), comparer);
        }

        /// <summary>
        /// Groups the elements of a sequence of documents and the values of a specified metadata key
        /// using the value of the specified element metadata for the elements of the group.
        /// If a document does not contain the specified key or element metadata keys, it is not included in the result set.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TElement">The type of the element.</typeparam>
        /// <param name="documents">The documents.</param>
        /// <param name="keyMetadataKey">The key metadata key.</param>
        /// <param name="elementMetadataKey">The element metadata key.</param>
        /// <returns>A sequence of groups.</returns>
        public static IEnumerable<IGrouping<TKey, TElement>> GroupBy<TKey, TElement>(
            this IEnumerable<IDocument> documents,
            string keyMetadataKey,
            string elementMetadataKey)
        {
            return documents.GroupBy<TKey, TElement>(keyMetadataKey, elementMetadataKey, null);
        }

        /// <summary>
        /// Groups the elements of a sequence of documents and the values of a specified metadata key
        /// using the value of the specified element metadata for the elements of the group
        /// and compares the keys by using a specified comparer.
        /// If a document does not contain the specified key or element metadata keys, it is not included in the result set.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TElement">The type of the element.</typeparam>
        /// <param name="documents">The documents.</param>
        /// <param name="keyMetadataKey">The key metadata key.</param>
        /// <param name="elementMetadataKey">The element metadata key.</param>
        /// <param name="comparer">The comparer.</param>
        /// <returns>A sequence of groups.</returns>
        public static IEnumerable<IGrouping<TKey, TElement>> GroupBy<TKey, TElement>(
            this IEnumerable<IDocument> documents,
            string keyMetadataKey,
            string elementMetadataKey,
            IEqualityComparer<TKey> comparer)
        {
            if (documents == null)
            {
                throw new ArgumentNullException(nameof(documents));
            }
            if (keyMetadataKey == null)
            {
                throw new ArgumentNullException(nameof(keyMetadataKey));
            }
            if (elementMetadataKey == null)
            {
                throw new ArgumentNullException(nameof(elementMetadataKey));
            }

            return documents
                .Distinct()
                .Where(x => x.ContainsKey(keyMetadataKey) && x.ContainsKey(elementMetadataKey))
                .GroupBy(x => x.Get<TKey>(keyMetadataKey), x => x.Get<TElement>(elementMetadataKey), comparer);
        }

        /// <summary>
        /// Groups the elements of a sequence of documents according to a specified metadata key
        /// that contains a sequence of keys.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <param name="documents">The documents.</param>
        /// <param name="keyMetadataKey">The key metadata key.</param>
        /// <returns>A sequence of groups.</returns>
        public static IEnumerable<IGrouping<TKey, IDocument>> GroupByMany<TKey>(
            this IEnumerable<IDocument> documents,
            string keyMetadataKey)
        {
            return documents.GroupByMany<TKey>(keyMetadataKey, null);
        }

        /// <summary>
        /// Groups the elements of a sequence of documents according to a specified metadata key
        /// that contains a sequence of keys
        /// and compares the keys by using a specified comparer.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <param name="documents">The documents.</param>
        /// <param name="keyMetadataKey">The key metadata key.</param>
        /// <param name="comparer">The comparer.</param>
        /// <returns>A sequence of groups.</returns>
        public static IEnumerable<IGrouping<TKey, IDocument>> GroupByMany<TKey>(
            this IEnumerable<IDocument> documents,
            string keyMetadataKey,
            IEqualityComparer<TKey> comparer)
        {
            if (documents == null)
            {
                throw new ArgumentNullException(nameof(documents));
            }
            if (keyMetadataKey == null)
            {
                throw new ArgumentNullException(nameof(keyMetadataKey));
            }

            return documents
                .Distinct()
                .Where(x => x.ContainsKey(keyMetadataKey))
                .GroupByMany(x => x.List<TKey>(keyMetadataKey), comparer);
        }

        /// <summary>
        /// Groups the elements of a sequence of documents according to a specified metadata key
        /// that contains a sequence of keys
        /// and gets the elements for each group by using a specified metadata key.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TElement">The type of the element.</typeparam>
        /// <param name="documents">The documents.</param>
        /// <param name="keyMetadataKey">The key metadata key.</param>
        /// <param name="elementMetadataKey">The element metadata key.</param>
        /// <returns>A sequence of groups.</returns>
        public static IEnumerable<IGrouping<TKey, TElement>> GroupByMany<TKey, TElement>(
            this IEnumerable<IDocument> documents,
            string keyMetadataKey,
            string elementMetadataKey)
        {
            return documents.GroupByMany<TKey, TElement>(keyMetadataKey, elementMetadataKey, null);
        }

        /// <summary>
        /// Groups the elements of a sequence of documents according to a specified metadata key
        /// that contains a sequence of keys.
        /// The keys are compared by using a comparer and each group's elements
        /// are obtained by using a specified metadata key.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TElement">The type of the element.</typeparam>
        /// <param name="documents">The documents.</param>
        /// <param name="keyMetadataKey">The key metadata key.</param>
        /// <param name="elementMetadataKey">The element metadata key.</param>
        /// <param name="comparer">The comparer.</param>
        /// <returns>A sequence of groups.</returns>
        public static IEnumerable<IGrouping<TKey, TElement>> GroupByMany<TKey, TElement>(
            this IEnumerable<IDocument> documents,
            string keyMetadataKey,
            string elementMetadataKey,
            IEqualityComparer<TKey> comparer)
        {
            if (documents == null)
            {
                throw new ArgumentNullException(nameof(documents));
            }
            if (keyMetadataKey == null)
            {
                throw new ArgumentNullException(nameof(keyMetadataKey));
            }
            if (elementMetadataKey == null)
            {
                throw new ArgumentNullException(nameof(elementMetadataKey));
            }

            return documents
                .Distinct()
                .Where(x => x.ContainsKey(keyMetadataKey) && x.ContainsKey(elementMetadataKey))
                .GroupByMany(x => x.List<TKey>(keyMetadataKey), x => x.Get<TElement>(elementMetadataKey), comparer);
        }

        /// <summary>
        /// Groups the elements of a sequence of documents according to a specified metadata key
        /// that contains a sequence of keys
        /// and gets the elements for each group by using a specified metadata key.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TElement">The type of the element.</typeparam>
        /// <param name="documents">The documents.</param>
        /// <param name="keyMetadataKey">The key metadata key.</param>
        /// <param name="elementMetadataKey">The element metadata key.</param>
        /// <returns>A sequence of groups.</returns>
        public static IEnumerable<IGrouping<TKey, TElement>> GroupByManyToMany<TKey, TElement>(
            this IEnumerable<IDocument> documents,
            string keyMetadataKey,
            string elementMetadataKey)
        {
            return documents.GroupByManyToMany<TKey, TElement>(keyMetadataKey, elementMetadataKey, null);
        }

        /// <summary>
        /// Groups the elements of a sequence of documents according to a specified metadata key
        /// that contains a sequence of keys.
        /// The keys are compared by using a comparer and each group's elements
        /// are obtained by using a specified metadata key.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TElement">The type of the element.</typeparam>
        /// <param name="documents">The documents.</param>
        /// <param name="keyMetadataKey">The key metadata key.</param>
        /// <param name="elementMetadataKey">The element metadata key.</param>
        /// <param name="comparer">The comparer.</param>
        /// <returns>A sequence of groups.</returns>
        public static IEnumerable<IGrouping<TKey, TElement>> GroupByManyToMany<TKey, TElement>(
            this IEnumerable<IDocument> documents,
            string keyMetadataKey,
            string elementMetadataKey,
            IEqualityComparer<TKey> comparer)
        {
            if (documents == null)
            {
                throw new ArgumentNullException(nameof(documents));
            }
            if (keyMetadataKey == null)
            {
                throw new ArgumentNullException(nameof(keyMetadataKey));
            }
            if (elementMetadataKey == null)
            {
                throw new ArgumentNullException(nameof(elementMetadataKey));
            }

            return documents
                .Distinct()
                .Where(x => x.ContainsKey(keyMetadataKey) && x.ContainsKey(elementMetadataKey))
                .GroupByManyToMany(x => x.List<TKey>(keyMetadataKey), x => x.List<TElement>(elementMetadataKey), comparer);
        }
    }
}
