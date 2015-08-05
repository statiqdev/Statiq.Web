using System.Collections.Generic;
using System.IO;

namespace Wyam.Abstractions
{
    // A document is immutable, call .Clone() to get a new document with persisted object and/or new metadata items
    // Documents also proxy their metadata and implement the entire IMetadata interface
    // Content and Stream are guaranteed non-null for every document
    public interface IDocument : IMetadata
    {
        string Source { get; }
        IMetadata Metadata { get; }
        string Content { get; }
        Stream Stream { get; }
        IDocument Clone(string source, string content, IEnumerable<KeyValuePair<string, object>> items = null);
        IDocument Clone(string content, IEnumerable<KeyValuePair<string, object>> items = null);
        IDocument Clone(string source, Stream stream, IEnumerable<KeyValuePair<string, object>> items = null);
        IDocument Clone(Stream stream, IEnumerable<KeyValuePair<string, object>> items = null);
        IDocument Clone(IEnumerable<KeyValuePair<string, object>> items = null);
    }
}