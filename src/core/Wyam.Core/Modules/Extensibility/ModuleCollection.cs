using System.Collections.Generic;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Modules;

namespace Wyam.Core.Modules.Extensibility
{
    /// <summary>
    /// Executes child modules enabling better reuse.
    /// </summary>
    /// <remarks>
    /// All child modules will be executed as if they were inline. This enables you to specify
    /// a sequence of modules outside of a pipeline and then reuse that sequence of modules
    /// in multiple pipelines. Note that this module is also handy for wrapping a single module
    /// that has a complex configuration if you expect to use it in multiple places.
    /// </remarks>
    /// <example>
    /// <code>
    /// ModuleCollection common = ModuleCollection(ModuleA(), ModuleB(), ModuleC());
    ///
    /// Piplines.Add("A",
    ///     ModuleX(),
    ///     ModuleY(),
    ///     common,
    ///     ModuleZ()
    /// );
    ///
    /// Piplines.Add("B",
    ///     ModuleX(),
    ///     common,
    ///     ModuleZ()
    /// );
    /// </code>
    /// </example>
    /// <category>Extensibility</category>
    public class ModuleCollection : ContainerModule
    {
        /// <summary>
        /// Creates a module collection with the specified child modules.
        /// Any <c>null</c> items in the sequence of modules will be discarded.
        /// </summary>
        /// <param name="modules">The child modules.</param>
        public ModuleCollection(params IModule[] modules)
            : this((IEnumerable<IModule>)modules)
        {
        }

        /// <summary>
        /// Creates a module collection with the specified child modules.
        /// Any <c>null</c> items in the sequence of modules will be discarded.
        /// </summary>
        /// <param name="modules">The child modules.</param>
        public ModuleCollection(IEnumerable<IModule> modules)
            : base(modules)
        {
        }

        /// <inheritdoc />
        public override IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return context.Execute(this, inputs);
        }
    }
}
