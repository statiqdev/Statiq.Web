using System;
using System.Collections.Generic;
using System.Linq;

namespace Wyam.Common.Util
{
    /// <summary>
    /// Extension methods for collection types.
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// Adds a range of values to a collection.
        /// </summary>
        /// <typeparam name="T">The type of the collection items.</typeparam>
        /// <param name="collection">The collection to add values to.</param>
        /// <param name="items">The items to add.</param>
        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            foreach (T item in items)
            {
                collection.Add(item);
            }
        }

        /// <summary>
        /// Removes all items that match a predicate from a collection.
        /// </summary>
        /// <typeparam name="T">The type of the collection items.</typeparam>
        /// <param name="collection">The collection to remove items from.</param>
        /// <param name="match">The predicate (return <c>true</c> to remove the item).</param>
        /// <returns>The number of items removed.</returns>
        public static int RemoveAll<T>(this ICollection<T> collection, Func<T, bool> match)
        {
            IList<T> toRemove = collection.Where(match).ToList();
            foreach (T item in toRemove)
            {
                collection.Remove(item);
            }
            return toRemove.Count;
        }

        /// <summary>
        /// Verifies that a dictionary contains all requires keys.
        /// An <see cref="ArgumentException"/> will be thrown if the
        /// specified keys are not all present in the dictionary.
        /// </summary>
        /// <typeparam name="TKey">The type of keys.</typeparam>
        /// <typeparam name="TValue">The type of values.</typeparam>
        /// <param name="dictionary">The dictionary to verify.</param>
        /// <param name="keys">The keys that must be present in the dictionary.</param>
        public static void RequireKeys<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, params TKey[] keys)
        {
            if (!keys.All(x => dictionary.ContainsKey(x)))
            {
                throw new ArgumentException("Dictionary does not contain all required keys");
            }
        }
    }
}
