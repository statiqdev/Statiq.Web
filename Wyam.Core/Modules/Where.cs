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
        private readonly Func<IDocument, bool> _predicate;

        public Where(Func<IDocument, bool> predicate)
        {
            _predicate = predicate;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return inputs.Where(_predicate);
        }
    }
}
