using System;
using System.Collections.Generic;
using System.Linq;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Modules;

namespace Wyam.Core.Modules.Control
{
    /// <summary>
    /// Sorts the input documents based on the specified comparison delegate.
    /// </summary>
    /// <remarks>
    /// The sorted documents are output as the result of this module. This is similar
    /// to the <see cref="OrderBy"/> module but gives greater control over the sorting
    /// process.
    /// </remarks>
    /// <category>Control</category>
    public class Sort : IModule
    {
        private readonly Comparison<IDocument> _sort;

        /// <summary>
        /// Creates a sort module.
        /// </summary>
        /// <param name="sort">The sorting delegate to use.</param>
        public Sort(Comparison<IDocument> sort)
        {
            _sort = sort ?? throw new ArgumentNullException(nameof(sort));
        }

        /// <inheritdoc />
        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            IDocument[] inputArray = inputs.ToArray();
            Array.Sort(inputArray, _sort);
            return inputArray;
        }
    }
}