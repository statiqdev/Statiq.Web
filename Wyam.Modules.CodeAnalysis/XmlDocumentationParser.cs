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
using Wyam.Common.Documents;
using Wyam.Common.Tracing;

namespace Wyam.Modules.CodeAnalysis
{
    internal class XmlDocumentationParser
    {
        private readonly ISymbol _symbol;
        private readonly ConcurrentDictionary<string, IDocument> _commentIdToDocument;
        private readonly ConcurrentDictionary<string, string> _cssClasses;
        private readonly ITrace _trace;
        private bool _parsed;
        private IReadOnlyList<string> _seeAlsoHtml = ImmutableArray<string>.Empty;
        private string _exampleHtml = string.Empty;
        private string _remarksHtml = string.Empty;
        private string _summaryHtml = string.Empty;
        private string _returnsHtml = string.Empty;
        private string _valueHtml = string.Empty;
        private IReadOnlyList<KeyValuePair<string, string>> _exceptionHtml 
            = ImmutableArray<KeyValuePair<string, string>>.Empty;
        private IReadOnlyList<KeyValuePair<string, string>> _permissionHtml
            = ImmutableArray<KeyValuePair<string, string>>.Empty;
        private IReadOnlyList<KeyValuePair<string, string>> _paramHtml
            = ImmutableArray<KeyValuePair<string, string>>.Empty;
        private IReadOnlyList<KeyValuePair<string, string>> _typeParamHtml
            = ImmutableArray<KeyValuePair<string, string>>.Empty;

        public XmlDocumentationParser(ISymbol symbol, ConcurrentDictionary<string, IDocument> commentIdToDocument,
            ConcurrentDictionary<string, string> cssClasses, ITrace trace)
        {
            _symbol = symbol;
            _commentIdToDocument = commentIdToDocument;
            _trace = trace;
            _cssClasses = cssClasses;
        }

        public IReadOnlyList<string> GetSeeAlsoHtml()
        {
            Parse();
            return _seeAlsoHtml;
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

        public string GetReturnsHtml()
        {
            Parse();
            return _returnsHtml;
        }

        public string GetValueHtml()
        {
            Parse();
            return _valueHtml;
        }

        public IReadOnlyList<KeyValuePair<string, string>> GetExceptionHtml()
        {
            Parse();
            return _exceptionHtml;
        }

        public IReadOnlyList<KeyValuePair<string, string>> GetPermissionHtml()
        {
            Parse();
            return _permissionHtml;
        }

        public IReadOnlyList<KeyValuePair<string, string>> GetParamHtml()
        {
            Parse();
            return _paramHtml;
        }

        public IReadOnlyList<KeyValuePair<string, string>> GetTypeParamHtml()
        {
            Parse();
            return _typeParamHtml;
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
                    _seeAlsoHtml = ProcessSeeAlsoElements(xdoc.Root);
                    _exampleHtml = ProcessRootElement(xdoc.Root, "example");
                    _remarksHtml = ProcessRootElement(xdoc.Root, "remarks");
                    _summaryHtml = ProcessRootElement(xdoc.Root, "summary");
                    _returnsHtml = ProcessRootElement(xdoc.Root, "returns");
                    _valueHtml = ProcessRootElement(xdoc.Root, "value");
                    _exceptionHtml = ProcessExceptionOrPermissionElements(xdoc.Root, "exception");
                    _permissionHtml = ProcessExceptionOrPermissionElements(xdoc.Root, "permission");
                    _paramHtml = ProcessParamOrTypeParamElements(xdoc.Root, "param");
                    _typeParamHtml = ProcessParamOrTypeParamElements(xdoc.Root, "typeparam");
                }
                catch (Exception ex)
                {
                    _trace.Warning($"Could not parse XML documentation comments for {_symbol.Name}: {ex.Message}");
                }
            }

            _parsed = true;
        }

        // <seealso>
        private IReadOnlyList<string> ProcessSeeAlsoElements(XElement root)
        {
            List<string> seeAlso = new List<string>();
            foreach (XElement seealsoElement in root.Descendants("seealso").ToList())
            {
                bool link;
                seeAlso.Add(GetCrefLinkOrName(seealsoElement, out link));
                seealsoElement.Remove();
            }
            return seeAlso.ToImmutableArray();
        }

