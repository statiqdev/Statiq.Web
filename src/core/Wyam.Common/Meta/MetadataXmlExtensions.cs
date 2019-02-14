using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Wyam.Common.Meta
{
    public static class MetadataXmlExtensions
    {
        /// <summary>
        /// Gets an XML attribute for the given metadata key.
        /// The name of the attribute will be the lower-case key name.
        /// </summary>
        /// <param name="metadata">The metadata containing the value.</param>
        /// <param name="key">The key containing the attribute value.</param>
        /// <returns>The attribute if the key was found, <c>null</c> if not.</returns>
        public static XAttribute XAttribute(this IMetadata metadata, string key) =>
            XAttribute(metadata, key, x => x);

        /// <summary>
        /// Gets an XML attribute for the given metadata key.
        /// </summary>
        /// <param name="metadata">The metadata containing the value.</param>
        /// <param name="name">The name of the XML attribute.</param>
        /// <param name="key">The key containing the attribute value.</param>
        /// <returns>The attribute if the key was found, <c>null</c> if not.</returns>
        public static XAttribute XAttribute(this IMetadata metadata, string name, string key) =>
            XAttribute(metadata, name, key, x => x);

        public static XAttribute XAttribute(this IMetadata metadata, string key, Func<string, string> valueFunc) =>
            XAttribute(metadata, key.ToLower(), key, valueFunc);

        public static XAttribute XAttribute(this IMetadata metadata, string name, string key, Func<string, string> valueFunc) =>
            XAttribute(metadata, key, x => new XAttribute(name, valueFunc(x)));

        public static XAttribute XAttribute(this IMetadata metadata, string key, Func<string, XAttribute> attributeFunc) =>
            metadata.TryGetValue(key, out string value) ? attributeFunc(value) : null;

        public static XElement XElement(this IMetadata metadata, string key, Func<string, object[]> contentFunc) =>
            XElement(metadata, key.ToLower(), key, contentFunc);

        public static XElement XElement(this IMetadata metadata, string name, string key, Func<string, object[]> contentFunc) =>
            XElement(metadata, key, x => new XElement(name, contentFunc(x)));

        public static XElement XElement(this IMetadata metadata, string key, Func<string, XElement> elementFunc) =>
            metadata.TryGetValue(key, out string value) ? elementFunc(value) : null;
    }
}
