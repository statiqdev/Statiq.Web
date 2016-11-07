using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Meta;
using Wyam.Common.Tracing;

namespace Wyam.CodeAnalysis.Analysis
{
	internal class XmlDocumentationParser
	{
	    private readonly IExecutionContext _context;
	    private readonly ISymbol _symbol;
	    private readonly ConcurrentDictionary<string, IDocument> _commentIdToDocument;
		private readonly ConcurrentDictionary<string, string> _cssClasses;
        private List<Action> _processActions; 
        private readonly object _processLock = new object();

		public string Example { get; private set; } = string.Empty;
		public string Remarks { get; private set; } = string.Empty;
		public string Summary { get; private set; } = string.Empty;
		public string Returns { get; private set; } = string.Empty;
		public string Value { get; private set; } = string.Empty;
		public IReadOnlyList<ReferenceComment> Exceptions { get; private set; } 
			= ImmutableArray<ReferenceComment>.Empty;
		public IReadOnlyList<ReferenceComment> Permissions { get; private set; }
			= ImmutableArray<ReferenceComment>.Empty;
		public IReadOnlyList<ReferenceComment> Params { get; private set; }
			= ImmutableArray<ReferenceComment>.Empty;
		public IReadOnlyList<ReferenceComment> TypeParams { get; private set; }
			= ImmutableArray<ReferenceComment>.Empty;
		public IReadOnlyList<string> SeeAlso { get; private set; } 
			= ImmutableArray<string>.Empty;
		public IReadOnlyDictionary<string, IReadOnlyList<OtherComment>> OtherComments { get; private set; }
			= ImmutableDictionary<string, IReadOnlyList<OtherComment>>.Empty;

		public XmlDocumentationParser(
            IExecutionContext context,
            ISymbol symbol,
            ConcurrentDictionary<string, IDocument> commentIdToDocument,
			ConcurrentDictionary<string, string> cssClasses)
		{
		    _context = context;
		    _symbol = symbol;
		    _commentIdToDocument = commentIdToDocument;
			_cssClasses = cssClasses;
        }

        // Returns a list of custom elements
	    public IEnumerable<string> Parse(string documentationCommentXml)
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
                                _processActions.Add(() => SeeAlso = GetSeeAlsoHtml(seeAlsoElements));
                            }

                            // All other top-level elements
                            List<IGrouping<string, XElement>> otherElements = new List<IGrouping<string, XElement>>();
                            foreach (IGrouping<string, XElement> group in root.Elements().GroupBy(x => x.Name.ToString()))
                            {
                                string elementName = group.Key.ToLower(CultureInfo.InvariantCulture);
                                switch (elementName)
                                {
                                    case "example":
                                        _processActions.Add(() => Example = GetSimpleComment(group, elementName));
                                        break;
                                    case "remarks":
                                        _processActions.Add(() => Remarks = GetSimpleComment(group, elementName));
                                        break;
                                    case "summary":
                                        _processActions.Add(() => Summary = GetSimpleComment(group, elementName));
                                        break;
                                    case "returns":
                                        _processActions.Add(() => Returns = GetSimpleComment(group, elementName));
                                        break;
                                    case "value":
                                        _processActions.Add(() => Value = GetSimpleComment(group, elementName));
                                        break;
                                    case "exception":
                                        _processActions.Add(() => Exceptions = GetReferenceComments(group, true, elementName));
                                        break;
                                    case "permission":
                                        _processActions.Add(() => Permissions = GetReferenceComments(group, true, elementName));
                                        break;
                                    case "param":
                                        _processActions.Add(() => Params = GetReferenceComments(group, false, elementName));
                                        break;
                                    case "typeparam":
                                        _processActions.Add(() => TypeParams = GetReferenceComments(group, false, elementName));
                                        break;
                                    default:
                                        otherElements.Add(group);
                                        break;
                                }
                            }

