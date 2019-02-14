using System;
using System.Collections.Generic;
using Wyam.Common.Documents;
using Wyam.Common.IO;

namespace Wyam.Common.Meta
{
    /// <summary>
    /// Contains a set of metadata with flexible runtime conversion methods. Metadata keys are case-insensitive.
    /// </summary>
    public interface IMetadata : IReadOnlyDictionary<string, object>
    {
        /// <summary>
        /// Presents metadata values as a specific type (see <see cref="IMetadata"/>).
        /// </summary>
        /// <typeparam name="T">The type metadata values should be converted to.</typeparam>
        /// <returns>A strongly-typed <see cref="IMetadata"/> object that returns values converted to type T.</returns>
        IMetadata<T> MetadataAs<T>();

        /// <summary>
        /// Gets the value for the specified key. This method never throws an exception. It will return the specified
        /// default value or null if the key is not found.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="defaultValue">The default value to use if the key is not found.</param>
        /// <returns>The value for the specified key or the specified default value.</returns>
        object Get(string key, object defaultValue = null);

        /// <summary>
        /// Gets the raw value for the specified key. This method will not materialize <see cref="IMetadataValue"/>
        /// values the way <see cref="Get(string, object)"/> will. A <see cref="KeyNotFoundException"/> will be thrown
        /// for missing keys.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <returns>The raw value for the specified ke.y</returns>
        object GetRaw(string key);

        /// <summary>
        /// Gets the value for the specified key converted to the specified type.
        /// This method never throws an exception. It will return default(T) if the key is not found
        /// or the value cannot be converted to T.
        /// </summary>
        /// <typeparam name="T">The desired return type.</typeparam>
        /// <param name="key">The key of the value to get.</param>
        /// <returns>The value for the specified key converted to type T or default(T) if the key is not found or cannot be converted to type T.</returns>
        T Get<T>(string key);

        /// <summary>
        /// Gets the value for the specified key. This method never throws an exception. It will return the specified
        /// default value if the key is not found.
        /// </summary>
        /// <typeparam name="T">The desired return type.</typeparam>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="defaultValue">The default value to use if the key is not found or cannot be converted to type T.</param>
        /// <returns>The value for the specified key converted to type T or the specified default value.</returns>
        T Get<T>(string key, T defaultValue);

        /// <summary>
        /// Tries to get the value for the specified key.
        /// </summary>
        /// <typeparam name="T">The desired return type.</typeparam>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">The value of the key if it was found and could be converted to the desired return type.</param>
        /// <returns><c>true</c> if the key was found and the value could be converted to the desired return type, <c>false</c> otherwise.</returns>
        bool TryGetValue<T>(string key, out T value);

        /// <summary>
        /// Gets a new <see cref="IMetadata"/> containing only the specified keys and their values. If a key is not present in the current
        /// metadata, it will be ignored and will not be copied to the new metadata object.
        /// </summary>
        /// <param name="keys">The keys to include in the new metadata object.</param>
        /// <returns>A new <see cref="IMetadata"/> containing the specified keys and their values.</returns>
        IMetadata GetMetadata(params string[] keys);
    }

    /// <summary>
    /// Contains a set of metadata converted to type <typeparamref name="T"/>.
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
