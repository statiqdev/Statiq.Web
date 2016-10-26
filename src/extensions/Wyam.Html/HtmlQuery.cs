using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using Wyam.Common;
using Wyam.Common.Modules;
using Wyam.Common.Execution;
using Wyam.Common.Tracing;
using IDocument = Wyam.Common.Documents.IDocument;

namespace Wyam.Html
{
    /// <summary>
    /// Queries HTML content and creates new documents with content and metadata from the results.
    /// </summary>
    /// <remarks>
    /// Once you provide a DOM query selector, the module creates new output documents 
    /// for each query result and allows you to set the new document content and/or set new 
    /// metadata based on the query result.
    /// </remarks>
    /// <metadata name="OuterHtml" type="string">Contains the outer HTML of the query result (unless an alternate metadata key is specified).</metadata>
    /// <metadata name="InnerHtml" type="string">Contains the inner HTML of the query result (unless an alternate metadata key is specified).</metadata>
    /// <metadata name="TextContent" type="string">Contains the text content of the query result (unless an alternate metadata key is specified).</metadata>
    /// <category>Metadata</category>
    public class HtmlQuery : IModule
    {
        private readonly string _querySelector;
        private bool _first;
        private bool? _outerHtmlContent;
        private readonly List<Action<IElement, Dictionary<string, object>>> _metadataActions 
            = new List<Action<IElement, Dictionary<string, object>>>();

        /// <summary>
        /// Creates the module with the specified query selector.
        /// </summary>
        /// <param name="querySelector">The query selector to use.</param>
        public HtmlQuery(string querySelector)
        {
            _querySelector = querySelector;
        }

        /// <summary>
        /// Specifies that only the first query result should be processed (the default is <c>false</c>).
        /// </summary>
        /// <param name="first">If set to <c>true</c>, only the first result is processed.</param>
        public HtmlQuery First(bool first = true)
        {
            _first = first;
            return this;
        }

        /// <summary>
        /// Sets the content of the result document(s) to the content of the corresponding query result, 
        /// optionally specifying whether inner or outer HTML content should be used. The default is
        /// <c>false</c>, which does not add any content to the result documents (only metadata).
        /// </summary>
        /// <param name="outerHtml">If set to <c>true</c>, outer HTML content is used.</param>
        public HtmlQuery SetContent(bool outerHtml = true)
        {
            _outerHtmlContent = outerHtml;
            return this;
        }

        /// <summary>
        /// Gets the outer HTML of each query result and sets it in the metadata of the 
        /// corresponding result document(s) with the specified key.
        /// </summary>
        /// <param name="metadataKey">The metadata key in which to place the outer HTML.</param>
        public HtmlQuery GetOuterHtml(string metadataKey = HtmlKeys.OuterHtml)
        {
            if (!string.IsNullOrWhiteSpace(metadataKey))
            {
                _metadataActions.Add((e, d) => d[metadataKey] = e.OuterHtml);
            }
            return this;
        }

        /// <summary>
        /// Gets the inner HTML of each query result and sets it in the metadata of the 
        /// corresponding result document(s) with the specified key.
        /// </summary>
        /// <param name="metadataKey">The metadata key in which to place the inner HTML.</param>
        public HtmlQuery GetInnerHtml(string metadataKey = HtmlKeys.InnerHtml)
        {
            if (!string.IsNullOrWhiteSpace(metadataKey))
            {
                _metadataActions.Add((e, d) => d[metadataKey] = e.InnerHtml);
            }
            return this;
        }

        /// <summary>
        /// Gets the text content of each query result and sets it in the metadata of 
        /// the corresponding result document(s) with the specified key.
        /// </summary>
        /// <param name="metadataKey">The metadata key in which to place the text content.</param>
        public HtmlQuery GetTextContent(string metadataKey = HtmlKeys.TextContent)
        {
            if (!string.IsNullOrWhiteSpace(metadataKey))
            {
                _metadataActions.Add((e, d) => d[metadataKey] = e.TextContent);
            }
            return this;
        }

        /// <summary>
        /// Gets the specified attribute value of each query result and sets it in the metadata 
        /// of the corresponding result document(s). If the attribute is not found for a given 
        /// query result, no metadata is set. If <c>metadataKey</c> is <c>null</c>, the attribute name will 
        /// be used as the metadata key, otherwise the specified metadata key will be used.
        /// </summary>
        /// <param name="attributeName">Name of the attribute to get.</param>
        /// <param name="metadataKey">The metadata key in which to place the attribute value.</param>
        public HtmlQuery GetAttributeValue(string attributeName, string metadataKey = null)
        {
            if (string.IsNullOrWhiteSpace(metadataKey))
            {
                metadataKey = attributeName;
            }
            _metadataActions.Add((e, d) =>
            {
                if (e.HasAttribute(attributeName))
                {
                    d[metadataKey] = e.GetAttribute(attributeName);
                }
            });
            return this;
        }

        /// <summary>
        /// Gets the values for all attributes of each query result and sets them in the 
        /// metadata of the corresponding result document(s) with keys names equal to the attribute local name.
        /// </summary>
        public HtmlQuery GetAttributeValues()
        {
            _metadataActions.Add((e, d) =>
            {
                foreach (IAttr attribute in e.Attributes)
                {
                    d[attribute.LocalName] = attribute.Value;
                }
            });
            return this;
        }

        /// <summary>
        /// Gets all information for each query result and sets the metadata of the corresponding result 
        /// document(s). This is equivalent to calling <c>GetOuterHtml()</c>, <c>GetInnerHtml()</c>, 
        /// <c>GetTextContent()</c>, and <c>GetAttributeValues()</c> with default arguments.
        /// </summary>
        public HtmlQuery GetAll()
        {
            GetOuterHtml();
            GetInnerHtml();
            GetTextContent();
            GetAttributeValues();
            return this;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            HtmlParser parser = new HtmlParser();
            return inputs.AsParallel().SelectMany(input =>
            {
                // Parse the HTML content
                IHtmlDocument htmlDocument = input.ParseHtml(parser);
                if(htmlDocument == null)
                {
                    return new [] { input };
                }

                // Evaluate the query selector
                try
                {
                    if (!string.IsNullOrWhiteSpace(_querySelector))
                    {
                        IElement[] elements = _first
                            ? new[] {htmlDocument.QuerySelector(_querySelector)}
                            : htmlDocument.QuerySelectorAll(_querySelector).ToArray();
                        if (elements.Length > 0 && elements[0] != null)
                        {
                            List<IDocument> documents = new List<IDocument>();
                            foreach (IElement element in elements)
                            {
                                // Get the metadata
                                Dictionary<string, object> metadata = new Dictionary<string, object>();
                                foreach (Action<IElement, Dictionary<string, object>> metadataAction in _metadataActions)
                                {
                                    metadataAction(element, metadata);
                                }

                                // Clone the document and optionally change content to the HTML element
                                documents.Add(_outerHtmlContent.HasValue
                                    ? context.GetDocument(input, _outerHtmlContent.Value ? element.OuterHtml : element.InnerHtml, metadata.Count == 0 ? null : metadata)
                                    : context.GetDocument(input, metadata));
                            }
                            return (IEnumerable<IDocument>) documents;
                        }
                    }
                    return new[] { input };
                }
                catch (Exception ex)
                {
                    Trace.Warning("Exception while processing HTML for {0}: {1}", input.SourceString(), ex.Message);
                    return new[] { input };
                }
            });
        }
    }
}
