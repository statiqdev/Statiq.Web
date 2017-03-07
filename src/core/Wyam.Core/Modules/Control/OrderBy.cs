using System;
using System.Collections.Generic;
using System.Linq;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Execution;
using Wyam.Core.Util;

namespace Wyam.Core.Modules.Control
{
    /// <summary>
    /// Orders the input documents based on the specified key function.
    /// </summary>
    /// <remarks>
    /// The ordered documents are output as the result of this module.
    /// </remarks>
    /// <category>Control</category>
    public class OrderBy : IModule
    {
        private readonly Stack<Order> _orders = new Stack<Order>();

        /// <summary>
        /// Orders the input documents using the specified delegate to get the ordering key.
        /// </summary>
        /// <param name="key">A delegate that should return the key to use for ordering.</param>
        public OrderBy(DocumentConfig key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            _orders.Push(new Order(key));
        }

        /// <summary>
        /// Orders the input documents using the specified delegate to get a secondary ordering key.
        /// You can chain as many ThenBy calls together as needed.
        /// </summary>
        /// <param name="key">A delegate that should return the key to use for ordering.</param>
        public OrderBy ThenBy(DocumentConfig key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            _orders.Push(new Order(key));
            return this;
        }

        /// <summary>
        /// Specifies whether the documents should be output in descending order (the default is ascending order).
        /// If you use this method after called ThenBy, the descending ordering will apply to the secondary sort.
        /// </summary>
        /// <param name="descending">If set to <c>true</c>, the documents are output in descending order.</param>
        /// <returns></returns>
        public OrderBy Descending(bool descending = true)
        {
            _orders.Peek().Descending = descending;
            return this;
        }

        /// <summary>
        /// Specifies a comparer to use for the ordering.
        /// </summary>
        /// <param name="comparer">The comparer to use.</param>
        public OrderBy WithComparer(IComparer<object> comparer)
        {
            _orders.Peek().Comparer = comparer;
            return this;
        }

        /// <summary>
        /// Specifies a typed comparer to use for the ordering. A conversion to the
        /// comparer type will be attempted for all metadata values. If the conversion fails,
        /// the values will be considered equivalent. Note that this will also have the effect
        /// of treating different convertible types as being of the same type. For example,
        /// if you have two keys, 1 and "1", and use a string-based comparison, the
        /// documents will compare as equal.
        /// </summary>
        /// <param name="comparer">The typed comparer to use.</param>
        public OrderBy WithComparer<TValue>(IComparer<TValue> comparer)
        {
            _orders.Peek().Comparer = comparer == null ? null : new ConvertingComparer<TValue>(comparer);
            return this;
        }

        /// <inheritdoc />
        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            IOrderedEnumerable<IDocument> orderdList = null;
            foreach (Order order in _orders.Reverse())
            {
                if (orderdList == null)
                {
                    orderdList = order.Descending
                       ? inputs.OrderByDescending(x => order.Key(x, context), order.Comparer)
                       : inputs.OrderBy(x => order.Key(x, context), order.Comparer);
                }
                else
                {
                    orderdList = order.Descending
                        ? orderdList.ThenByDescending(x => order.Key(x, context), order.Comparer)
                        : orderdList.ThenBy(x => order.Key(x, context), order.Comparer);
                }
            }

            return orderdList;
        }

        private class Order
        {
            public DocumentConfig Key { get; }
            public bool Descending { get; set; }
            public IComparer<object> Comparer { get; set; }

            public Order(DocumentConfig key)
            {
                Key = key;
            }
        }
    }
}
