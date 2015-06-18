using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Abstractions;

namespace Wyam.Core.Modules
{
    // Executes a sequence of modules against an empty initial document and concatenates their results and the base results
    public class Concat : IModule
    {
        private readonly IModule[] _modules;

        public Concat(params IModule[] modules)
        {
            _modules = modules;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return inputs.Concat(context.Execute(_modules, null));
        }
    }
}
