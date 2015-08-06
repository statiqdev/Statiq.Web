using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common;

namespace Wyam.Core.Modules
{
    // This allows you to insert previously processed documents into your pipeline
    public class ConcatDocuments : Documents
    {
        public ConcatDocuments(string pipeline = null)
            : base(pipeline)
        {
        }
        
        public override IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return inputs.Concat(base.Execute(inputs, context));
        }
    }
}
