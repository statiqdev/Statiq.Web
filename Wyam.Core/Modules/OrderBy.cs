using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common;

namespace Wyam.Core.Modules
{
    public class OrderBy<TKey> : IModule
    {
        private readonly Func<IDocument, IExecutionContext, TKey> _orderFunc;
        private bool _descending;

        public OrderBy(Func<IDocument, IExecutionContext, TKey> orderFunc)
        {
            if (orderFunc == null)
            {
                throw new ArgumentNullException(nameof(orderFunc));
            }
            _orderFunc = orderFunc;
        }

        public OrderBy<TKey> Descending(bool descending = true)
        {
            _descending = descending;
            return this;
        } 

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return _descending 
                ? inputs.OrderByDescending(x => _orderFunc(x, context)) 
                : inputs.OrderBy(x => _orderFunc(x, context));
        }
    }
}
