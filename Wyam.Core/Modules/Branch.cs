using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Wyam.Extensibility;

namespace Wyam.Core.Modules
{
    // This executes the specified modules and then outputs the input contexts without modification
    // In other words, the branch does not come back and affect the main module flow
    public class Branch : IModule
    {
        private readonly Func<IModuleContext, bool> _predicate;
        private readonly IModule[] _modules;

        public Branch(params IModule[] modules)
        {
            _modules = modules;
        }

        public Branch(Func<IModuleContext, bool> predicate, params IModule[] modules)
        {
            _predicate = predicate;
            _modules = modules;
        }

        public IEnumerable<IModuleContext> Execute(IReadOnlyList<IModuleContext> inputs, IPipelineContext pipeline)
        {
            IEnumerable<IModuleContext> contexts = _predicate == null ? inputs : inputs.Where(_predicate);
            pipeline.Execute(_modules, contexts);
            return inputs;
        }
    }
}
