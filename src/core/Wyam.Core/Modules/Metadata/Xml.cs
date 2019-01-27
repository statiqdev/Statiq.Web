using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Modules;

namespace Wyam.Core.Modules.Metadata
{
    /// <summary>
    /// Creates new documents from elements within XML. This module will either
    /// ignore input documents and use specificed XML content or use the content
    /// from input documents depending on how it's configured. An XPath expression
    /// can be used to find target XML elements, and the InnerXml of each child element
    /// of the target elements as well as the values of each attribute
    /// will be placed into the metadata of the generated documents.
    /// </summary>
    /// <category>Metadata</category>
    public class Xml : ReadDataModule<Xml, Dictionary<string, object>>
    {
        private readonly string _data;
        private readonly Dictionary<string, string> _metadataXPaths = new Dictionary<string, string>();
        private string _itemXPath;

        /// <summary>
        /// Creates new documents from input documents. The child elements of the root element will be used.
        /// </summary>
        public Xml()
        {
        }

        /// <summary>
        /// Creates new documents from input documents.
        /// </summary>
        /// <param name="itemXPath">The XPath expression to use to find child items. If null, all child elements will be used.</param>
        public Xml(string itemXPath)
        {
            _itemXPath = itemXPath;
        }

        /// <summary>
        /// Creates new documents from the specified XML data.
        /// </summary>
        /// <param name="data">The XML data.</param>
        /// <param name="itemXPath">The XPath expression to use to find child items. If <c>null</c>, all child elements will be used.</param>
        public Xml(string data, string itemXPath)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));
            _itemXPath = itemXPath;
        }

        /// <summary>
        /// Sets the XPath expression to use to find child items. If null, all child elements will be used.
        /// </summary>
        /// <param name="itemXPath">The XPath expression to use.</param>
        /// <returns>The current module instance.</returns>
        public Xml WithItemXPath(string itemXPath)
        {
            _itemXPath = itemXPath;
            return this;
        }

        /// <summary>
        /// Adds additional XPath patterns to be run on each element and assigned to a metadata key.
        /// To be safe, these patterns should start with "./" so they scope only to the element.
        /// The InnerXml of the first matching node will be used as the value of the metadata.
        /// </summary>
        /// <param name="key">The metadata key to store the value in.</param>
        /// <param name="xpath">The XPath expression for the additional metadata.</param>
        /// <returns>The current module instance.</returns>
        public Xml WithMetadataXPath(string key, string xpath)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException(nameof(key));
            }
            if (string.IsNullOrEmpty(xpath))
            {
                throw new ArgumentException(nameof(xpath));
            }

            _metadataXPaths.Add(key, xpath);
            return this;
        }

        /// <inheritdoc />
        protected override IEnumerable<Dictionary<string, object>> GetItems(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            // Get XML from the input documents?
            if (_data == null)
            {
                return inputs.AsParallel().SelectMany(input =>
                {
                    XmlDocument inputDoc = new XmlDocument();
                    using (Stream stream = input.GetStream())
                    {
                        inputDoc.Load(stream);
                    }
                    return GetItems(inputDoc);
                });
            }

            // Otherwise load it from the data
            XmlDocument dataDoc = new XmlDocument();
            dataDoc.LoadXml(_data);
            return GetItems(dataDoc);
        }

        private IEnumerable<Dictionary<string, object>> GetItems(XmlDocument doc)
        {
            // Get the elements
            XmlNodeList elements = _itemXPath == null
                ? doc.ChildNodes
                : doc.SelectNodes(_itemXPath);
            if (elements == null)
            {
                return null;
            }

            // Iterate and populate the items
            List<Dictionary<string, object>> items = new List<Dictionary<string, object>>();
            foreach (XmlElement element in elements)
            {
                Dictionary<string, object> dict = new Dictionary<string, object>();

                // Add attributes
                foreach (XmlAttribute attribute in element.Attributes)
                {
                    dict[attribute.Name] = attribute.Value;
                }

                // Iterate the children of each element
                foreach (XmlElement childElement in element.ChildNodes)
                {
                    // Use the element name as the key
                    string key = childElement.Name;

                    if (!dict.ContainsKey(key))
                    {
                        // This key doesn't exist, so just add the content of the element
                        dict.Add(key, childElement.InnerXml);
                    }
                    else
                    {
                        // NOTE: This just might be horrible, since the value of the same key might be a scalar in one document, and a list in another document...

                        // This key exists...
                        List<object> list = dict[key] as List<object>;
                        if (list != null)
                        {
                            // It's already a list, so just add this value to the end of it
                            list.Add(childElement.InnerXml);
                        }
                        else
                        {
                            // It's a scalar value, so turn it into a list of (1) the existing value, and (2) this new value
                            dict[key] = new List<object>
                            {
                                dict[key],
                                childElement.InnerXml
                            };
                        }
                    }
                }

                // These are additional XPath patterns to be run on each element and assigned to a meta key
                // To be safe, those patterns should start with "./" so they scope only to the element
                foreach (KeyValuePair<string, string> metadataXPath in _metadataXPaths)
                {
                    XmlNode node = element.SelectSingleNode(metadataXPath.Value);
                    if (node != null)
                    {
                        dict[metadataXPath.Key] = node.InnerXml;
                    }
                }

                items.Add(dict);
            }
            return items;
        }
    }
}