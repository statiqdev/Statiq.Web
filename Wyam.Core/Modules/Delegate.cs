using System;
using System.Collections.Generic;
using System.Linq;
using Wyam.Core;

namespace Wyam.Core.Modules
{
    // Executes the specified delegate for each input
    public class Delegate : IModule
    {
        private readonly Func<IModuleContext, IEnumerable<IModuleContext>> _execute;

        public Delegate(Func<IModuleContext, IEnumerable<IModuleContext>> execute)
        {
            _execute = execute;
        }

        public IEnumerable<IModuleContext> Execute(IEnumerable<IModuleContext> inputs, IPipelineContext pipeline)
        {
            return inputs.SelectMany(x => _execute(x));
        }
    }
}