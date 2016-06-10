using System;
using System.Collections.Generic;
using System.Linq;
using Wyam.Common.Util;

namespace Wyam.Common.Documents
{
    public static class ToLookupExtensions
    {
        /// <summary>
        /// Creates a lookup from a sequence of documents using the values of a specified metadata key. 
        /// If a document does not contain the specified metadata key, it is not included in the result set.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <param name="documents">The documents.</param>
        /// <param name="keyMetadataKey">The key metadata key.</param>
        /// <returns>A lookup.</returns>
        public static ILookup<TKey, IDocument> ToLookup<TKey>(
            this IEnumerable<IDocument> documents, 
            string keyMetadataKey)
        {
            return documents.ToLookup<TKey>(keyMetadataKey, null);
        }

        /// <summary>
        /// Creates a lookup from a sequence of documents and the values of a specified metadata key 
        /// and compares the keys by using a specified comparer.
        /// If a document does not contain the specified metadata key, it is not included in the result set.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <param name="documents">The documents.</param>
        /// <param name="keyMetadataKey">The key metadata key.</param>
        /// <param name="comparer">The comparer.</param>
        /// <returns>A lookup.</returns>
        public static ILookup<TKey, IDocument> ToLookup<TKey>(
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
                .ToLookup(x => x.Get<TKey>(keyMetadataKey), comparer);
        }

        /// <summary>
        /// Creates a lookup from a sequence of documents and the values of a specified metadata key
        /// using the value of the specified element metadata for the elements of the lookup.
        /// If a document does not contain the specified key or element metadata keys, it is not included in the result set.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TElement">The type of the element.</typeparam>
        /// <param name="documents">The documents.</param>
        /// <param name="keyMetadataKey">The key metadata key.</param>
        /// <param name="elementMetadataKey">The element metadata key.</param>
        /// <returns>A lookup.</returns>
        public static ILookup<TKey, TElement> ToLookup<TKey, TElement>(
            this IEnumerable<IDocument> documents,
            string keyMetadataKey, 
            string elementMetadataKey)
        {
            return documents.ToLookup<TKey, TElement>(keyMetadataKey, elementMetadataKey, null);
        }

        /// <summary>
        /// Creates a lookup from a sequence of documents and the values of a specified metadata key
        /// using the value of the specified element metadata for the elements of the lookup
        /// and compares the keys by using a specified comparer.
        /// If a document does not contain the specified key or element metadata keys, it is not included in the result set.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TElement">The type of the element.</typeparam>
        /// <param name="documents">The documents.</param>
        /// <param name="keyMetadataKey">The key metadata key.</param>
        /// <param name="elementMetadataKey">The element metadata key.</param>
        /// <param name="comparer">The comparer.</param>
        /// <returns>A lookup.</returns>
        public static ILookup<TKey, TElement> ToLookup<TKey, TElement>(
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
                .ToLookup(x => x.Get<TKey>(keyMetadataKey), x => x.Get<TElement>(elementMetadataKey), comparer);
        }

        /// <summary>
        /// Creates a lookup from a sequence of documents according to a specified metadata key 
        /// that contains a sequence of keys.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <param name="documents">The documents.</param>
        /// <param name="keyMetadataKey">The key metadata key.</param>
        /// <returns>A lookup.</returns>
        public static ILookup<TKey, IDocument> ToLookupMany<TKey>(
            this IEnumerable<IDocument> documents,
            string keyMetadataKey)
        {
            return documents.ToLookupMany<TKey>(keyMetadataKey, null);
        }

        /// <summary>
        /// Creates a lookup from a sequence of documents according to a specified metadata key 
        /// that contains a sequence of keys
        /// and compares the keys by using a specified comparer.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <param name="documents">The documents.</param>
        /// <param name="keyMetadataKey">The key metadata key.</param>
        /// <param name="comparer">The comparer.</param>
        /// <returns>A lookup.</returns>
        public static ILookup<TKey, IDocument> ToLookupMany<TKey>(
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
                .ToLookupMany(x => x.List<TKey>(keyMetadataKey), comparer);
        }

        /// <summary>
        /// Creates a lookup from a sequence of documents according to a specified metadata key
        /// that contains a sequence of keys
        /// and gets the elements for each group by using a specified metadata key.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TElement">The type of the element.</typeparam>
        /// <param name="documents">The documents.</param>
        /// <param name="keyMetadataKey">The key metadata key.</param>
        /// <param name="elementMetadataKey">The element metadata key.</param>
        /// <returns>A lookup.</returns>
        public static ILookup<TKey, TElement> ToLookupMany<TKey, TElement>(
            this IEnumerable<IDocument> documents,
            string keyMetadataKey, 
            string elementMetadataKey)
        {
            return documents.ToLookupMany<TKey, TElement>(keyMetadataKey, elementMetadataKey, null);
        }

        /// <summary>
        /// Creates a lookup from a sequence of documents according to a specified metadata key
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
        /// <returns>A lookup.</returns>
        public static ILookup<TKey, TElement> ToLookupMany<TKey, TElement>(
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
                .ToLookupMany(x => x.List<TKey>(keyMetadataKey), x => x.Get<TElement>(elementMetadataKey), comparer);
        }

        /// <summary>
        /// Creates a lookup from a sequence of documents according to a specified metadata key
        /// that contains a sequence of keys
        /// and gets the elements for each group by using a specified metadata key.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TElement">The type of the element.</typeparam>
        /// <param name="documents">The documents.</param>
        /// <param name="keyMetadataKey">The key metadata key.</param>
        /// <param name="elementMetadataKey">The element metadata key.</param>
        /// <returns>A lookup.</returns>
        public static ILookup<TKey, TElement> ToLookupManyToMany<TKey, TElement>(
            this IEnumerable<IDocument> documents,
            string keyMetadataKey, 
            string elementMetadataKey)
        {
            return documents.ToLookupManyToMany<TKey, TElement>(keyMetadataKey, elementMetadataKey, null);
        }

        /// <summary>
        /// Creates a lookup from a sequence of documents according to a specified metadata key
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
        /// <returns>A lookup.</returns>
        public static ILookup<TKey, TElement> ToLookupManyToMany<TKey, TElement>(
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
                .ToLookupManyToMany(x => x.List<TKey>(keyMetadataKey), x => x.List<TElement>(elementMetadataKey), comparer);
        }
    }
}
