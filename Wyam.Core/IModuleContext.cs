using System.Collections.Generic;

namespace Wyam.Core
{
    // A pipeline context is immutable, call .Clone() to get a new context with persisted object and/or new metadata items
    public interface IModuleContext
    {
        IMetadata Metadata { get; }

        // This is a shortcut for getting a metadata value
        object this[string key] { get; }
        
        // Content will never be null (if null is passed in, it'll be converted to string.Empty)
        string Content { get; }
        
        IModuleContext Clone(string content, IEnumerable<KeyValuePair<string, object>> items = null);
        IModuleContext Clone(IEnumerable<KeyValuePair<string, object>> items = null);
    }
}