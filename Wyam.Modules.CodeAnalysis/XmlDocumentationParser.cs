using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Wyam.Common;

namespace Wyam.Modules.CodeAnalysis
{
    internal class XmlDocumentationParser
    {
        private readonly ISymbol _symbol;
        private readonly ConcurrentDictionary<string, IDocument> _commentIdToDocument;
        private readonly ConcurrentDictionary<string, string> _cssClasses;
        private readonly ITrace _trace;
        private bool _parsed;
        private string _exampleHtml = string.Empty;
        private string _remarksHtml = string.Empty;
        private string _summaryHtml = string.Empty;
        private string _exceptionHtml = string.Empty;

        public XmlDocumentationParser(ISymbol symbol, ConcurrentDictionary<string, IDocument> commentIdToDocument, 
            ConcurrentDictionary<string, string> cssClasses, ITrace trace)
        {
            _symbol = symbol;
            _commentIdToDocument = commentIdToDocument;
            _trace = trace;
            _cssClasses = cssClasses;
        }

        public string GetExampleHtml()
        {
            Parse();
            return _exampleHtml;
        }

        public string GetRemarksHtml()
        {
            Parse();
            return _remarksHtml;
        }

        public string GetSummaryHtml()
        {
            Parse();
            return _summaryHtml;
        }

        public string GetExceptionHtml()
        {
            Parse();
            return _exceptionHtml;
        }

        private void Parse()
        {
            if (_parsed)
            {
                return;
            }

            string documentationCommentXml;
            if (_symbol != null && !string.IsNullOrWhiteSpace(
                documentationCommentXml = _symbol.GetDocumentationCommentXml(expandIncludes: true)))
            {
                try
                {
                    // We shouldn't need a root element, the compiler adds a "<member name='Foo.Bar'>" root for us
                    XDocument xdoc = XDocument.Parse(documentationCommentXml, LoadOptions.PreserveWhitespace);
                    _exampleHtml = ProcessTopLevelElement(xdoc.Root, "example");
                    _remarksHtml = ProcessTopLevelElement(xdoc.Root, "remarks");
                    _summaryHtml = ProcessTopLevelElement(xdoc.Root, "summary");
                    _exceptionHtml = ProcessExceptionElements(xdoc.Root);
                }
                catch (Exception ex)
                {
                    _trace.Warning($"Could not parse XML documentation comments for {_symbol.Name}: {ex.Message}");
                }
            }

            _parsed = true;
        }

        // <example>, <remarks>, <summary>
        private string ProcessTopLevelElement(XElement root, string elementName)
        {
            return string.Join("\n", root.Elements(elementName).Select(x =>
            {
                ProcessNestedElements(x);

                // Return InnerXml
                var reader = x.CreateReader();
                reader.MoveToContent();
                return $"<div class=\"doc-{elementName}\">{reader.ReadInnerXml()}</div>";
            }));
        }

        // <exception>
        private string ProcessExceptionElements(XElement root)
        {
            string tableTag;
            tableTag = _cssClasses.TryGetValue("table", out tableTag) ? $"<table class=\"{tableTag}\">" : "<table>";
            return $@"{tableTag}
                <thead>
                    <tr>
                        <th>Exception</th>
                        <th>Condition</th>
                    </tr>
                </thead>
                <tbody>{string.Join("\n", root.Elements("exception").Select(ex =>
                    {
                        XAttribute crefAttribute = ex.Attribute("cref");
                        IDocument crefDoc;
                        string exception;
                        if (crefAttribute != null && _commentIdToDocument.TryGetValue(crefAttribute.Value, out crefDoc))
                        {
                            exception = $"<a href=\"{crefDoc.Link(MetadataKeys.WritePath)}\">{crefDoc[MetadataKeys.DisplayName]}</a>";
                        }
                        else
                        {
                            exception = crefAttribute?.Value.Substring(crefAttribute.Value.IndexOf(':') + 1) ?? string.Empty;
                        }
                        ProcessNestedElements(ex);
                        var reader = ex.CreateReader();
                        reader.MoveToContent();
                        return $"<tr><td>{exception}</td><td>{reader.ReadInnerXml()}</td></tr>";
                    }))}
                </tbody>
            </table>";
        }

        // Groups all the nested element processing together so it can be used from multiple parent elements
        private void ProcessNestedElements(XElement parentElement)
        {
            ProcessCodeElement(parentElement);
            ProcessCElement(parentElement);
        }

        // <code>
        private void ProcessCodeElement(XElement parentElement)
        {
            XElement codeElement = parentElement.Element("code");
            codeElement?.ReplaceWith(new XElement("pre", codeElement));
        }

        // <c>
        private void ProcessCElement(XElement parentElement)
        {
            XElement cElement = parentElement.Element("c");
            if (cElement != null)
            {
                cElement.Name = "code";
            }
        }
    }
}
