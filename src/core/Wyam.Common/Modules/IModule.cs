using System.Collections.Generic;
using Wyam.Common.Documents;
using Wyam.Common.Execution;

namespace Wyam.Common.Modules
{
    public interface IModule
    {
        /// <summary>
        /// This should not be called directly, instead call <c>IExecutionContext.Execute()</c> if you need to execute a module from within another module.
        /// </summary>
        IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context);
    }
}
