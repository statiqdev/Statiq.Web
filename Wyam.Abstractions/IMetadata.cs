using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Abstractions
{
    public interface IMetadata : IReadOnlyDictionary<string, object>
    {
        // Returns a strongly-typed metadata that returns values converted to T
        IMetadata<T> MetadataAs<T>();
        
        // These methods never throw, they return the specified default value or default(T) if the key is not found
        object Get(string key, object defaultValue = null);
        T Get<T>(string key);
        T Get<T>(string key, T defaultValue);

        // This method doesn't throw, equivalent to Get<string>(key, defaultValue)
        string String(string key, string defaultValue = null);
    }

    public interface IMetadata<T> : IReadOnlyDictionary<string, T>
    {
        // This returns default(T) if the key is not found
        T Get(string key);
        T Get(string key, T defaultValue);
    }
}