                            // Add and return the custom elements
                            if (otherElements.Count > 0)
                            {
                                _processActions.Add(() => OtherComments = otherElements.ToImmutableDictionary(x => x.Key, GetOtherComments));
                            }
                            return otherElements.Select(x => x.Key);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Trace.Warning($"Could not parse XML documentation comments for {_symbol.Name}: {ex.Message}");
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
            try
            {
                return elements.Select(element =>
                {
                    string link;
                    string name = GetCrefNameAndLink(element, out link);
                    return link ?? name;
                }).ToImmutableArray();
            }
            catch (Exception ex)
            {
                Trace.Warning($"Could not parse <seealso> XML documentation comments for {_symbol.Name}: {ex.Message}");
            }
            return ImmutableArray<string>.Empty;
        }

        // <example>, <remarks>, <summary>, <returns>, <value>
        private string GetSimpleComment(IEnumerable<XElement> elements, string elementName)
		{
            try
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
            catch (Exception ex)
            {
                Trace.Warning($"Could not parse <{elementName}> XML documentation comments for {_symbol.Name}: {ex.Message}");
            }
            return string.Empty;
		}

        // <exception>, <permission>, <param>, <typeParam>
        private IReadOnlyList<ReferenceComment> GetReferenceComments(IEnumerable<XElement> elements, bool keyIsCref, string elementName)
        {
            try
            {
                return elements.Select(element =>
                {
                    string link = null;
                    string name = keyIsCref
                        ? GetCrefNameAndLink(element, out link)
                        : (element.Attribute("name")?.Value ?? string.Empty);
                    ProcessChildElements(element);
                    AddCssClasses(element);
                    XmlReader reader = element.CreateReader();
                    reader.MoveToContent();
                    return new ReferenceComment(name, link, reader.ReadInnerXml());
                }).ToImmutableArray();
            }
            catch (Exception ex)
            {
                Trace.Warning($"Could not parse <{elementName}> XML documentation comments for {_symbol.Name}: {ex.Message}");
            }
            return ImmutableArray<ReferenceComment>.Empty;
        }

        private IReadOnlyList<OtherComment> GetOtherComments(IEnumerable<XElement> elements)
	    {
	        try
	        {
	            return elements.Select(element =>
                {
                    ProcessChildElements(element);
                    AddCssClasses(element);
                    XmlReader reader = element.CreateReader();
                    reader.MoveToContent();
                    return new OtherComment(
                        element.Attributes().Distinct(new XAttributeNameEqualityComparer()).ToImmutableDictionary(x => x.Name.ToString(), x => x.Value),
                        reader.ReadInnerXml());
                }).ToImmutableArray();
	        }
	        catch (Exception ex)
            {
                Trace.Warning($"Could not parse other XML documentation comments for {_symbol.Name}: {ex.Message}");
            }
	        return ImmutableArray<OtherComment>.Empty;
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
        
        // Returns the name and sets link if one could be found (or null if not)
		private string GetCrefNameAndLink(XElement element, out string link)
		{
			XAttribute crefAttribute = element.Attribute("cref");
			IDocument crefDoc;
			if (crefAttribute != null && _commentIdToDocument.TryGetValue(crefAttribute.Value, out crefDoc))
			{
			    string name = crefDoc.String(CodeAnalysisKeys.DisplayName);
				link = $"<a href=\"{_context.GetLink(crefDoc.FilePath(Keys.WritePath))}\">{name}</a>";
			    return name;
			}
			link = null;
			return crefAttribute?.Value.Substring(crefAttribute.Value.IndexOf(':') + 1) ?? string.Empty;
		}

		// Adds/updates CSS classes for all nested elements
		private void AddCssClasses(XElement parentElement)
		{
			foreach (XElement element in parentElement.Descendants().ToList())
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
			foreach (XElement codeElement in parentElement.Elements("code").ToList())
			{
				codeElement.ReplaceWith(new XElement("pre", codeElement));
			}
		}

		// <c>
		private void ProcessChildCElements(XElement parentElement)
		{
			foreach (XElement cElement in parentElement.Elements("c").ToList())
			{
				cElement.Name = "code";
			}
		}

		// <list>
		private void ProcessChildListElements(XElement parentElement)
		{
			foreach (XElement listElement in parentElement.Elements("list").ToList())
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
				foreach (XElement termElement in itemElement.Elements("term").ToList())
				{
					termElement.Name = "span";
					AddCssClasses(termElement, "term");
					ProcessChildElements(termElement);
				}
				foreach (XElement descriptionElement in itemElement.Elements("description").ToList())
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
			foreach (XElement paraElement in parentElement.Elements("para").ToList())
			{
				paraElement.Name = "p";
				ProcessChildElements(paraElement);
			}
		}

		// <paramref>, <typeparamref>
		private void ProcessChildParamrefAndTypeparamrefElements(XElement parentElement, string elementName)
		{
			foreach (XElement paramrefElement in parentElement.Elements(elementName).ToList())
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
			foreach (XElement seeElement in parentElement.Elements("see").ToList())
			{
				string link;
				string name = GetCrefNameAndLink(seeElement, out link);
				seeElement.ReplaceWith(link != null ? (object)XElement.Parse(link) : name);
			}
		}
	}
}
