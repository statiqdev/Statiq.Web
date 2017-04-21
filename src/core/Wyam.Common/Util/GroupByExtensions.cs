using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Common.Util
{
    /// <summary>
    /// Extensions for performing grouping operations.
    /// </summary>
    public static class GroupByExtensions
    {
        /// <summary>
        /// Groups the elements of a sequence according to a specified key selector function
        /// that returns a sequence of keys.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="keySelector">The key selector.</param>
        /// <returns>A sequence of groups.</returns>
        public static IEnumerable<IGrouping<TKey, TSource>> GroupByMany<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, IEnumerable<TKey>> keySelector)
        {
            return source.GroupByMany(keySelector, x => x, null);
        }

        /// <summary>
        /// Groups the elements of a sequence according to a specified key selector function
        /// that returns a sequence of keys
        /// and compares the keys by using a specified comparer.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="keySelector">The key selector.</param>
        /// <param name="comparer">The comparer.</param>
        /// <returns>A sequence of groups.</returns>
        public static IEnumerable<IGrouping<TKey, TSource>> GroupByMany<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, IEnumerable<TKey>> keySelector,
            IEqualityComparer<TKey> comparer)
        {
            return source.GroupByMany(keySelector, x => x, comparer);
        }

        /// <summary>
        /// Groups the elements of a sequence according to a specified key selector function
        /// that returns a sequence of keys
        /// and projects the elements for each group by using a specified function.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TElement">The type of the element.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="keySelector">The key selector.</param>
        /// <param name="elementSelector">The element selector.</param>
        /// <returns>A sequence of groups.</returns>
        public static IEnumerable<IGrouping<TKey, TElement>> GroupByMany<TSource, TKey, TElement>(
            this IEnumerable<TSource> source,
            Func<TSource, IEnumerable<TKey>> keySelector,
            Func<TSource, TElement> elementSelector)
        {
            return source.GroupByMany(keySelector, elementSelector, null);
        }

        /// <summary>
        /// Groups the elements of a sequence according to a specified key selector function
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
        /// <returns>A sequence of groups.</returns>
        public static IEnumerable<IGrouping<TKey, TElement>> GroupByMany<TSource, TKey, TElement>(
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
                .Select(x => new
                {
                    Keys = keySelector(x),
                    Element = elementSelector(x)
                })
                .Where(x => x.Keys != null)
                .SelectMany(x => x.Keys.Select(key => new
                {
                    Key = key,
                    x.Element
                }))
                .GroupBy(x => x.Key, x => x.Element, comparer);
        }

        /// <summary>
        /// Groups the elements of a sequence according to a specified key selector function
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
        /// <returns>A sequence of groups.</returns>
        public static IEnumerable<IGrouping<TKey, TElement>> GroupByManyToMany<TSource, TKey, TElement>(
            this IEnumerable<TSource> source,
            Func<TSource, IEnumerable<TKey>> keySelector,
            Func<TSource, IEnumerable<TElement>> elementSelector)
        {
            return source.GroupByManyToMany(keySelector, elementSelector, null);
        }

        /// <summary>
        /// Groups the elements of a sequence according to a specified key selector function
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
        /// <returns>A sequence of groups.</returns>
        public static IEnumerable<IGrouping<TKey, TElement>> GroupByManyToMany<TSource, TKey, TElement>(
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
                .Select(x => new
                {
                    Keys = keySelector(x),
                    Elements = elementSelector(x)
                })
                .Where(x => x.Keys != null && x.Elements != null)
                .SelectMany(x => x.Keys
                    .SelectMany(key => x.Elements.Select(elem => new
                    {
                        Key = key,
                        Element = elem
                    })))
                .GroupBy(x => x.Key, x => x.Element, comparer);
        }
    }
}
