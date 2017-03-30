using System;
using System.Collections.Generic;
using Wyam.Common.Documents;
using Wyam.Common.IO;

namespace Wyam.Common.Meta
{
    /// <summary>
    /// Extensions to make it easier to get typed information from metadata.
    /// </summary>
    public static class MetadataConversionExtensions
    {
        /// <summary>
        /// Gets the value for the specified key converted to a string. This method never throws an exception. It will return the specified
        /// default value if the key is not found.
        /// </summary>
        /// <param name="metadata">The metadata containing the value.</param>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="defaultValue">The default value to use if the key is not found or cannot be converted to a string.</param>
        /// <returns>The value for the specified key converted to a string or the specified default value.</returns>
        public static string String(this IMetadata metadata, string key, string defaultValue = null) => metadata.Get(key, defaultValue);

        /// <summary>
        /// Gets the value for the specified key converted to a bool. This method never throws an exception. It will return the specified
        /// default value if the key is not found.
        /// </summary>
        /// <param name="metadata">The metadata containing the value.</param>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="defaultValue">The default value to use if the key is not found or cannot be converted to a bool.</param>
        /// <returns>The value for the specified key converted to a bool or the specified default value.</returns>
        public static bool Bool(this IMetadata metadata, string key, bool defaultValue = false) => metadata.Get(key, defaultValue);

        /// <summary>
        /// Gets the value for the specified key converted to a <see cref="DateTime"/>. This method never throws an exception. It will return the specified
        /// default value if the key is not found.
        /// </summary>
        /// <param name="metadata">The metadata containing the value.</param>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="defaultValue">The default value to use if the key is not found or cannot be converted to a <see cref="DateTime"/>.</param>
        /// <returns>The value for the specified key converted to a <see cref="DateTime"/> or the specified default value.</returns>
        public static DateTime DateTime(this IMetadata metadata, string key, DateTime defaultValue = default(DateTime)) => metadata.Get(key, defaultValue);

        /// <summary>
        /// Gets the value for the specified key converted to a <see cref="FilePath"/>. This method never throws an exception. It will
        /// return the specified default value if the key is not found or if the string value can't be converted to a <see cref="FilePath"/>.
        /// </summary>
        /// <param name="metadata">The metadata containing the value.</param>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="defaultValue">The default value to use if the key is not found or cannot be converted to a <see cref="FilePath"/>.</param>
        /// <returns>The value for the specified key converted to a <see cref="FilePath"/> or the specified default value.</returns>
        public static FilePath FilePath(this IMetadata metadata, string key, FilePath defaultValue = null) => metadata.Get(key, defaultValue);

        /// <summary>
        /// Gets the value for the specified key converted to a <see cref="DirectoryPath"/>. This method never throws an exception. It will
        /// return the specified default value if the key is not found or if the string value can't be converted to a <see cref="DirectoryPath"/>.
        /// </summary>
        /// <param name="metadata">The metadata containing the value.</param>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="defaultValue">The default value to use if the key is not found or cannot be converted to a <see cref="DirectoryPath"/>.</param>
        /// <returns>The value for the specified key converted to a <see cref="DirectoryPath"/> or the specified default value.</returns>
        public static DirectoryPath DirectoryPath(this IMetadata metadata, string key, DirectoryPath defaultValue = null) => metadata.Get(key, defaultValue);

        /// <summary>
        /// Gets the value for the specified key converted to a <see cref="IReadOnlyList{T}"/>. This method never throws an exception. It will return the specified
        /// default value if the key is not found. Note that if the value is atomic, the conversion operation will succeed and return a list with one item.
        /// </summary>
        /// <typeparam name="T">The type to convert to.</typeparam>
        /// <param name="metadata">The metadata containing the value.</param>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="defaultValue">The default value to use if the key is not found or cannot be converted to a list.</param>
        /// <returns>The value for the specified key converted to a list or the specified default value.</returns>
        public static IReadOnlyList<T> List<T>(this IMetadata metadata, string key, IReadOnlyList<T> defaultValue = null) => metadata.Get(key, defaultValue);

        /// <summary>
        /// Gets the value for the specified key converted to a <see cref="IDocument"/>. This method never throws an exception.
        /// It will return null if the key is not found.
        /// </summary>
        /// <param name="metadata">The metadata containing the value.</param>
        /// <param name="key">The key of the document to get.</param>
        /// <param name="defaultValue">The default value to use if the key is not found or cannot be converted to a document.</param>
        /// <returns>The value for the specified key converted to a string or null.</returns>
        public static IDocument Document(this IMetadata metadata, string key, IDocument defaultValue = null) => metadata.Get(key, defaultValue);

        /// <summary>
        /// Gets the value for the specified key converted to a <see cref="IReadOnlyList{IDocument}"/>. This method never throws an exception.
        /// It will return null if the key is not found and an empty list if the key is found but contains no items that can be converted to <see cref="IDocument"/>.
        /// </summary>
        /// <param name="metadata">The metadata containing the value.</param>
        /// <param name="key">The key of the documents to get.</param>
        /// <param name="defaultValue">The default value to use if the key is not found or cannot be converted to a document list.</param>
        /// <returns>The value for the specified key converted to a list or null.</returns>
        public static IReadOnlyList<IDocument> DocumentList(this IMetadata metadata, string key, IReadOnlyList<IDocument> defaultValue = null) => metadata.Get(key, defaultValue);

        /// <summary>
        /// Gets the value associated with the specified key as a dynamic object. This is equivalent
        /// to calling <c>as dynamic</c> to cast the value.
        /// </summary>
        /// <param name="metadata">The metadata containing the value.</param>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="defaultValue">The default value to use if either the key is not found or the
        /// underlying value is null (since the dynamic runtime binder can't bind null values).</param>
        /// <returns>A dynamic value for the specific key or default value.</returns>
        public static dynamic Dynamic(this IMetadata metadata, string key, object defaultValue = null) => metadata.Get(key, defaultValue) ?? defaultValue;
    }
}
