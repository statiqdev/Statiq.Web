using System.Collections.Generic;
using System.Linq;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Execution;
using System;

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
        private Func<IExecutionContext, IDocument, IReadOnlyList<IDocument>, bool> _predicate;

        /// <summary>
        /// Executes the specified modules with an empty initial input document.
        /// </summary>
        /// <param name="modules">The modules to execute.</param>
        public Concat(params IModule[] modules)
            : this((IEnumerable<IModule>)modules)
        {
        }

        /// <summary>
        /// Executes the specified modules with an empty initial input document.
        /// </summary>
        /// <param name="modules">The modules to execute.</param>
        public Concat(IEnumerable<IModule> modules)
            : base(modules)
        {
        }

        /// <summary>
        /// Specifies a predicate to use when determining which documents to concatenate with the original list.
        /// </summary>
        /// <param name="predicate">The predicate to evaluate.</param>
        /// <returns>The current module instance.</returns>
        public Concat Where(Func<IExecutionContext, IDocument, IReadOnlyList<IDocument>, bool> predicate)
        {
            Func<IExecutionContext, IDocument, IReadOnlyList<IDocument>, bool> currentPredicate = _predicate;
            _predicate = currentPredicate == null ? predicate : (ctx, doc, orig) => currentPredicate(ctx, doc, orig) && predicate(ctx, doc, orig);
            return this;
        }

        /// <inheritdoc />
        public override IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            IEnumerable<IDocument> docs = context.Execute(this);
            if (_predicate != null)
            {
                docs = docs.Where(x => _predicate(context, x, inputs));
            }
            return inputs.Concat(docs);
        }
    }
}
