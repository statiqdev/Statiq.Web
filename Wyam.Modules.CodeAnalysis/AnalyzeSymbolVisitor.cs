using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Wyam.Common;
using Wyam.Common.Caching;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Pipelines;

namespace Wyam.Modules.CodeAnalysis
{
    internal class AnalyzeSymbolVisitor : SymbolVisitor
    {
        private readonly ConcurrentDictionary<ISymbol, IDocument> _symbolToDocument = new ConcurrentDictionary<ISymbol, IDocument>();
        private readonly ConcurrentDictionary<string, IDocument> _commentIdToDocument = new ConcurrentDictionary<string, IDocument>();
        private ImmutableArray<KeyValuePair<INamedTypeSymbol, IDocument>> _namedTypes;  // This contains all of the NamedType symbols and documents obtained during the initial processing
        private readonly IExecutionContext _context;
        private readonly Func<ISymbol, bool> _symbolPredicate;
        private readonly Func<IMetadata, string> _writePath;
        private readonly ConcurrentDictionary<string, string> _cssClasses;
        private readonly bool _docsForImplicitSymbols;
        private bool _finished; // When this is true, we're visiting external symbols and should omit certain metadata and don't descend

        public AnalyzeSymbolVisitor(
            IExecutionContext context, 
            Func<ISymbol, bool> symbolPredicate, 
            Func<IMetadata, string> writePath, 
            ConcurrentDictionary<string, string> cssClasses,
            bool docsForImplicitSymbols)
        {
            _context = context;
            _symbolPredicate = symbolPredicate;
            _writePath = writePath;
            _cssClasses = cssClasses;
            _docsForImplicitSymbols = docsForImplicitSymbols;
        }

        public IEnumerable<IDocument> Finish()
        {
            _finished = true;
            _namedTypes = _symbolToDocument
                .Where(x => x.Key.Kind == SymbolKind.NamedType)
                .Select(x => new KeyValuePair<INamedTypeSymbol, IDocument>((INamedTypeSymbol)x.Key, x.Value))
                .ToImmutableArray();
            return _symbolToDocument.Select(x => x.Value);
        }

        public bool TryGetDocument(ISymbol symbol, out IDocument document)
        {
            return _symbolToDocument.TryGetValue(symbol, out document);
        }

        public override void VisitNamespace(INamespaceSymbol symbol)
        {
            if (_finished || _symbolPredicate == null || _symbolPredicate(symbol))
            {
                AddDocument(symbol, true, new MetadataItems
                {
                    { MetadataKeys.SpecificKind, (k, m) => symbol.Kind.ToString() },
                    { MetadataKeys.MemberNamespaces, DocumentsFor(symbol.GetNamespaceMembers()) },
                    { MetadataKeys.MemberTypes, DocumentsFor(symbol.GetTypeMembers()) }
                });
            }

            // Descend if not finished, regardless if this namespace was included
            if (!_finished)
            {
                Parallel.ForEach(symbol.GetMembers(), s => s.Accept(this));
            }
        }

        public override void VisitNamedType(INamedTypeSymbol symbol)
        {
            if (_finished || _symbolPredicate == null || _symbolPredicate(symbol))
            {
                MetadataItems metadata = new MetadataItems
                {
                    { MetadataKeys.SpecificKind, (k, m) => symbol.TypeKind.ToString() },
                    { MetadataKeys.ContainingType, DocumentFor(symbol.ContainingType) },
                    { MetadataKeys.MemberTypes, DocumentsFor(symbol.GetTypeMembers()) },
                    { MetadataKeys.BaseType, DocumentFor(symbol.BaseType) },
                    { MetadataKeys.AllInterfaces, DocumentsFor(symbol.AllInterfaces) },
                    { MetadataKeys.Members, DocumentsFor(symbol.GetMembers().Where(MemberPredicate)) },
                    { MetadataKeys.Constructors,
                        DocumentsFor(symbol.Constructors.Where(x => !x.IsImplicitlyDeclared)) },
                    { MetadataKeys.TypeParameters, DocumentsFor(symbol.TypeParameters) },
                    { MetadataKeys.Accessibility, (k, m) => symbol.DeclaredAccessibility.ToString() }
                };
                if (!_finished)
                {
                    metadata.AddRange(new []
                    {
                        new MetadataItem(MetadataKeys.DerivedTypes, (k, m) => GetDerivedTypes(symbol), true),
                        new MetadataItem(MetadataKeys.ImplementingTypes, (k, m) => GetImplementingTypes(symbol), true)
                    });
                }
                AddDocument(symbol, true, metadata);

                // Descend if not finished, and only if this type was included
                if (!_finished)
                {
                    Parallel.ForEach(symbol.GetMembers()
                        .Where(MemberPredicate)
                        .Concat(symbol.Constructors.Where(x => !x.IsImplicitlyDeclared)),
                        s => s.Accept(this));
                }
            }
        }

