using System.Collections.Generic;

namespace Wyam.Core
{
    // A pipeline context is immutable, call .Clone() to get a new context with persisted object and/or new metadata items
    public interface IModuleContext
    {
        IMetadata Metadata { get; }
        string Content { get; }
        IModuleContext Clone(string content, IEnumerable<KeyValuePair<string, object>> items = null);
        IModuleContext Clone(IEnumerable<KeyValuePair<string, object>> items = null);
    }
}