using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
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
		private readonly ConcurrentDictionary<string, IDocument> _commentIdToDocument;
		private readonly ConcurrentDictionary<string, string> _cssClasses;
		private readonly ITrace _trace;
        private List<Action> _processActions; 
        private readonly object _processLock = new object();

		public string ExampleHtml { get; private set; } = string.Empty;
		public string RemarksHtml { get; private set; } = string.Empty;
		public string SummaryHtml { get; private set; } = string.Empty;
		public string ReturnsHtml { get; private set; } = string.Empty;
		public string ValueHtml { get; private set; } = string.Empty;
		public IReadOnlyList<KeyValuePair<string, string>> ExceptionHtml { get; private set; } 
			= ImmutableArray<KeyValuePair<string, string>>.Empty;
		public IReadOnlyList<KeyValuePair<string, string>> PermissionHtml { get; private set; }
			= ImmutableArray<KeyValuePair<string, string>>.Empty;
		public IReadOnlyList<KeyValuePair<string, string>> ParamHtml { get; private set; }
			= ImmutableArray<KeyValuePair<string, string>>.Empty;
		public IReadOnlyList<KeyValuePair<string, string>> TypeParamHtml { get; private set; }
			= ImmutableArray<KeyValuePair<string, string>>.Empty;
		public IReadOnlyList<string> SeeAlsoHtml { get; private set; } 
			= ImmutableArray<string>.Empty;
		public IReadOnlyDictionary<string, IReadOnlyList<KeyValuePair<IReadOnlyDictionary<string, string>, string>>> OtherHtml { get; private set; }
			= ImmutableDictionary<string, IReadOnlyList<KeyValuePair<IReadOnlyDictionary<string, string>, string>>>.Empty;

		public XmlDocumentationParser(
            ConcurrentDictionary<string, IDocument> commentIdToDocument,
			ConcurrentDictionary<string, string> cssClasses, 
			ITrace trace)
		{
			_commentIdToDocument = commentIdToDocument;
			_trace = trace;
			_cssClasses = cssClasses;
        }

        // Returns a list of custom elements
	    public IEnumerable<string> Parse(ISymbol symbol, string documentationCommentXml)
        {
            if (!string.IsNullOrEmpty(documentationCommentXml))
            {
                try
                {
                    // We shouldn't need a root element, the compiler adds a "<member name='Foo.Bar'>" root for us
                    XDocument xml = XDocument.Parse(documentationCommentXml, LoadOptions.PreserveWhitespace);
                    XElement root = xml.Root;
                    if (root != null)
                    {
                        lock (_processLock)
                        {
                            _processActions = new List<Action>();

                            // <seealso> - get all descendant elements (even if they're nested), do this first
                            List<XElement> seeAlsoElements = root.Descendants("seealso").ToList().Select(x =>
                            {
                                x.Remove();
                                return x;
                            }).ToList();
                            if (seeAlsoElements.Count > 0)
                            {
                                _processActions.Add(() => SeeAlsoHtml = GetSeeAlsoHtml(seeAlsoElements));
                            }

                            // All other top-level elements
                            List<IGrouping<string, XElement>> otherElements = new List<IGrouping<string, XElement>>();
                            foreach (IGrouping<string, XElement> group in root.Elements().GroupBy(x => x.Name.ToString()))
                            {
                                switch (group.Key.ToLower(CultureInfo.InvariantCulture))
                                {
                                    case "example":
                                        _processActions.Add(() => ExampleHtml = GetSimpleHtml(group));
                                        break;
                                    case "remarks":
                                        _processActions.Add(() => RemarksHtml = GetSimpleHtml(group));
                                        break;
                                    case "summary":
                                        _processActions.Add(() => SummaryHtml = GetSimpleHtml(group));
                                        break;
                                    case "returns":
                                        _processActions.Add(() => ReturnsHtml = GetSimpleHtml(group));
                                        break;
                                    case "value":
                                        _processActions.Add(() => ValueHtml = GetSimpleHtml(group));
                                        break;
                                    case "exception":
                                        _processActions.Add(() => ExceptionHtml = GetKeyedListHtml(group, true));
                                        break;
                                    case "permission":
                                        _processActions.Add(() => PermissionHtml = GetKeyedListHtml(group, true));
                                        break;
                                    case "param":
                                        _processActions.Add(() => ParamHtml = GetKeyedListHtml(group, false));
                                        break;
                                    case "typeparam":
                                        _processActions.Add(() => TypeParamHtml = GetKeyedListHtml(group, false));
                                        break;
                                    default:
                                        otherElements.Add(group);
                                        break;
                                }
                            }

                            // Add and return the custom elements
                            if (otherElements.Count > 0)
                            {
                                _processActions.Add(() => OtherHtml = otherElements.ToImmutableDictionary(x => x.Key, GetOtherHtml));
                            }
                            return otherElements.Select(x => x.Key);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _trace.Warning($"Could not parse XML documentation comments for {symbol.Name}: {ex.Message}");
                }
            }

	        return Array.Empty<string>();
        } 
        
        // Lazily processes all the elements found while parsing (call after walking all symbols so the reference dictionary will be complete)
		public XmlDocumentationParser Process()
		{
            lock(_processLock)
            {
		        if (_processActions != null)
		        {
		            foreach (Action processAction in _processActions)
		            {
		                processAction();
		            }
                    _processActions = null;
		        }
		        return this;
            }
		}

        private IReadOnlyList<string> GetSeeAlsoHtml(IEnumerable<XElement> elements)
        {
            return elements.Select(element =>
            {
                bool link;
                return GetCrefLinkOrName(element, out link);
            }).ToImmutableArray();
        }

        // <example>, <remarks>, <summary>, <returns>, <value>
        private string GetSimpleHtml(IEnumerable<XElement> elements)
		{
			return string.Join("\n", elements.Select(element =>
			{
				ProcessChildElements(element);
				AddCssClasses(element);
				XmlReader reader = element.CreateReader();
				reader.MoveToContent();
				return reader.ReadInnerXml();
			}));
		}

        // <exception>, <permission>, <param>, <typeParam>
        private IReadOnlyList<KeyValuePair<string, string>> GetKeyedListHtml(IEnumerable<XElement> elements, bool keyIsCref)
		{
			return elements.Select(element =>
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
			}).ToImmutableArray();
		}

	    private IReadOnlyList<KeyValuePair<IReadOnlyDictionary<string, string>, string>> GetOtherHtml(IEnumerable<XElement> elements)
	    {
	        return elements.Select(element =>
            {
                ProcessChildElements(element);
                AddCssClasses(element);
                XmlReader reader = element.CreateReader();
                reader.MoveToContent();
                return new KeyValuePair<IReadOnlyDictionary<string, string>, string>(
                    element.Attributes().Distinct(new XAttributeNameEqualityComparer()).ToImmutableDictionary(x => x.Name.ToString(), x => x.Value),
                    reader.ReadInnerXml());
            }).ToImmutableArray();
	    }

	    private class XAttributeNameEqualityComparer : IEqualityComparer<XAttribute>
	    {
	        public bool Equals(XAttribute x, XAttribute y)
	        {
	            return x.Name.ToString().Equals(y.Name.ToString());
	        }

	        public int GetHashCode(XAttribute obj)
	        {
	            return obj.Name.ToString().GetHashCode();
	        }
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
