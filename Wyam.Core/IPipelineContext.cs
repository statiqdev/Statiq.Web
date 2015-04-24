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
        IReadOnlyList<IModuleContext> Execute(IModule module, IEnumerable<IModuleContext> contexts);
    }
}
