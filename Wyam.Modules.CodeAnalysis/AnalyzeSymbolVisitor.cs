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
using Wyam.Common.Pipelines;
using Metadata = Wyam.Common.Documents.Metadata;

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
                AddDocument(symbol, true, new[]
                {
                    Metadata.Create(MetadataKeys.SpecificKind, (k, m) => symbol.Kind.ToString()),
                    Metadata.Create(MetadataKeys.MemberNamespaces, DocumentsFor(symbol.GetNamespaceMembers())),
                    Metadata.Create(MetadataKeys.MemberTypes, DocumentsFor(symbol.GetTypeMembers()))
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
                List<KeyValuePair<string, object>> metadata = new List<KeyValuePair<string, object>>
                {
                    Metadata.Create(MetadataKeys.SpecificKind, (k, m) => symbol.TypeKind.ToString()),
                    Metadata.Create(MetadataKeys.ContainingType, DocumentFor(symbol.ContainingType)),
                    Metadata.Create(MetadataKeys.MemberTypes, DocumentsFor(symbol.GetTypeMembers())),
                    Metadata.Create(MetadataKeys.BaseType, DocumentFor(symbol.BaseType)),
                    Metadata.Create(MetadataKeys.AllInterfaces, DocumentsFor(symbol.AllInterfaces)),
                    Metadata.Create(MetadataKeys.Members, DocumentsFor(symbol.GetMembers().Where(MemberPredicate))),
                    Metadata.Create(MetadataKeys.Constructors,
                        DocumentsFor(symbol.Constructors.Where(x => !x.IsImplicitlyDeclared))),
                    Metadata.Create(MetadataKeys.TypeParameters, DocumentsFor(symbol.TypeParameters)),
                    Metadata.Create(MetadataKeys.Accessibility, (k, m) => symbol.DeclaredAccessibility.ToString())
                };
                if (!_finished)
                {
                    metadata.AddRange(new[]
                    {
                        Metadata.Create(MetadataKeys.DerivedTypes, (k, m) => GetDerivedTypes(symbol), true),
                        Metadata.Create(MetadataKeys.ImplementingTypes, (k, m) => GetImplementingTypes(symbol), true)
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
                AddDocumentForMember(symbol, false, new[]
                {
                    Metadata.Create(MetadataKeys.SpecificKind, (k, m) => symbol.TypeParameterKind.ToString()),
                    Metadata.Create(MetadataKeys.DeclaringType, DocumentFor(symbol.DeclaringType))
                });
            }
        }

        public override void VisitParameter(IParameterSymbol symbol)
        {
            if (_finished || _symbolPredicate == null || _symbolPredicate(symbol))
            {
                AddDocumentForMember(symbol, false, new[]
                {
                    Metadata.Create(MetadataKeys.SpecificKind, (k, m) => symbol.Kind.ToString()),
                    Metadata.Create(MetadataKeys.Type, DocumentFor(symbol.Type))
                });
            }
        }

        public override void VisitMethod(IMethodSymbol symbol)
        {
            if (_finished || _symbolPredicate == null || _symbolPredicate(symbol))
            {
                AddDocumentForMember(symbol, true, new[]
                {
                    Metadata.Create(MetadataKeys.SpecificKind,
                        (k, m) => symbol.MethodKind == MethodKind.Ordinary ? "Method" : symbol.MethodKind.ToString()),
                    Metadata.Create(MetadataKeys.TypeParameters, DocumentsFor(symbol.TypeParameters)),
                    Metadata.Create(MetadataKeys.Parameters, DocumentsFor(symbol.Parameters)),
                    Metadata.Create(MetadataKeys.ReturnType, DocumentFor(symbol.ReturnType)),
                    Metadata.Create(MetadataKeys.Overridden, DocumentFor(symbol.OverriddenMethod)),
                    Metadata.Create(MetadataKeys.Accessibility, (k, m) => symbol.DeclaredAccessibility.ToString())
                });
            }
        }

        public override void VisitField(IFieldSymbol symbol)
        {
            if (_finished || _symbolPredicate == null || _symbolPredicate(symbol))
            {
                AddDocumentForMember(symbol, true, new[]
                {
                    Metadata.Create(MetadataKeys.SpecificKind, (k, m) => symbol.Kind.ToString()),
                    Metadata.Create(MetadataKeys.Type, DocumentFor(symbol.Type)),
                    Metadata.Create(MetadataKeys.Accessibility, (k, m) => symbol.DeclaredAccessibility.ToString())
                });
            }
        }

        public override void VisitEvent(IEventSymbol symbol)
        {
            if (_finished || _symbolPredicate == null || _symbolPredicate(symbol))
            {
                AddDocumentForMember(symbol, true, new[]
                {
                    Metadata.Create(MetadataKeys.SpecificKind, (k, m) => symbol.Kind.ToString()),
                    Metadata.Create(MetadataKeys.Type, DocumentFor(symbol.Type)),
                    Metadata.Create(MetadataKeys.Overridden, DocumentFor(symbol.OverriddenEvent)),
                    Metadata.Create(MetadataKeys.Accessibility, (k, m) => symbol.DeclaredAccessibility.ToString())
                });
            }
        }

        public override void VisitProperty(IPropertySymbol symbol)
        {
            if (_finished || _symbolPredicate == null || _symbolPredicate(symbol))
            {
                AddDocumentForMember(symbol, true, new[]
                {
                    Metadata.Create(MetadataKeys.SpecificKind, (k, m) => symbol.Kind.ToString()),
                    Metadata.Create(MetadataKeys.Parameters, DocumentsFor(symbol.Parameters)),
                    Metadata.Create(MetadataKeys.Type, DocumentFor(symbol.Type)),
                    Metadata.Create(MetadataKeys.Overridden, DocumentFor(symbol.OverriddenProperty)),
                    Metadata.Create(MetadataKeys.Accessibility, (k, m) => symbol.DeclaredAccessibility.ToString())
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

        private void AddDocumentForMember(ISymbol symbol, bool xmlDocumentation, IEnumerable<KeyValuePair<string, object>> items)
        {
            AddDocument(symbol, xmlDocumentation, items.Concat(new[]
            {
                Metadata.Create(MetadataKeys.ContainingType, DocumentFor(symbol.ContainingType))
            }));
        }

        private void AddDocument(ISymbol symbol, bool xmlDocumentation, IEnumerable<KeyValuePair<string, object>> items)
        {
            // Get universal metadata
            List<KeyValuePair<string, object>> metadata = new List<KeyValuePair<string, object>>
            {
                // In general, cache the values that need calculation and don't cache the ones that are just properties of ISymbol
                Metadata.Create(MetadataKeys.IsResult, !_finished),
                Metadata.Create(MetadataKeys.SymbolId, (k, m) => GetId(symbol), true),
                Metadata.Create(MetadataKeys.Symbol, symbol),
                Metadata.Create(MetadataKeys.Name, (k, m) => symbol.Name),
                Metadata.Create(MetadataKeys.FullName, (k, m) => GetFullName(symbol), true),
                Metadata.Create(MetadataKeys.DisplayName, (k, m) => GetDisplayName(symbol), true),
                Metadata.Create(MetadataKeys.QualifiedName, (k, m) => GetQualifiedName(symbol), true),
                Metadata.Create(MetadataKeys.Kind, (k, m) => symbol.Kind.ToString()),
                Metadata.Create(MetadataKeys.ContainingNamespace, DocumentFor(symbol.ContainingNamespace)),
                Metadata.Create(MetadataKeys.Syntax, (k, m) => GetSyntax(symbol), true)
            };

            // Add metadata that's specific to initially-processed symbols
            if (!_finished)
            {
                metadata.AddRange(new[]
                {
                    Metadata.Create(MetadataKeys.WritePath, (k, m) => _writePath(m), true),
                    Metadata.Create(MetadataKeys.RelativeFilePath, (k, m) => m.String(MetadataKeys.WritePath)),
                    Metadata.Create(MetadataKeys.RelativeFilePathBase, (k, m) => PathHelper.RemoveExtension(m.String(MetadataKeys.WritePath))),
                    Metadata.Create(MetadataKeys.RelativeFileDir, (k, m) => Path.GetDirectoryName(m.String(MetadataKeys.WritePath)))
                });
            }

            // XML Documentation
            if (xmlDocumentation && (!_finished || _docsForImplicitSymbols))
            {
                AddXmlDocumentation(symbol, metadata);
            }

            // Create the document and add it to the cache
            IDocument document = _context.GetNewDocument(symbol.ToDisplayString(), null, items.Concat(metadata));
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

        private void AddXmlDocumentation(ISymbol symbol, List<KeyValuePair<string, object>> metadata)
        {
            string documentationCommentXml = symbol.GetDocumentationCommentXml(expandIncludes: true);
            XmlDocumentationParser xmlDocumentationParser
                = new XmlDocumentationParser(symbol, _commentIdToDocument, _cssClasses, _context.Trace);
            IEnumerable<string> otherHtmlElementNames = xmlDocumentationParser.Parse(documentationCommentXml);

            // Add standard HTML elements
            metadata.AddRange(new []
            {
                Metadata.Create(MetadataKeys.CommentXml, documentationCommentXml),
                Metadata.Create(MetadataKeys.Example, (k, m) => xmlDocumentationParser.Process().Example),
                Metadata.Create(MetadataKeys.Remarks, (k, m) => xmlDocumentationParser.Process().Remarks),
                Metadata.Create(MetadataKeys.Summary, (k, m) => xmlDocumentationParser.Process().Summary),
                Metadata.Create(MetadataKeys.Returns, (k, m) => xmlDocumentationParser.Process().Returns),
                Metadata.Create(MetadataKeys.Value, (k, m) => xmlDocumentationParser.Process().Value),
                Metadata.Create(MetadataKeys.Exceptions, (k, m) => xmlDocumentationParser.Process().Exceptions),
                Metadata.Create(MetadataKeys.Permissions, (k, m) => xmlDocumentationParser.Process().Permissions),
                Metadata.Create(MetadataKeys.Params, (k, m) => xmlDocumentationParser.Process().Params),
                Metadata.Create(MetadataKeys.TypeParams, (k, m) => xmlDocumentationParser.Process().TypeParams),
                Metadata.Create(MetadataKeys.SeeAlso, (k, m) => xmlDocumentationParser.Process().SeeAlso)
            });

            // Add other HTML elements with keys of [ElementName]Html
            metadata.AddRange(otherHtmlElementNames.Select(x => 
                Metadata.Create(FirstLetterToUpper(x) + "Comments",
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