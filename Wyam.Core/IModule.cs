using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Core
{
    public interface IModule
    {
        IEnumerable<IModuleContext> Execute(IEnumerable<IModuleContext> inputs, IPipelineContext pipeline);
    }
}
