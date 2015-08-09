using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Common
{
    public interface IModule
    {
        // This should not be called directly, instead call IExecutionContext.Execute() if you need to execute a module from within another module
        IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context);
    }
}
