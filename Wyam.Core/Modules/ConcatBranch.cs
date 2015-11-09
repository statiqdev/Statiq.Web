using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Pipelines;

namespace Wyam.Core.Modules
{
    // Executes a sequence of modules against the input and concatenates their results and the original input
    public class ConcatBranch : IModule
    {
        private readonly IModule[] _modules;
        private Func<IDocument, IExecutionContext, bool> _predicate;

        public ConcatBranch(params IModule[] modules)
        {
            _modules = modules;
        }

        // The delegate should return a bool
        public ConcatBranch Where(DocumentConfig predicate)
        {
            Func<IDocument, IExecutionContext, bool> currentPredicate = _predicate;
            _predicate = currentPredicate == null
                ? (Func<IDocument, IExecutionContext, bool>)(predicate.Invoke<bool>)
                : ((x, c) => currentPredicate(x, c) && predicate.Invoke<bool>(x, c));
            return this;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            IEnumerable<IDocument> documents = _predicate == null ? inputs : inputs.Where(x => _predicate(x, context));
            return inputs.Concat(context.Execute(_modules, documents));
        }
    }
}
