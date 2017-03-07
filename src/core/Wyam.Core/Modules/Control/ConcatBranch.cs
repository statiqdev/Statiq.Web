using System;
using System.Collections.Generic;
using System.Linq;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Execution;
using Wyam.Common.Util;

namespace Wyam.Core.Modules.Control
{
    /// <summary>
    /// Executes a sequence of modules against the input documents and concatenates their results and the original input.
    /// This is similar to <see cref="Branch"/> except that the results of the specified modules are concatenated with the
    /// original input documents instead of being forgotten.
    /// </summary>
    /// <category>Control</category>
    public class ConcatBranch : ContainerModule
    {
        private Func<IDocument, IExecutionContext, bool> _predicate;

        /// <summary>
        /// Evaluates the specified modules with each input document as the initial
        /// document and then outputs the original input documents without modification concatenated with the result documents
        /// from the specified modules.
        /// </summary>
        /// <param name="modules">The modules to execute.</param>
        public ConcatBranch(params IModule[] modules)
            : base(modules)
        {
        }

        /// <summary>
        /// Limits the documents passed to the child modules to those that satisfy the
        /// supplied predicate. All original input documents are output without
        /// modification regardless of whether they satisfy the predicate.
        /// </summary>
        /// <param name="predicate">A delegate that should return a <c>bool</c>.</param>
        public ConcatBranch Where(DocumentConfig predicate)
        {
            Func<IDocument, IExecutionContext, bool> currentPredicate = _predicate;
            _predicate = currentPredicate == null
                ? (Func<IDocument, IExecutionContext, bool>)(predicate.Invoke<bool>)
                : ((x, c) => currentPredicate(x, c) && predicate.Invoke<bool>(x, c));
            return this;
        }

        /// <inheritdoc />
        public override IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            IEnumerable<IDocument> documents = _predicate == null
                ? inputs
                : inputs.Where(context, x => _predicate(x, context));
            return inputs.Concat(context.Execute(this, documents));
        }
    }
}
