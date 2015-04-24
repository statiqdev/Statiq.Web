using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Core.Modules
{
    // This executes the specified modules and then outputs the input contexts without modification
    public class Branch : IModule
    {
        private readonly IModule[] _modules;

        public Branch(params IModule[] modules)
        {
            _modules = modules;
        }

        public IEnumerable<IModuleContext> Execute(IReadOnlyList<IModuleContext> inputs, IPipelineContext pipeline)
        {
            IReadOnlyList<IModuleContext> contexts = inputs;
            foreach (IModule module in _modules)
            {
                contexts = pipeline.Execute(module, contexts);
            }
            return inputs;
        }
    }
}
