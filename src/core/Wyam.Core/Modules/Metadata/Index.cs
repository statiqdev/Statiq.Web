using System.Collections.Generic;
using System.Linq;
using Wyam.Common.Documents;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Execution;
using Wyam.Core.Documents;
using Wyam.Core.Meta;

namespace Wyam.Core.Modules.Metadata
{
    /// <summary>
    /// Adds a one-based index to every document as metadata.
    /// </summary>
    /// <metadata cref="Keys.Index" usage="Output" />
    /// <category>Metadata</category>
    public class Index : IModule
    {
        /// <inheritdoc />
        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return inputs.Select((x, i) => context.GetDocument(x, new MetadataItems { { Keys.Index, i + 1} }));
        }
    }
}
