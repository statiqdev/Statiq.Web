using System.Collections.Generic;
using Microsoft.SqlServer.Server;
using Wyam.Common.Documents;
using Wyam.Common.IO;

namespace Wyam.Common.Meta
{
    /// <summary>
    /// Contains a set of metadata with flexible runtime conversion methods.
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
        /// Gets the value for the specified key converted to the specified type. 
        /// This method never throws an exception. It will return default(T) if the key is not found
        /// or the value cannot be converted to T.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <returns>The value for the specified key converted to type T or default(T) if the key is not found or cannot be converted to type T.</returns>
        T Get<T>(string key);

        /// <summary>
        /// Gets the value for the specified key. This method never throws an exception. It will return the specified 
        /// default value if the key is not found.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="defaultValue">The default value to use if the key is not found or cannot be converted to type T.</param>
        /// <returns>The value for the specified key converted to type T or the specified default value.</returns>
        T Get<T>(string key, T defaultValue);

        /// <summary>
        /// Gets the value for the specified key converted to a string. This method never throws an exception. It will return the specified 
        /// default value if the key is not found.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="defaultValue">The default value to use if the key is not found or cannot be converted to a string.</param>
        /// <returns>The value for the specified key converted to a string or the specified default value.</returns>
        string String(string key, string defaultValue = null);

        /// <summary>
        /// Gets the value for the specified key converted to a <see cref="FilePath"/>. This method never throws an exception. It will
        /// return the specified default value if the key is not found or if the string value can't be converted to a <see cref="FilePath"/>.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="defaultValue">The default value to use if the key is not found or cannot be converted to a <see cref="FilePath"/>.</param>
        /// <returns>The value for the specified key converted to a <see cref="FilePath"/> or the specified default value.</returns>
        FilePath FilePath(string key, FilePath defaultValue = null);

        /// <summary>
        /// Gets the value for the specified key converted to a <see cref="DirectoryPath"/>. This method never throws an exception. It will
        /// return the specified default value if the key is not found or if the string value can't be converted to a <see cref="DirectoryPath"/>.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="defaultValue">The default value to use if the key is not found or cannot be converted to a <see cref="DirectoryPath"/>.</param>
        /// <returns>The value for the specified key converted to a <see cref="DirectoryPath"/> or the specified default value.</returns>
        DirectoryPath DirectoryPath(string key, DirectoryPath defaultValue = null);

        /// <summary>
        /// Gets the value for the specified key converted to a <see cref="IReadOnlyList{T}"/>. This method never throws an exception. It will return the specified 
        /// default value if the key is not found. Note that if the value is atomic, the conversion operation will succeed and return a list with one item.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="defaultValue">The default value to use if the key is not found or cannot be converted to a list.</param>
        /// <returns>The value for the specified key converted to a list or the specified default value.</returns>
        IReadOnlyList<T> List<T>(string key, IReadOnlyList<T> defaultValue = null);

        /// <summary>
        /// Gets the value for the specified key converted to a <see cref="IDocument"/>. This method never throws an exception. 
        /// It will return null if the key is not found.
        /// </summary>
        /// <param name="key">The key of the document to get.</param>
        /// <returns>The value for the specified key converted to a string or null.</returns>
        IDocument Document(string key);

        /// <summary>
        /// Gets the value for the specified key converted to a <see cref="IReadOnlyList{IDocument}"/>. This method never throws an exception. 
        /// It will return null if the key is not found and an empty list if the key is found but contains no items that can be converted to <see cref="IDocument"/>.
        /// </summary>
        /// <param name="key">The key of the documents to get.</param>
        /// <returns>The value for the specified key converted to a list or null.</returns>
        IReadOnlyList<IDocument> Documents(string key);

        /// <summary>
        /// Gets the value for the specified key converted to a link. This method never throws an exception. It will return the specified
        /// default value if the key is not found. The difference between this and getting a plain string is that forward-slashes are
        /// replaced with back-slashes to create valid links from file system paths and other strings.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="defaultValue">The default value to use if the key is not found or cannot be converted to a string.</param>
        /// <param name="pretty">If set to <c>true</c>, trailing "/index.html" and "/index.htm" are trimmed.</param>
        /// <returns>The value for the specified key converted to a link or the specified default value.</returns>
        string Link(string key, string defaultValue = null, bool pretty = true);

        /// <summary>
        /// Gets the value associated with the specified key as a dynamic object. This is equivalent
        /// to calling <c>as dynamic</c> to cast the value.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="defaultValue">The default value to use if either the key is not found or the
        /// underlying value is null (since the dynamic runtime binder can't bind null values).</param>
        /// <returns>A dynamic value for the specific key or default value.</returns>
        dynamic Dynamic(string key, object defaultValue = null);
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