        public override void VisitTypeParameter(ITypeParameterSymbol symbol)
        {
            if (_finished || _symbolPredicate == null || _symbolPredicate(symbol))
            {
                AddDocumentForMember(symbol, false, new MetadataItems
                {
                    new MetadataItem(MetadataKeys.SpecificKind, (k, m) => symbol.TypeParameterKind.ToString()),
                    new MetadataItem(MetadataKeys.DeclaringType, DocumentFor(symbol.DeclaringType))
                });
            }
        }

        public override void VisitParameter(IParameterSymbol symbol)
        {
            if (_finished || _symbolPredicate == null || _symbolPredicate(symbol))
            {
                AddDocumentForMember(symbol, false, new MetadataItems
                {
                    new MetadataItem(MetadataKeys.SpecificKind, (k, m) => symbol.Kind.ToString()),
                    new MetadataItem(MetadataKeys.Type, DocumentFor(symbol.Type))
                });
            }
        }

        public override void VisitMethod(IMethodSymbol symbol)
        {
            if (_finished || _symbolPredicate == null || _symbolPredicate(symbol))
            {
                AddDocumentForMember(symbol, true, new MetadataItems
                {
                    new MetadataItem(MetadataKeys.SpecificKind,
                        (k, m) => symbol.MethodKind == MethodKind.Ordinary ? "Method" : symbol.MethodKind.ToString()),
                    new MetadataItem(MetadataKeys.TypeParameters, DocumentsFor(symbol.TypeParameters)),
                    new MetadataItem(MetadataKeys.Parameters, DocumentsFor(symbol.Parameters)),
                    new MetadataItem(MetadataKeys.ReturnType, DocumentFor(symbol.ReturnType)),
                    new MetadataItem(MetadataKeys.OverriddenMethod, DocumentFor(symbol.OverriddenMethod)),
                    new MetadataItem(MetadataKeys.Accessibility, (k, m) => symbol.DeclaredAccessibility.ToString())
                });
            }
        }

        public override void VisitField(IFieldSymbol symbol)
        {
            if (_finished || _symbolPredicate == null || _symbolPredicate(symbol))
            {
                AddDocumentForMember(symbol, true, new MetadataItems
                {
                    new MetadataItem(MetadataKeys.SpecificKind, (k, m) => symbol.Kind.ToString()),
                    new MetadataItem(MetadataKeys.Type, DocumentFor(symbol.Type)),
                    new MetadataItem(MetadataKeys.Accessibility, (k, m) => symbol.DeclaredAccessibility.ToString())
                });
            }
        }

        public override void VisitEvent(IEventSymbol symbol)
        {
            if (_finished || _symbolPredicate == null || _symbolPredicate(symbol))
            {
                AddDocumentForMember(symbol, true, new MetadataItems
                {
                    new MetadataItem(MetadataKeys.SpecificKind, (k, m) => symbol.Kind.ToString()),
                    new MetadataItem(MetadataKeys.Type, DocumentFor(symbol.Type)),
                    new MetadataItem(MetadataKeys.OverriddenMethod, DocumentFor(symbol.OverriddenEvent)),
                    new MetadataItem(MetadataKeys.Accessibility, (k, m) => symbol.DeclaredAccessibility.ToString())
                });
            }
        }

