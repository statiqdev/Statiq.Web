using System.Collections.Generic;
using System.Linq;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Execution;

namespace Wyam.Core.Modules.Control
{
    /// <summary>
    /// Executes modules and concatenates their output with the input documents.
    /// </summary>
    /// <remarks>
    /// The specified modules are executed with an empty initial document and then 
    /// outputs the original input documents without modification concatenated with the 
    /// results from the specified module sequence.
    /// </remarks>
    /// <category>Control</category>
    public class Concat : ContainerModule
    {
        /// <summary>
        /// Executes the specified modules with an empty initial input document.
        /// </summary>
        /// <param name="modules">The modules to execute.</param>
        public Concat(params IModule[] modules)
            : base(modules)
        {
        }

        public override IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return inputs.Concat(context.Execute(this));
        }
    }
}
