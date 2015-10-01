using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Common
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

        // Another shortcut method that gets strings, but replaces forward-slashes with back-slashes
        // pretty = trailing /index.html will be trimmed
        string Link(string key, string defaultValue = null, bool pretty = true);
    }

    /// <summary>
    /// Contains the set of metadata converted to type <typeparamref name="T"/>.
    /// The conversion is designed to be flexible and several different methods of type
    /// conversion are tried. Only those values that can be converted to type <typeparamref name="T"/>
    /// are actually included in the dictionary.
    /// </summary>
    /// <typeparam name="T">The type all metadata values should be converted to.</typeparam>
    public interface IMetadata<T> : IReadOnlyDictionary<string, T>
    {
        /// <summary>Gets the value associated with the specified key converted to <typeparamref name="T"/>.</summary>
        /// <param name="key">The key of the value to get.</param>
        /// <returns>The converted value for the specified key or <c>default(T)</c> if not found.</returns>
        T Get(string key);

        /// <summary>Gets the value associated with the specified key converted to <typeparamref name="T"/>.</summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="defaultValue">The default value to use if either the key is not found or the underlying type is not convertible.</param>
        /// <returns>The converted value for the specified key or <paramref name="defaultValue"/> if not found.</returns>
        T Get(string key, T defaultValue);
    }
}
