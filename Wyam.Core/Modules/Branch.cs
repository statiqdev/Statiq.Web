using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common;

namespace Wyam.Core.Modules
{
    // This executes the specified modules and then outputs the input documents without modification
    // In other words, the branch does affect the primary pipeline module flow
    public class Branch : IModule
    {
        private readonly IModule[] _modules;
        private Func<IDocument, IExecutionContext, bool> _predicate;

        public Branch(params IModule[] modules)
        {
            _modules = modules;
        }

        public Branch Where(Func<IDocument, IExecutionContext, bool> predicate)
        {
            _predicate = predicate;
            return this;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            IEnumerable<IDocument> documents = _predicate == null ? inputs : inputs.Where(x => _predicate(x, context));
            context.Execute(_modules, documents);
            return inputs;
        }
    }
}
