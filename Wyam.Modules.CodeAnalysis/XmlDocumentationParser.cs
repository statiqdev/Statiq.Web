using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
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
        private IReadOnlyList<string> _exampleHtml = ImmutableArray<string>.Empty;
        private IReadOnlyList<string> _remarksHtml = ImmutableArray<string>.Empty;
        private IReadOnlyList<string> _summaryHtml = ImmutableArray<string>.Empty;
        private IReadOnlyList<KeyValuePair<string, string>> _exceptionHtml 
            = ImmutableArray<KeyValuePair<string, string>>.Empty;

        public XmlDocumentationParser(ISymbol symbol, ConcurrentDictionary<string, IDocument> commentIdToDocument,
            ConcurrentDictionary<string, string> cssClasses, ITrace trace)
        {
            _symbol = symbol;
            _commentIdToDocument = commentIdToDocument;
            _trace = trace;
            _cssClasses = cssClasses;
        }

        public IReadOnlyList<string> GetExampleHtml()
        {
            Parse();
            return _exampleHtml;
        }

        public IReadOnlyList<string> GetRemarksHtml()
        {
            Parse();
            return _remarksHtml;
        }

        public IReadOnlyList<string> GetSummaryHtml()
        {
            Parse();
            return _summaryHtml;
        }

        public IReadOnlyList<KeyValuePair<string, string>> GetExceptionHtml()
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
        private IReadOnlyList<string> ProcessTopLevelElement(XElement root, string elementName)
        {
            return root.Elements(elementName).Select(element =>
            {
                ProcessChildElements(element);
                AddCssClasses(element);

                // Return InnerXml
                XmlReader reader = element.CreateReader();
                reader.MoveToContent();
                return reader.ReadInnerXml();
            }).ToImmutableArray();
        }

        // <exception>
        private IReadOnlyList<KeyValuePair<string, string>> ProcessExceptionElements(XElement root)
        {
            return root.Elements("exception").Select(exceptionElement =>
            {
                // Get exception class link or name
                XAttribute crefAttribute = exceptionElement.Attribute("cref");
                IDocument crefDoc;
                string linkOrName;
                if (crefAttribute != null && _commentIdToDocument.TryGetValue(crefAttribute.Value, out crefDoc))
                {
                    linkOrName = $"<a href=\"{crefDoc.Link(MetadataKeys.WritePath)}\">{crefDoc[MetadataKeys.DisplayName]}</a>";
                }
                else
                {
                    linkOrName = crefAttribute?.Value.Substring(crefAttribute.Value.IndexOf(':') + 1) ?? string.Empty;
                }

                // Process nested elements in the exception description, get InnerXml, and return the KeyValuePair
                ProcessChildElements(exceptionElement);
                AddCssClasses(exceptionElement);
                XmlReader reader = exceptionElement.CreateReader();
                reader.MoveToContent();
                return new KeyValuePair<string, string>(linkOrName, reader.ReadInnerXml());
            }).ToImmutableArray();
        }

        // Adds/updates CSS classes for all nested elements
        private void AddCssClasses(XElement parentElement)
        {
            foreach (XElement element in parentElement.Descendants())
            {
                string cssClasses;
                if (_cssClasses.TryGetValue(element.Name.ToString(), out cssClasses) && !string.IsNullOrWhiteSpace(cssClasses))
                {
                    XAttribute classAttribute = element.Attribute("class");
                    if (classAttribute != null)
                    {
                        classAttribute.Value = classAttribute.Value + " " + cssClasses;
                    }
                    else
                    {
                        element.Add(new XAttribute("class", cssClasses));
                    }
                }
            }
        }

        // Groups all the nested element processing together so it can be used from multiple parent elements
        private void ProcessChildElements(XElement parentElement)
        {
            ProcessChildCodeElements(parentElement);
            ProcessChildCElements(parentElement);
            ProcessChildListElements(parentElement);
            ProcessChildParaElements(parentElement);
        }

        // <code>
        private void ProcessChildCodeElements(XElement parentElement)
        {
            foreach (XElement codeElement in parentElement.Elements("code"))
            {
                codeElement.ReplaceWith(new XElement("pre", codeElement));
            }
        }

        // <c>
        private void ProcessChildCElements(XElement parentElement)
        {
            foreach (XElement cElement in parentElement.Elements("c"))
            {
                cElement.Name = "code";
            }
        }

        // <list>
        private void ProcessChildListElements(XElement parentElement)
        {
            foreach (XElement listElement in parentElement.Elements("list"))
            {
                XAttribute typeAttribute = listElement.Attribute("type");
                if (typeAttribute != null && typeAttribute.Value == "table")
                {
                    ProcessListElementTable(listElement, typeAttribute);
                }
                else
                {
                    ProcessListElementList(listElement, typeAttribute);
                }
            }
        }

        private void ProcessListElementList(XElement listElement, XAttribute typeAttribute)
        {
            // Number or bullet
            if (typeAttribute != null && typeAttribute.Value == "number")
            {
                listElement.Name = "ol";
            }
            else
            {
                listElement.Name = "ul";
            }
            typeAttribute?.Remove();

            // Replace children
            foreach(XElement itemElement in listElement.Elements("listheader")
                .Concat(listElement.Elements("item")).ToList())
            {
                foreach (XElement termElement in itemElement.Elements("term"))
                {
                    termElement.Name = "strong";
                    ProcessChildElements(termElement);
                }
                foreach (XElement descriptionElement in itemElement.Elements("description"))
                {
                    descriptionElement.Name = "span";
                    ProcessChildElements(descriptionElement);
                }

                itemElement.Name = "li";
            }
        }

        private void ProcessListElementTable(XElement listElement, XAttribute typeAttribute)
        {
            listElement.Name = "table";
            typeAttribute?.Remove();
            
            foreach (XElement itemElement in listElement.Elements("listheader")
                .Concat(listElement.Elements("item")).ToList())
            {
                foreach (XElement termElement in itemElement.Elements("term"))
                {
                    termElement.Name = itemElement.Name == "listheader" ? "th" : "td";
                    ProcessChildElements(termElement);
                }

                itemElement.Name = "tr";
            }
        }

        // <para>
        private void ProcessChildParaElements(XElement parentElement)
        {
            foreach (XElement paraElement in parentElement.Elements("para"))
            {
                paraElement.Name = "p";
                ProcessChildElements(paraElement);
            }
        }

    }
}
