using System.Collections.Generic;
using System.Linq;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Pipelines;
using Wyam.Core.Documents;

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
            return inputs.Select((x, i) => x.Clone(new Dictionary<string, object> {{MetadataKeys.Index, i + 1}}));
        }
    }
}
