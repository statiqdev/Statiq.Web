using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Common
{
    // All methods return distinct sequences of documents
    public interface IDocumentCollection : IEnumerable<IDocument>
    {
        IReadOnlyDictionary<string, IEnumerable<IDocument>> ByPipeline();
        IEnumerable<IDocument> FromPipeline(string pipeline);
        IEnumerable<IDocument> ExceptPipeline(string pipeline);
    }
}