        // <example>, <remarks>, <summary>, <returns>, <value>
        private string ProcessRootElement(XElement root, string elementName)
        {
            return string.Join("\n", root.Elements(elementName).Select(element =>
            {
                ProcessChildElements(element);
                AddCssClasses(element);

                // Return InnerXml
                XmlReader reader = element.CreateReader();
                reader.MoveToContent();
                return reader.ReadInnerXml();
            }));
        }

        // <exception>, <permission>
        private IReadOnlyList<KeyValuePair<string, string>> ProcessExceptionOrPermissionElements(XElement root, string elementName)
        {
            return root.Elements(elementName).Select(element =>
            {
                bool link;
                string linkOrName = GetCrefLinkOrName(element, out link);
                ProcessChildElements(element);
                AddCssClasses(element);
                XmlReader reader = element.CreateReader();
                reader.MoveToContent();
                return new KeyValuePair<string, string>(linkOrName, reader.ReadInnerXml());
            }).ToImmutableArray();
        }

        // <param>, <typeparam>
        private IReadOnlyList<KeyValuePair<string, string>> ProcessParamOrTypeParamElements(XElement root, string elementName)
        {
            return root.Elements(elementName).Select(element =>
            {
                XAttribute nameAttribute = element.Attribute("name");
                string name = nameAttribute?.Value ?? string.Empty;
                ProcessChildElements(element);
                AddCssClasses(element);
                XmlReader reader = element.CreateReader();
                reader.MoveToContent();
                return new KeyValuePair<string, string>(name, reader.ReadInnerXml());
            }).ToImmutableArray();
        }

        private string GetCrefLinkOrName(XElement element, out bool link)
        {
            XAttribute crefAttribute = element.Attribute("cref");
            IDocument crefDoc;
            if (crefAttribute != null && _commentIdToDocument.TryGetValue(crefAttribute.Value, out crefDoc))
            {
                link = true;
                return $"<a href=\"{crefDoc.Link(MetadataKeys.WritePath)}\">{crefDoc[MetadataKeys.DisplayName]}</a>";
            }
            link = false;
            return crefAttribute?.Value.Substring(crefAttribute.Value.IndexOf(':') + 1) ?? string.Empty;
        }

        // Adds/updates CSS classes for all nested elements
        private void AddCssClasses(XElement parentElement)
        {
            foreach (XElement element in parentElement.Descendants())
            {
                string cssClasses;
                if (_cssClasses.TryGetValue(element.Name.ToString(), out cssClasses) && !string.IsNullOrWhiteSpace(cssClasses))
                {
                    AddCssClasses(element, cssClasses);
                }
            }
        }

        private void AddCssClasses(XElement element, string cssClasses)
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

        // Groups all the nested element processing together so it can be used from multiple parent elements
        private void ProcessChildElements(XElement parentElement)
        {
            ProcessChildCodeElements(parentElement);
            ProcessChildCElements(parentElement);
            ProcessChildListElements(parentElement);
            ProcessChildParaElements(parentElement);
            ProcessChildParamrefAndTypeparamrefElements(parentElement, "paramref");
            ProcessChildParamrefAndTypeparamrefElements(parentElement, "typeparamref");
            ProcessChildSeeElements(parentElement);
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
                    termElement.Name = "span";
                    AddCssClasses(termElement, "term");
                    ProcessChildElements(termElement);
                }
                foreach (XElement descriptionElement in itemElement.Elements("description"))
                {
                    descriptionElement.Name = "span";
                    AddCssClasses(descriptionElement, "description");
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

        // <paramref>, <typeparamref>
        private void ProcessChildParamrefAndTypeparamrefElements(XElement parentElement, string elementName)
        {
            foreach (XElement paramrefElement in parentElement.Elements(elementName))
            {
                XAttribute nameAttribute = paramrefElement.Attribute("name");
                paramrefElement.Value = nameAttribute?.Value ?? string.Empty;
                paramrefElement.Name = "span";
                AddCssClasses(paramrefElement, elementName);
            }
        }

        // <see>
        private void ProcessChildSeeElements(XElement parentElement)
        {
            foreach (XElement seeElement in parentElement.Elements("see"))
            {
                bool link;
                string linkOrName = GetCrefLinkOrName(seeElement, out link);
                seeElement.ReplaceWith(link ? (object)XElement.Parse(linkOrName) : linkOrName);
            }
        }
    }
}
