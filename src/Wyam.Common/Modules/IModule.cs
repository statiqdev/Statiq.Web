using System.Collections.Generic;
using Wyam.Common.Documents;
using Wyam.Common.Execution;

namespace Wyam.Common.Modules
{
    public interface IModule
    {
        // This should not be called directly, instead call IExecutionContext.Execute() if you need to execute a module from within another module
        IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context);
    }
}
