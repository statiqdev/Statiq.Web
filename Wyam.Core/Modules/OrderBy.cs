using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common;

namespace Wyam.Core.Modules
{
    public class OrderBy : IModule
    {
        private readonly DocumentConfig _key;
        private bool _descending;

        public OrderBy(DocumentConfig key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            _key = key;
        }

        public OrderBy Descending(bool descending = true)
        {
            _descending = descending;
            return this;
        } 

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return _descending 
                ? inputs.OrderByDescending(x => _key(x, context)) 
                : inputs.OrderBy(x => _key(x, context));
        }
    }
}
