using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Core
{
    public interface IPipelineContext
    {
        Trace Trace { get; }
        IReadOnlyList<IModuleContext> CompletedContexts { get; }

        // This executes the specified modules on the specified input contexts and returns the final result contexts
        IReadOnlyList<IModuleContext> Execute(IEnumerable<IModule> modules, IEnumerable<IModuleContext> inputContexts);
    }
}
