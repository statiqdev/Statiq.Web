using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Abstractions
{
    public interface IModule
    {
        // This should not be called directly, instead call IPipelineContext.Execute() if you need to execute a module from within another module
        IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IPipelineContext pipeline);
    }
}
