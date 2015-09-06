using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common;

namespace Wyam.Core.Modules
{
    // This filters the documents
    public class Where : IModule
    {
        private readonly DocumentConfig _predicate;

        // The delegate should return a bool
        public Where(DocumentConfig predicate)
        {
            _predicate = predicate;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return inputs.Where(x => _predicate.Invoke<bool>(x, context));
        }
    }
}