        public override void VisitProperty(IPropertySymbol symbol)
        {
            if (_finished || _symbolPredicate == null || _symbolPredicate(symbol))
            {
                AddDocumentForMember(symbol, true, new MetadataItems
                {
                    new MetadataItem(MetadataKeys.SpecificKind, (k, m) => symbol.Kind.ToString()),
                    new MetadataItem(MetadataKeys.Parameters, DocumentsFor(symbol.Parameters)),
                    new MetadataItem(MetadataKeys.Type, DocumentFor(symbol.Type)),
                    new MetadataItem(MetadataKeys.OverriddenMethod, DocumentFor(symbol.OverriddenProperty)),
                    new MetadataItem(MetadataKeys.Accessibility, (k, m) => symbol.DeclaredAccessibility.ToString())
                });
            }
        }

        // Helpers below...

        private bool MemberPredicate(ISymbol symbol)
        {
            IPropertySymbol propertySymbol = symbol as IPropertySymbol;
            if (propertySymbol != null && propertySymbol.IsIndexer)
            {
                // Special case for indexers
                return true;
            }
            return symbol.CanBeReferencedByName && !symbol.IsImplicitlyDeclared;
        }

        private void AddDocumentForMember(ISymbol symbol, bool xmlDocumentation, MetadataItems items)
        {
            items.AddRange(new[]
            {
                new MetadataItem(MetadataKeys.ContainingType, DocumentFor(symbol.ContainingType))
            });
            AddDocument(symbol, xmlDocumentation, items);
        }

        private void AddDocument(ISymbol symbol, bool xmlDocumentation, MetadataItems items)
        {
            // Get universal metadata
            items.AddRange(new []
            {
                // In general, cache the values that need calculation and don't cache the ones that are just properties of ISymbol
                new MetadataItem(MetadataKeys.IsResult, !_finished),
                new MetadataItem(MetadataKeys.SymbolId, (k, m) => GetId(symbol), true),
                new MetadataItem(MetadataKeys.Symbol, symbol),
                new MetadataItem(MetadataKeys.Name, (k, m) => symbol.Name),
                new MetadataItem(MetadataKeys.FullName, (k, m) => GetFullName(symbol), true),
                new MetadataItem(MetadataKeys.DisplayName, (k, m) => GetDisplayName(symbol), true),
                new MetadataItem(MetadataKeys.QualifiedName, (k, m) => GetQualifiedName(symbol), true),
                new MetadataItem(MetadataKeys.Kind, (k, m) => symbol.Kind.ToString()),
                new MetadataItem(MetadataKeys.ContainingNamespace, DocumentFor(symbol.ContainingNamespace)),
                new MetadataItem(MetadataKeys.Syntax, (k, m) => GetSyntax(symbol), true)
            });

            // Add metadata that's specific to initially-processed symbols
            if (!_finished)
            {
                items.AddRange(new[]
                {
                    new MetadataItem(MetadataKeys.WritePath, (k, m) => _writePath(m), true),
                    new MetadataItem(MetadataKeys.RelativeFilePath, (k, m) => m.String(MetadataKeys.WritePath)),
                    new MetadataItem(MetadataKeys.RelativeFilePathBase, (k, m) => PathHelper.RemoveExtension(m.String(MetadataKeys.WritePath))),
                    new MetadataItem(MetadataKeys.RelativeFileDir, (k, m) => Path.GetDirectoryName(m.String(MetadataKeys.WritePath)))
                });
            }

            // XML Documentation
            if (xmlDocumentation && (!_finished || _docsForImplicitSymbols))
            {
                AddXmlDocumentation(symbol, items);
            }

            // Create the document and add it to the cache
            IDocument document = _context.GetNewDocument(symbol.ToDisplayString(), null, items);
            _symbolToDocument.GetOrAdd(symbol, _ => document);

            // Map the comment ID to the document
            if (!_finished)
            {
                string documentationCommentId = symbol.GetDocumentationCommentId();
                if (documentationCommentId != null)
                {
                    _commentIdToDocument.GetOrAdd(documentationCommentId, _ => document);
                }
            }
        }

