using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Pipelines;

namespace Wyam.Core.Modules
{
    /// <summary>
    /// Filters the current sequence of modules using a predicate.
    /// </summary>
    /// <category>Control</category>
    public class Where : IModule
    {
        private readonly DocumentConfig _predicate;

        /// <summary>
        /// Specifies the predicate to use for filtering documents. 
        /// Only input documents for which the predicate returns <c>true</c> will be output.
        /// </summary>
        /// <param name="predicate">A predicate delegate that should return a <c>bool</c>.</param>
        public Where(DocumentConfig predicate)
        {
            _predicate = predicate;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return inputs.Where(x => _predicate.Invoke<bool>(x, context));
        }
    }
}
