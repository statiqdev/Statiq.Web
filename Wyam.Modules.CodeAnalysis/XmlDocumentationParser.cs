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
        private readonly XDocument _xml;
        private readonly ConcurrentDictionary<string, IDocument> _commentIdToDocument;
        private readonly ConcurrentDictionary<string, string> _cssClasses;
        private readonly ITrace _trace;

        public XmlDocumentationParser(ISymbol symbol,
            string documentationCommentXml, 
            ConcurrentDictionary<string, IDocument> commentIdToDocument,
            ConcurrentDictionary<string, string> cssClasses, 
            ITrace trace)
        {
            _commentIdToDocument = commentIdToDocument;
            _trace = trace;
            _cssClasses = cssClasses;

            if (!string.IsNullOrEmpty(documentationCommentXml))
            {

                try
                {
                    // We shouldn't need a root element, the compiler adds a "<member name='Foo.Bar'>" root for us
                    _xml = XDocument.Parse(documentationCommentXml, LoadOptions.PreserveWhitespace);
                }
                catch (Exception ex)
                {
                    _trace.Warning($"Could not parse XML documentation comments for {symbol.Name}: {ex.Message}");
                }
            }
        }

        // <example>, <remarks>, <summary>, <returns>, <value>
        private readonly ConcurrentDictionary<string, string> _simpleHtmlCache
            = new ConcurrentDictionary<string, string>();
        
        public string GetSimpleHtml(string elementName)
        {
            // Need to get the <seealso> elements first since they may be inside this element
            GetSeeAlsoHtml();

            return _xml?.Root == null 
                ? string.Empty 
                : _simpleHtmlCache.GetOrAdd(elementName, 
                    _ => string.Join("\n", _xml.Root.Elements(elementName).Select(element =>
                    {
                        ProcessChildElements(element);
                        AddCssClasses(element);

                        // Return InnerXml
                        XmlReader reader = element.CreateReader();
                        reader.MoveToContent();
                        return reader.ReadInnerXml();
                    })));
        }

        // <exception>, <permission>, <param>, <typeParam>
        private readonly ConcurrentDictionary<string, IReadOnlyList<KeyValuePair<string, string>>> _keyedListHtmlCache
            = new ConcurrentDictionary<string, IReadOnlyList<KeyValuePair<string, string>>>();
        
        public IReadOnlyList<KeyValuePair<string, string>> GetKeyedListHtml(string elementName, bool keyIsCref)
        {
            // Need to get the <seealso> elements first since they may be inside this element
            GetSeeAlsoHtml();

            return _xml?.Root == null
                ? ImmutableArray<KeyValuePair<string, string>>.Empty
                : _keyedListHtmlCache.GetOrAdd(elementName,
                    _ => _xml.Root.Elements(elementName).Select(element =>
                    {
                        bool link;
                        string linkOrName = keyIsCref 
                            ? GetCrefLinkOrName(element, out link) 
                            : (element.Attribute("name")?.Value ?? string.Empty);
                        ProcessChildElements(element);
                        AddCssClasses(element);
                        XmlReader reader = element.CreateReader();
                        reader.MoveToContent();
                        return new KeyValuePair<string, string>(linkOrName, reader.ReadInnerXml());
                    }).ToImmutableArray());
        }

        // <seeAlso>
        private readonly ConcurrentDictionary<string, IReadOnlyList<string>> _listHtmlCache
            = new ConcurrentDictionary<string, IReadOnlyList<string>>();

        public IReadOnlyList<string> GetSeeAlsoHtml()
        {
            return _xml?.Root == null
                ? ImmutableArray<string>.Empty
                : _listHtmlCache.GetOrAdd("seealso",
                    _ => _xml.Root.Descendants("seealso").ToList().Select(element =>
                    {
                        bool link;
                        string value = GetCrefLinkOrName(element, out link);
                        element.Remove();
                        return value;
                    }).ToImmutableArray());
        }

        // Custom elements
        private readonly ConcurrentDictionary<string, IReadOnlyList<KeyValuePair<IReadOnlyDictionary<string, string>, string>>> _multipleKeyedListHtmlCache
            = new ConcurrentDictionary<string, IReadOnlyList<KeyValuePair<IReadOnlyDictionary<string, string>, string>>>();

        // TODO: parsing for custom elements

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