        private void AddXmlDocumentation(ISymbol symbol, MetadataItems metadata)
        {
            string documentationCommentXml = symbol.GetDocumentationCommentXml(expandIncludes: true);
            XmlDocumentationParser xmlDocumentationParser
                = new XmlDocumentationParser(symbol, _commentIdToDocument, _cssClasses, _context.Trace);
            IEnumerable<string> otherHtmlElementNames = xmlDocumentationParser.Parse(documentationCommentXml);

            // Add standard HTML elements
            metadata.AddRange(new []
            {
                new MetadataItem(MetadataKeys.CommentXml, documentationCommentXml),
                new MetadataItem(MetadataKeys.Example, (k, m) => xmlDocumentationParser.Process().Example),
                new MetadataItem(MetadataKeys.Remarks, (k, m) => xmlDocumentationParser.Process().Remarks),
                new MetadataItem(MetadataKeys.Summary, (k, m) => xmlDocumentationParser.Process().Summary),
                new MetadataItem(MetadataKeys.Returns, (k, m) => xmlDocumentationParser.Process().Returns),
                new MetadataItem(MetadataKeys.Value, (k, m) => xmlDocumentationParser.Process().Value),
                new MetadataItem(MetadataKeys.Exceptions, (k, m) => xmlDocumentationParser.Process().Exceptions),
                new MetadataItem(MetadataKeys.Permissions, (k, m) => xmlDocumentationParser.Process().Permissions),
                new MetadataItem(MetadataKeys.Params, (k, m) => xmlDocumentationParser.Process().Params),
                new MetadataItem(MetadataKeys.TypeParams, (k, m) => xmlDocumentationParser.Process().TypeParams),
                new MetadataItem(MetadataKeys.SeeAlso, (k, m) => xmlDocumentationParser.Process().SeeAlso)
            });

            // Add other HTML elements with keys of [ElementName]Html
            metadata.AddRange(otherHtmlElementNames.Select(x => 
                new MetadataItem(FirstLetterToUpper(x) + "Comments",
                    (k, m) => xmlDocumentationParser.Process().OtherComments[x])));
        }

        public static string FirstLetterToUpper(string str)
        {
            if (str == null)
            {
                return null;
            }

            if (str.Length > 1)
            {
                return char.ToUpper(str[0]) + str.Substring(1);
            }

            return str.ToUpper();
        }

        private static string GetId(ISymbol symbol)
        {
            return BitConverter.ToString(BitConverter.GetBytes(Crc32.Calculate(symbol.GetDocumentationCommentId() ?? GetFullName(symbol)))).Replace("-", string.Empty);
        }

        private static string GetFullName(ISymbol symbol)
        {
            return symbol.ToDisplayString(new SymbolDisplayFormat(
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypes,
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                parameterOptions: SymbolDisplayParameterOptions.IncludeType, 
                memberOptions: SymbolDisplayMemberOptions.IncludeParameters,
                miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes));
        }

        private static string GetQualifiedName(ISymbol symbol)
        {
            return symbol.ToDisplayString(new SymbolDisplayFormat(
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters));
        }

        private static string GetDisplayName(ISymbol symbol)
        {
            if (symbol.Kind == SymbolKind.Namespace)
            {
                // Use "global" for the global namespace display name since it's a reserved keyword and it's used to refer to the global namespace in code
                return symbol.ContainingNamespace == null ? "global" : GetQualifiedName(symbol);
            }
            return GetFullName(symbol);
        }

        private IReadOnlyList<IDocument> GetDerivedTypes(INamedTypeSymbol symbol)
        {
            return _namedTypes
                .Where(x => x.Key.BaseType != null && x.Key.BaseType.Equals(symbol))
                .Select(x => x.Value)
                .ToImmutableArray();
        }

        private IReadOnlyList<IDocument> GetImplementingTypes(INamedTypeSymbol symbol)
        {
            return _namedTypes
                .Where(x => x.Key.AllInterfaces.Contains(symbol))
                .Select(x => x.Value)
                .ToImmutableArray();
        }

        private string GetSyntax(ISymbol symbol)
        {
            return SyntaxHelper.GetSyntax(symbol);
        }

        private SymbolDocumentValue DocumentFor(ISymbol symbol)
        {
            return new SymbolDocumentValue(symbol, this);
        }

        private SymbolDocumentValues DocumentsFor(IEnumerable<ISymbol> symbols)
        {
            return new SymbolDocumentValues(symbols, this);
        }
    }
}