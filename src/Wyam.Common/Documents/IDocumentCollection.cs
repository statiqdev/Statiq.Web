using System.Collections.Generic;

namespace Wyam.Common.Documents
{
    // All methods return distinct sequences of documents
    public interface IDocumentCollection : IEnumerable<IDocument>
    {
        IReadOnlyDictionary<string, IEnumerable<IDocument>> ByPipeline();
        IEnumerable<IDocument> FromPipeline(string pipeline);
        IEnumerable<IDocument> ExceptPipeline(string pipeline);

        // Same as FromPipeline(string)
        IEnumerable<IDocument> this[string pipline] { get; }
    }
}
