using System.Collections.Generic;

namespace Wyam.Abstractions
{
    // A document is immutable, call .Clone() to get a new document with persisted object and/or new metadata items
    public interface IDocument
    {
        IMetadata Metadata { get; }

        // Shortcuts for getting metadata values
        object this[string key] { get; }
        object Get(string key, object defaultValue = null);
        T Get<T>(string key);
        T Get<T>(string key, T defaultValue);
        
        // Content will never be null (if null is passed in, it'll be converted to string.Empty)
        string Content { get; }
        
        IDocument Clone(string content, IEnumerable<KeyValuePair<string, object>> items = null);
        IDocument Clone(IEnumerable<KeyValuePair<string, object>> items = null);
    }
}