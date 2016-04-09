using System.Collections.Generic;
using System.Linq;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Execution;

namespace Wyam.Core.Modules.Control
{
    /// <summary>
    /// Takes the first X documents from the current pipeline and discards the rest.
    /// </summary>
    /// <category>Control</category>
    public class Take : IModule
    {
        private readonly int _x;

        /// <summary>
        /// Takes the first X documents from the current pipeline and discards the rest.
        /// </summary>
        /// <param name="x">An integer represeting the number of documents to preserve from the current pipeline.</param>
        public Take(int x)
        {
            _x = x;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return inputs.Take(_x);
        }
    }
}
