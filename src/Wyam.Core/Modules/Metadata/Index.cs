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
    /// <metadata name="Index" type="int">The one-based index of the current document relative to other documents in the pipeline.</metadata>
    /// <category>Metadata</category>
    public class Index : IModule
    {
        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return inputs.Select((x, i) => context.GetDocument(x, new MetadataItems {{Keys.Index, i + 1}}));
        }
    }
}
