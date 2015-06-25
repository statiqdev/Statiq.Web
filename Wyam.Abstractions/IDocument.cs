using System.Collections.Generic;

namespace Wyam.Abstractions
{
    // A document is immutable, call .Clone() to get a new document with persisted object and/or new metadata items
    // Documents also proxy their metadata and implement the entire IMetadata interface
    public interface IDocument : IMetadata
    {
        IMetadata Metadata { get; }
        string Content { get; }  // Content will never be null (if null is passed in, it'll be converted to string.Empty)
        IDocument Clone(string content, IEnumerable<KeyValuePair<string, object>> items = null);
        IDocument Clone(IEnumerable<KeyValuePair<string, object>> items = null);
    }
}