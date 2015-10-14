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
using Wyam.Common.Pipelines;
using IDocument = Wyam.Common.Documents.IDocument;

namespace Wyam.Modules.Html
{
    public class HtmlQuery : IModule
    {
        private string _querySelector;
        private bool _first;
        private bool? _outerHtmlContent;
        private readonly List<Action<IElement, Dictionary<string, object>>> _metadataActions 
            = new List<Action<IElement, Dictionary<string, object>>>();

        public HtmlQuery(string querySelector)
        {
            _querySelector = querySelector;
        }

        public HtmlQuery First(bool first = true)
        {
            _first = first;
            return this;
        }

        public HtmlQuery WithQuerySelector(string querySelector)
        {
            _querySelector = querySelector;
            return this;
        }

        public HtmlQuery SetContent(bool outerHtml = true)
        {
            _outerHtmlContent = outerHtml;
            return this;
        }

        public HtmlQuery GetOuterHtml(string metadataKey = "OuterHtml")
        {
            if (!string.IsNullOrWhiteSpace(metadataKey))
            {
                _metadataActions.Add((e, d) => d[metadataKey] = e.OuterHtml);
            }
            return this;
        }

        public HtmlQuery GetInnerHtml(string metadataKey = "InnerHtml")
        {
            if (!string.IsNullOrWhiteSpace(metadataKey))
            {
                _metadataActions.Add((e, d) => d[metadataKey] = e.InnerHtml);
            }
            return this;
        }

        public HtmlQuery GetTextContent(string metadataKey = "TextContent")
        {
            if (!string.IsNullOrWhiteSpace(metadataKey))
            {
                _metadataActions.Add((e, d) => d[metadataKey] = e.TextContent);
            }
            return this;
        }

        // If metadataKey is null, attributeName will be used as the metadata key
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
            return inputs.AsParallel().SelectMany(x =>
            {
                // Parse the HTML content
                IHtmlDocument htmlDocument;
                try
                {
                    using (Stream stream = x.GetStream())
                    {
                        htmlDocument = parser.Parse(stream);
                    }
                }
                catch (Exception ex)
                {
                    context.Trace.Warning("Exception while parsing HTML for {0}: {1}", x.Source, ex.Message);
                    return new [] { x };
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
                                    ? x.Clone(_outerHtmlContent.Value ? element.OuterHtml : element.InnerHtml, metadata.Count == 0 ? null : metadata)
                                    : x.Clone(metadata));
                            }
                            return (IEnumerable<IDocument>) documents;
                        }
                    }
                    return new[] { x };
                }
                catch (Exception ex)
                {
                    context.Trace.Warning("Exception while processing HTML for {0}: {1}", x.Source, ex.Message);
                    return new[] { x };
                }
            });
        }
    }
}
