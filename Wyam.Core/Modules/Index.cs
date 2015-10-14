using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Pipelines;
using Wyam.Core.Documents;

namespace Wyam.Core.Modules
{
    // Adds metadata with the key "Index" to every document containing the one-based index of the document in the current document collection
    public class Index : IModule
    {
        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return inputs.Select((x, i) => x.Clone(new Dictionary<string, object> {{MetadataKeys.Index, i + 1}}));
        }
    }
}
