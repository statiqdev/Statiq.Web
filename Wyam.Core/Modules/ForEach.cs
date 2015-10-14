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
    // Executes the input documents one at a time against the specified modules
    // The aggregated results of all child sequences is returned
    public class ForEach : IModule
    {
        private readonly IModule[] _modules;

        public ForEach(params IModule[] modules)
        {
            _modules = modules;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context) 
        {
            return inputs.SelectMany(x => context.Execute(_modules, new[] { x }));
        }
    }
}
