using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.Execution;

namespace Wyam.Common.Modules
{
    /// <summary>
    /// Wraps a module and gives it a name for use with <see cref="ModuleList"/>.
    /// </summary>
    /// <category>Extensibility</category>
    public class NamedModule : IModule
    {
        /// <summary>
        /// The name of the module.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The wrapped module.
        /// </summary>
        public IModule Module { get; }

        /// <summary>
        /// Wraps a module and gives it the specified name.
        /// </summary>
        /// <param name="name">The name of the module.</param>
        /// <param name="module">The wrapped module.</param>
        public NamedModule(string name, IModule module)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Module = module ?? throw new ArgumentNullException(nameof(module));
        }

        /// <inheritdoc />
        /// <summary>
        /// Passes execution to the wrapped module.
        /// </summary>
        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context) => Module.Execute(inputs, context);
    }
}
