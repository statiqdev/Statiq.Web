using System;
using System.Collections.Generic;
using System.Linq;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Pipelines;

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
        private readonly DocumentConfig _key;
        private bool _descending;
        private readonly List<ThenByEntry> _thenByList = new List<ThenByEntry>();

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
            _key = key;
        }

        /// <summary>
        /// Specifies whether the documents should be output in descending order (the default is ascending order).
        /// If you use this method after called ThenBy, the descending ordering will apply to the secondary sort.
        /// </summary>
        /// <param name="descending">If set to <c>true</c>, the documents are output in descending order.</param>
        /// <returns></returns>
        public OrderBy Descending(bool descending = true)
        {
            if (_thenByList.Count == 0)
                _descending = descending;
            else
                _thenByList.Last().Descending = descending;
            return this;
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
            _thenByList.Add(new ThenByEntry(key));
            return this;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            var orderdList = _descending
                ? inputs.OrderByDescending(x => _key(x, context))
                : inputs.OrderBy(x => _key(x, context));
            foreach (var thenBy in _thenByList)
            {
                orderdList = thenBy.Descending
                    ? orderdList.ThenByDescending(x => thenBy.Key(x, context))
                    : orderdList.ThenBy(x => thenBy.Key(x, context));
            }

            return orderdList;
        }

        private class ThenByEntry
        {

            public ThenByEntry(DocumentConfig key)
            {
                this.Key = key;
            }

            public DocumentConfig Key { get; }
            public bool Descending { get; set; }
        }
    }
}
