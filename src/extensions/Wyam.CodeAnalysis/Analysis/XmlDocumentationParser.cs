using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Meta;
using Wyam.Common.Tracing;
using Wyam.Common.Util;

namespace Wyam.CodeAnalysis.Analysis
{
    internal class XmlDocumentationParser
    {
        private readonly IExecutionContext _context;
        private readonly ISymbol _symbol;
        private readonly Compilation _compilation;
        private readonly ConcurrentDictionary<ISymbol, IDocument> _symbolToDocument;
        private readonly ConcurrentDictionary<string, string> _cssClasses;
        private readonly object _processLock = new object();
        private List<Action> _processActions;

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
            Compilation compilation,
            ConcurrentDictionary<ISymbol, IDocument> symbolToDocument,
            ConcurrentDictionary<string, string> cssClasses)
        {
            _context = context;
            _symbol = symbol;
            _compilation = compilation;
            _symbolToDocument = symbolToDocument;
            _cssClasses = cssClasses;
        }

        // Returns a list of custom elements
        public IEnumerable<string> Parse(string documentationCommentXml)
        {
            if (!string.IsNullOrEmpty(documentationCommentXml))
            {
                try
                {
                    // Process the elements
                    XElement root = GetRootElement(documentationCommentXml);
                    if (root != null)
                    {
                        lock (_processLock)
                        {
                            _processActions = new List<Action>();

                            // Add inherited documentation, do this very first since it manipulates the root XML
                            ProcessInheritDoc(root, _symbol, root.Elements("inheritdoc").ToList(), new HashSet<string>());

                            // <seealso> - get all descendant elements (even if they're nested),
                            // do this first since it will modify the comment XML to remove the <seealso> elements
                            List<XElement> seeAlsoElements = root.Descendants("seealso").ToList().Select(x =>
                            {
                                x.Remove();
                                return x;
                            }).ToList();
                            if (seeAlsoElements.Count > 0)
                            {
                                _processActions.Add(() => SeeAlso = GetSeeAlsoHtml(seeAlsoElements));
                            }

                            // All other top-level elements as individual actions so we don't process those elements if they don't exist
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
                                        _processActions.Add(() => Params = GetReferenceComments(
                                            group,
                                            false,
                                            elementName,
                                            (_symbol as IMethodSymbol)?.Parameters.Select(x => x.Name).ToArray() ?? Array.Empty<string>()));
                                        break;
                                    case "typeparam":
                                        _processActions.Add(() => TypeParams = GetReferenceComments(
                                            group,
                                            false,
                                            elementName,
                                            (_symbol as IMethodSymbol)?.TypeParameters.Select(x => x.Name).ToArray() ?? (_symbol as INamedTypeSymbol)?.TypeParameters.Select(x => x.Name).ToArray() ?? Array.Empty<string>()));
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
            lock (_processLock)
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

        // We shouldn't need a root element, the compiler adds a "<member name='Foo.Bar'>" root for us
        // unless we're using a custom XML documentation provider (I.e., for assembly docs), so add a root
        // and then ignore it if we got the root <member> element
        private XElement GetRootElement(string xml)
        {
            XDocument document = XDocument.Parse($"<root>{xml}</root>", LoadOptions.PreserveWhitespace);
            XElement root = document.Root;
            if (root?.Elements().Count() == 1
                && string.Equals(root.Elements().First().Name.LocalName, "member", StringComparison.OrdinalIgnoreCase))
            {
                root = root.Elements().First();
            }
            return root;
        }

        // Can be removed if https://github.com/dotnet/roslyn/issues/67 gets resolved
        // Modeled after Sandcastle implementation: http://tunnelvisionlabs.github.io/SHFB/docs-master/XMLCommentsGuide/html/86453FFB-B978-4A2A-9EB5-70E118CA8073.htm
        private void ProcessInheritDoc(XElement root, ISymbol currentSymbol, List<XElement> inheritDocElements, HashSet<string> inheritedSymbolCommentIds)
        {
            if (inheritDocElements.Count > 0)
            {
                // Gather the documents (first in the list takes precedence)
                List<ISymbol> inheritedSymbols = new List<ISymbol>();
                foreach (XElement inheritDocElement in inheritDocElements)
                {
                    // Remove from the parent
                    inheritDocElement.Remove();

                    // Locate the appropriate symbol
                    string inheritDocElementCref = inheritDocElement.Attribute("cref")?.Value;
                    if (inheritDocElementCref == null && inheritedSymbolCommentIds.Add(currentSymbol.GetDocumentationCommentId()))
                    {
                        INamedTypeSymbol currentTypeSymbol = currentSymbol as INamedTypeSymbol;
                        IMethodSymbol currentMethodSymbol = currentSymbol as IMethodSymbol;
                        IPropertySymbol currentPropertySymbol = currentSymbol as IPropertySymbol;
                        IEventSymbol currentEventSymbol = currentSymbol as IEventSymbol;
                        if (currentTypeSymbol != null)
                        {
                            // Types and interfaces, inherit from all base types
                            List<INamedTypeSymbol> baseTypeSymbols = AnalyzeSymbolVisitor.GetBaseTypes(currentTypeSymbol)
                                .Where(x => inheritedSymbolCommentIds.Add(x.GetDocumentationCommentId()))
                                .ToList();
                            if (baseTypeSymbols.Count > 0)
                            {
                                inheritedSymbols.AddRange(baseTypeSymbols);
                            }

                            // Then inherit from all interfaces
                            List<INamedTypeSymbol> interfaceSymbols = currentTypeSymbol.AllInterfaces
                                .Where(x => inheritedSymbolCommentIds.Add(x.GetDocumentationCommentId()))
                                .ToList();
                            if (interfaceSymbols.Count > 0)
                            {
                                inheritedSymbols.AddRange(interfaceSymbols);
                            }
                        }
                        else if (currentMethodSymbol != null && currentMethodSymbol.Name == currentMethodSymbol.ContainingType.Name)
                        {
                            // Constructor, check base type constructors for the same signature
                            string signature = AnalyzeSymbolVisitor.GetFullName(currentMethodSymbol);
                            signature = signature.Substring(signature.IndexOf('('));
                            foreach (INamedTypeSymbol baseTypeSymbol in AnalyzeSymbolVisitor.GetBaseTypes(currentMethodSymbol.ContainingType))
                            {
                                foreach (IMethodSymbol constructorSymbol in baseTypeSymbol.Constructors.Where(x => !x.IsImplicitlyDeclared))
                                {
                                    string constructorSignature = AnalyzeSymbolVisitor.GetFullName(constructorSymbol);
                                    constructorSignature = constructorSignature.Substring(constructorSignature.IndexOf('('));
                                    if (signature == constructorSignature
                                        && inheritedSymbolCommentIds.Add(constructorSymbol.GetDocumentationCommentId()))
                                    {
                                        inheritedSymbols.Add(constructorSymbol);
                                    }
                                }
                            }
                        }
                        else if (currentMethodSymbol != null)
                        {
                            PopulateInheritedMemberSymbols(currentMethodSymbol, x => x.OverriddenMethod, inheritedSymbolCommentIds, inheritedSymbols);
                        }
                        else if (currentPropertySymbol != null)
                        {
                            PopulateInheritedMemberSymbols(currentPropertySymbol, x => x.OverriddenProperty, inheritedSymbolCommentIds, inheritedSymbols);
                        }
                        else if (currentEventSymbol != null)
                        {
                            PopulateInheritedMemberSymbols(currentEventSymbol, x => x.OverriddenEvent, inheritedSymbolCommentIds, inheritedSymbols);
                        }
                    }
                    else if (inheritDocElementCref != null)
                    {
                        // Explicit cref
                        if (inheritedSymbolCommentIds.Add(inheritDocElementCref))
                        {
                            ISymbol inheritedSymbol = DocumentationCommentId.GetFirstSymbolForDeclarationId(inheritDocElementCref, _compilation);
                            if (inheritedSymbol != null)
                            {
                                inheritedSymbols.Add(inheritedSymbol);
                            }
                        }
                    }
                }

                // Add the inherited comments
                foreach (ISymbol inheritedSymbol in inheritedSymbols)
                {
                    string inheritedXml = inheritedSymbol.GetDocumentationCommentXml(expandIncludes: true);
                    if (!string.IsNullOrEmpty(inheritedXml))
                    {
                        XElement inheritedRoot = GetRootElement(inheritedXml);
                        if (inheritedRoot != null)
                        {
                            // Inherit elements other than <inheritdoc>
                            List<XElement> inheritedInheritDocElements = new List<XElement>();
                            foreach (XElement inheritedElement in inheritedRoot.Elements())
                            {
                                if (inheritedElement.Name == "inheritdoc")
                                {
                                    inheritedInheritDocElements.Add(inheritedElement);
                                }
                                else
                                {
                                    string inheritedElementCref = inheritedElement.Attribute("cref")?.Value;
                                    string inheritedElementName = inheritedElement.Attribute("name")?.Value;
                                    bool inherit = true;
                                    foreach (XElement rootElement in root.Elements(inheritedElement.Name))
                                    {
                                        if (inheritedElementCref == null && inheritedElementName == null)
                                        {
                                            // Don't inherit if the name is the same and there's no distinguishing attributes
                                            inherit = false;
                                            break;
                                        }
                                        if (inheritedElementCref != null && inheritedElementCref == rootElement.Attribute("cref")?.Value)
                                        {
                                            // Don't inherit if the cref attribute is the same
                                            inherit = false;
                                            break;
                                        }
                                        if (inheritedElementName != null && inheritedElementName == rootElement.Attribute("name")?.Value)
                                        {
                                            // Don't inherit if the name attribute is the same
                                            inherit = false;
                                            break;
                                        }
                                    }
                                    if (inherit)
                                    {
                                        root.Add(inheritedElement);
                                    }
                                }
                            }

                            // Recursively inherit <inheritdoc>
                            if (inheritedInheritDocElements.Count > 0)
                            {
                                ProcessInheritDoc(root, inheritedSymbol, inheritedInheritDocElements, inheritedSymbolCommentIds);
                            }
                        }
                    }
                }
            }
        }

        private void PopulateInheritedMemberSymbols<TSymbol>(
            TSymbol symbol,
            Func<TSymbol, TSymbol> getOverriddenSymbol,
            HashSet<string> inheritedSymbolCommentIds,
            List<ISymbol> inheritedSymbols)
            where TSymbol : class, ISymbol
        {
            TSymbol overriddenMethodSymbol = null;
            if (symbol.IsOverride)
            {
                // Override, get overridden method
                overriddenMethodSymbol = getOverriddenSymbol(symbol);
                if (overriddenMethodSymbol != null
                    && inheritedSymbolCommentIds.Add(overriddenMethodSymbol.GetDocumentationCommentId()))
                {
                    inheritedSymbols.Add(overriddenMethodSymbol);
                }
            }

            // Check if this is an interface implementation
            TSymbol interfaceSymbol = symbol.ContainingType.AllInterfaces
                .SelectMany(x => x.GetMembers().OfType<TSymbol>())
                .FirstOrDefault(x =>
                {
                    ISymbol implementationSymbol = symbol.ContainingType.FindImplementationForInterfaceMember(x);
                    return symbol.Equals(implementationSymbol)
                           || (overriddenMethodSymbol?.Equals(implementationSymbol) == true);
                });
            if (interfaceSymbol != null
                && inheritedSymbolCommentIds.Add(interfaceSymbol.GetDocumentationCommentId()))
            {
                inheritedSymbols.Add(interfaceSymbol);
            }
        }

        private IReadOnlyList<string> GetSeeAlsoHtml(IEnumerable<XElement> elements)
        {
            try
            {
                return elements.Select(element =>
                {
                    string link;
                    string name = GetRefNameAndLink(element, out link);
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
                }).Distinct());
            }
            catch (Exception ex)
            {
                Trace.Warning($"Could not parse <{elementName}> XML documentation comments for {_symbol.Name}: {ex.Message}");
            }
            return string.Empty;
        }

        // <exception>, <permission>, <param>, <typeParam>
        private IReadOnlyList<ReferenceComment> GetReferenceComments(IEnumerable<XElement> elements, bool keyIsCref, string elementName, string[] validNames = null)
        {
            try
            {
                return elements.Select(element =>
                {
                    string link = null;
                    string name = keyIsCref
                        ? GetRefNameAndLink(element, out link)
                        : (element.Attribute("name")?.Value ?? string.Empty);
                    if (validNames?.Contains(name) == false)
                    {
                        return null;
                    }
                    ProcessChildElements(element);
                    AddCssClasses(element);
                    XmlReader reader = element.CreateReader();
                    reader.MoveToContent();
                    return new ReferenceComment(name, link, reader.ReadInnerXml());
                })
                .Where(x => x != null)
                .ToImmutableArray();
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
        // First checks for "href" attribute and then checks for "cref"
        private string GetRefNameAndLink(XElement element, out string link)
        {
            // Check for href
            XAttribute hrefAttribute = element.Attribute("href");
            if (hrefAttribute != null)
            {
                link = $"<a href=\"{hrefAttribute.Value}\">{element.Value}</a>";
                return element.Value;
            }

            // Check for cref
            string cref = element.Attribute("cref")?.Value;
            if (cref != null)
            {
                IDocument crefDoc;
                ISymbol crefSymbol = DocumentationCommentId.GetFirstSymbolForDeclarationId(cref, _compilation);
                if (crefSymbol != null && _symbolToDocument.TryGetValue(crefSymbol, out crefDoc))
                {
                    string name = crefDoc.String(CodeAnalysisKeys.DisplayName);
                    link = $"<code><a href=\"{_context.GetLink(crefDoc.FilePath(Keys.WritePath))}\">{WebUtility.HtmlEncode(name)}</a></code>";
                    return name;
                }
            }
            link = null;
            return cref?.Substring(cref.IndexOf(':') + 1) ?? string.Empty;
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
            ProcessDescendantCdataElements(parentElement);
            ProcessChildCodeElements(parentElement);
            ProcessChildCElements(parentElement);
            ProcessChildListElements(parentElement);
            ProcessChildParaElements(parentElement);
            ProcessChildParamrefAndTypeparamrefElements(parentElement, "paramref");
            ProcessChildParamrefAndTypeparamrefElements(parentElement, "typeparamref");
            ProcessChildSeeElements(parentElement);
        }

        // CDATA
        // Should escape all CDATA content and remove the CDATA element
        private void ProcessDescendantCdataElements(XElement parentElement)
        {
            // Take them one at a time in case we erase one during the processing
            XCData cdata = parentElement.DescendantNodes().OfType<XCData>().FirstOrDefault();
            while (cdata != null)
            {
                cdata.ReplaceWith(new XText(cdata.Value.Trim()));
                cdata = parentElement.DescendantNodes().OfType<XCData>().FirstOrDefault();
            }
        }

        // <code>
        // Wrap with <pre> and trim margins off each line
        private void ProcessChildCodeElements(XElement parentElement)
        {
            foreach (XElement codeElement in parentElement.Elements("code").ToList())
            {
                // Get all the lines of the code element
                XmlReader reader = codeElement.CreateReader();
                reader.MoveToContent();
                List<string> lines = reader.ReadInnerXml().Split(new[] { "\n", "\r\n" }, StringSplitOptions.None).ToList();

                // Trim start and end lines
                while (lines[0].Trim() == string.Empty)
                {
                    lines.RemoveAt(0);
                }
                while (lines[lines.Count - 1].Trim() == string.Empty)
                {
                    lines.RemoveAt(lines.Count - 1);
                }

                // Tabs vs. spaces
                bool tabs = lines.Count > 0 && lines[0].StartsWith("\t");

                // Find the margin padding
                int padding = int.MaxValue;
                foreach (string line in lines)
                {
                    padding = Math.Min(padding, line.TakeWhile(x => tabs ? x == '\t' : x == ' ').Count());
                }

                // Remove the padding, replacing the nodes in the original element to preserve any attributes
                if (padding > 0)
                {
                    string newInnerXml = string.Join(
                        "\n",
                        lines.Select(x => padding < x.Length ? x.Substring(padding) : string.Empty));
                    XElement newCodeElement = XElement.Parse($"<code>{newInnerXml}</code>");
                    codeElement.ReplaceNodes(newCodeElement.Nodes().Cast<object>().ToArray());
                }

                // Wrap with pre
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
                if (typeAttribute?.Value == "table")
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
            if (typeAttribute?.Value == "number")
            {
                listElement.Name = "ol";
            }
            else
            {
                listElement.Name = "ul";
            }
            typeAttribute?.Remove();

            // Replace children
            foreach (XElement itemElement in listElement.Elements("listheader")
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
                string name = GetRefNameAndLink(seeElement, out link);
                seeElement.ReplaceWith(XElement.Parse(link ?? $"<code>{WebUtility.HtmlEncode(name)}</code>"));
            }
        }
    }
}
