using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Common.Util
{
    public static class ToLookupExtensions
    {
        /// <summary>
        /// Creates a lookup from a sequence according to a specified key selector function 
        /// that returns a sequence of keys.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="keySelector">The key selector.</param>
        /// <returns>A lookup.</returns>
        public static ILookup<TKey, TSource> ToLookupMany<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, IEnumerable<TKey>> keySelector)
        {
            return source.ToLookupMany(keySelector, x => x, null);
        }

        /// <summary>
        /// Creates a lookup from a sequence according to a specified key selector function 
        /// that returns a sequence of keys
        /// and compares the keys by using a specified comparer.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="keySelector">The key selector.</param>
        /// <param name="comparer">The comparer.</param>
        /// <returns>A lookup.</returns>
        public static ILookup<TKey, TSource> ToLookupMany<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, IEnumerable<TKey>> keySelector,
            IEqualityComparer<TKey> comparer)
        {
            return source.ToLookupMany(keySelector, x => x, comparer);
        }

        /// <summary>
        /// Creates a lookup from a sequence according to a specified key selector function 
        /// that returns a sequence of keys
        /// and projects the elements for each group by using a specified function.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TElement">The type of the element.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="keySelector">The key selector.</param>
        /// <param name="elementSelector">The element selector.</param>
        /// <returns>A lookup.</returns>
        public static ILookup<TKey, TElement> ToLookupMany<TSource, TKey, TElement>(
            this IEnumerable<TSource> source,
            Func<TSource, IEnumerable<TKey>> keySelector,
            Func<TSource, TElement> elementSelector)
        {
            return source.ToLookupMany(keySelector, elementSelector, null);
        }

        /// <summary>
        /// Creates a lookup from a sequence according to a specified key selector function
        /// that returns a sequence of keys.
        /// The keys are compared by using a comparer and each group's elements 
        /// are projected by using a specified function.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TElement">The type of the element.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="keySelector">The key selector.</param>
        /// <param name="elementSelector">The element selector.</param>
        /// <param name="comparer">The comparer.</param>
        /// <returns>A lookup.</returns>
        public static ILookup<TKey, TElement> ToLookupMany<TSource, TKey, TElement>(
            this IEnumerable<TSource> source,
            Func<TSource, IEnumerable<TKey>> keySelector,
            Func<TSource, TElement> elementSelector,
            IEqualityComparer<TKey> comparer)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (keySelector == null)
            {
                throw new ArgumentNullException(nameof(keySelector));
            }
            if (elementSelector == null)
            {
                throw new ArgumentNullException(nameof(elementSelector));
            }

            return source
                .SelectMany(x => keySelector(x)
                    .Select(key => Tuple.Create(key, elementSelector(x))))
                .ToLookup(x => x.Item1, x => x.Item2);
        }

        /// <summary>
        /// Creates a lookup from a sequence according to a specified key selector function 
        /// that returns a sequence of keys
        /// and projects the elements for each group by using a specified function
        /// that returns a sequence of elements.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TElement">The type of the element.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="keySelector">The key selector.</param>
        /// <param name="elementSelector">The element selector.</param>
        /// <returns>A lookup.</returns>
        public static ILookup<TKey, TElement> ToLookupManyToMany<TSource, TKey, TElement>(
            this IEnumerable<TSource> source,
            Func<TSource, IEnumerable<TKey>> keySelector,
            Func<TSource, IEnumerable<TElement>> elementSelector)
        {
            return source.ToLookupManyToMany(keySelector, elementSelector, null);
        }

        /// <summary>
        /// Creates a lookup from a sequence according to a specified key selector function
        /// that returns a sequence of keys.
        /// The keys are compared by using a comparer and each group's elements 
        /// are projected by using a specified function
        /// that returns a sequence of elements.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TElement">The type of the element.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="keySelector">The key selector.</param>
        /// <param name="elementSelector">The element selector.</param>
        /// <param name="comparer">The comparer.</param>
        /// <returns>A lookup.</returns>
        public static ILookup<TKey, TElement> ToLookupManyToMany<TSource, TKey, TElement>(
            this IEnumerable<TSource> source,
            Func<TSource, IEnumerable<TKey>> keySelector,
            Func<TSource, IEnumerable<TElement>> elementSelector,
            IEqualityComparer<TKey> comparer)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (keySelector == null)
            {
                throw new ArgumentNullException(nameof(keySelector));
            }
            if (elementSelector == null)
            {
                throw new ArgumentNullException(nameof(elementSelector));
            }

            return source
                .SelectMany(x => keySelector(x)
                    .SelectMany(key => elementSelector(x).Select(elem => Tuple.Create(key, elem))))
                .ToLookup(x => x.Item1, x => x.Item2, comparer);
        }
    }
}
