using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Pipelines;

namespace Wyam.Core.Modules
{
    // Executes a sequence of modules against the input and concatenates their results and the original input
    public class Fork : IModule
    {
        private readonly IModule[] _modules;

        public Fork(params IModule[] modules)
        {
            _modules = modules;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return inputs.Concat(context.Execute(_modules,inputs));
        }
    }
}
