using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Wyam.Common;
using Metadata = Wyam.Common.Metadata;

namespace Wyam.Modules.CodeAnalysis
{
    internal class AnalyzeSymbolVisitor : SymbolVisitor
    {
        private readonly ConcurrentDictionary<ISymbol, IDocument> _symbolToDocument = new ConcurrentDictionary<ISymbol, IDocument>();
        private readonly ConcurrentDictionary<string, IDocument> _commentIdToDocument = new ConcurrentDictionary<string, IDocument>();
        private ImmutableArray<KeyValuePair<INamedTypeSymbol, IDocument>> _namedTypes;  // This contains all of the NamedType symbols and documents obtained during the initial processing
        private readonly IExecutionContext _context;
        private readonly Func<IMetadata, string> _writePath;
        private readonly ConcurrentDictionary<string, string> _cssClasses;
        private bool _finished; // When this is true, we're visiting external symbols and should omit certain metadata and don't descend

        public AnalyzeSymbolVisitor(IExecutionContext context, 
            Func<IMetadata, string> writePath, ConcurrentDictionary<string, string> cssClasses)
        {
            _context = context;
            _writePath = writePath;
            _cssClasses = cssClasses;
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
            XmlDocumentationParser xmlDocumentationParser 
                = new XmlDocumentationParser(symbol, _commentIdToDocument, _cssClasses, _context.Trace);
            AddDocument(symbol, xmlDocumentationParser, new[]
            {
                Metadata.Create(MetadataKeys.SpecificKind, (k, m) => symbol.Kind.ToString()),
                Metadata.Create(MetadataKeys.MemberNamespaces, DocumentsFor(symbol.GetNamespaceMembers())),
                Metadata.Create(MetadataKeys.MemberTypes, DocumentsFor(symbol.GetTypeMembers()))
            });
            if (!_finished)
            {
                Parallel.ForEach(symbol.GetMembers(), s => s.Accept(this));
            }
        }

        public override void VisitNamedType(INamedTypeSymbol symbol)
        {
            XmlDocumentationParser xmlDocumentationParser 
                = new XmlDocumentationParser(symbol, _commentIdToDocument, _cssClasses, _context.Trace);
            List<KeyValuePair<string, object>> metadata = new List<KeyValuePair<string, object>>
            {
                Metadata.Create(MetadataKeys.SpecificKind, (k, m) => symbol.TypeKind.ToString()),
                Metadata.Create(MetadataKeys.ContainingType, DocumentFor(symbol.ContainingType)),
                Metadata.Create(MetadataKeys.MemberTypes, DocumentsFor(symbol.GetTypeMembers())),
                Metadata.Create(MetadataKeys.BaseType, DocumentFor(symbol.BaseType)),
                Metadata.Create(MetadataKeys.AllInterfaces, DocumentsFor(symbol.AllInterfaces)),
                Metadata.Create(MetadataKeys.Members, DocumentsFor(symbol.GetMembers().Where(x => x.CanBeReferencedByName && !x.IsImplicitlyDeclared))),
                Metadata.Create(MetadataKeys.Constructors, DocumentsFor(symbol.Constructors.Where(x => !x.IsImplicitlyDeclared))),
                Metadata.Create(MetadataKeys.TypeParams, DocumentsFor(symbol.TypeParameters))
            };
            if (!_finished)
            {
                metadata.AddRange(new[]
                {
                    Metadata.Create(MetadataKeys.DerivedTypes, (k, m) => GetDerivedTypes(symbol), true),
                    Metadata.Create(MetadataKeys.ImplementingTypes, (k, m) => GetImplementingTypes(symbol), true)
                });
            }
            AddDocument(symbol, xmlDocumentationParser, metadata);
            if (!_finished)
            {
                Parallel.ForEach(symbol.GetMembers()
                    .Where(x => x.CanBeReferencedByName && !x.IsImplicitlyDeclared)
                    .Concat(symbol.Constructors.Where(x => !x.IsImplicitlyDeclared)), 
                    s => s.Accept(this));
            }
        }

        public override void VisitTypeParameter(ITypeParameterSymbol symbol)
        {
            AddDocumentForMember(symbol, null, new[]
            {
                Metadata.Create(MetadataKeys.SpecificKind, (k, m) => symbol.TypeParameterKind.ToString()),
                Metadata.Create(MetadataKeys.DeclaringType, DocumentFor(symbol.DeclaringType))
            });
        }

        public override void VisitParameter(IParameterSymbol symbol)
        {
            AddDocumentForMember(symbol, null, new[]
            {
                Metadata.Create(MetadataKeys.SpecificKind, (k, m) => symbol.Kind.ToString()),
                Metadata.Create(MetadataKeys.Type, DocumentFor(symbol.Type))
            });
        }

        public override void VisitMethod(IMethodSymbol symbol)
        {
            XmlDocumentationParser xmlDocumentationParser 
                = new XmlDocumentationParser(symbol, _commentIdToDocument, _cssClasses, _context.Trace);
            AddDocumentForMember(symbol, xmlDocumentationParser, new[]
            {
                Metadata.Create(MetadataKeys.SpecificKind, (k, m) => symbol.MethodKind == MethodKind.Ordinary ? "Method" : symbol.MethodKind.ToString()),
                Metadata.Create(MetadataKeys.TypeParams, DocumentsFor(symbol.TypeParameters)),
                Metadata.Create(MetadataKeys.Parameters, DocumentsFor(symbol.Parameters)),
                Metadata.Create(MetadataKeys.ReturnType, DocumentFor(symbol.ReturnType))
            });
        }

        public override void VisitField(IFieldSymbol symbol)
        {
            XmlDocumentationParser xmlDocumentationParser
                = new XmlDocumentationParser(symbol, _commentIdToDocument, _cssClasses, _context.Trace);
            AddDocumentForMember(symbol, xmlDocumentationParser, new[]
            {
                Metadata.Create(MetadataKeys.SpecificKind, (k, m) => symbol.Kind.ToString())
            });
        }

        public override void VisitEvent(IEventSymbol symbol)
        {
            XmlDocumentationParser xmlDocumentationParser
                = new XmlDocumentationParser(symbol, _commentIdToDocument, _cssClasses, _context.Trace);
            AddDocumentForMember(symbol, xmlDocumentationParser, new[]
            {
                Metadata.Create(MetadataKeys.SpecificKind, (k, m) => symbol.Kind.ToString())
            });
        }

        public override void VisitProperty(IPropertySymbol symbol)
        {
            XmlDocumentationParser xmlDocumentationParser
                = new XmlDocumentationParser(symbol, _commentIdToDocument, _cssClasses, _context.Trace);
            AddDocumentForMember(symbol, xmlDocumentationParser, new[]
            {
                Metadata.Create(MetadataKeys.SpecificKind, (k, m) => symbol.Kind.ToString()),
                Metadata.Create(MetadataKeys.Parameters, DocumentsFor(symbol.Parameters)),
                Metadata.Create(MetadataKeys.Type, DocumentFor(symbol.Type))
            });
        }

        // Helpers below...

        private void AddDocumentForMember(ISymbol symbol, XmlDocumentationParser xmlDocumentationParser, IEnumerable<KeyValuePair<string, object>> items)
        {
            AddDocument(symbol, xmlDocumentationParser, items.Concat(new[]
            {
                Metadata.Create(MetadataKeys.ContainingType, DocumentFor(symbol.ContainingType))
            }));
        }

        private void AddDocument(ISymbol symbol, XmlDocumentationParser xmlDocumentationParser, IEnumerable<KeyValuePair<string, object>> items)
        {
            // Get universal metadata
            List<KeyValuePair<string, object>> metadata = new List<KeyValuePair<string, object>>
            {
                // In general, cache the values that need calculation and don't cache the ones that are just properties of ISymbol
                Metadata.Create(MetadataKeys.SymbolId, (k, m) => GetId(symbol), true),
                Metadata.Create(MetadataKeys.Symbol, symbol),
                Metadata.Create(MetadataKeys.Name, (k, m) => symbol.Name),
                Metadata.Create(MetadataKeys.FullName, (k, m) => GetFullName(symbol), true),
                Metadata.Create(MetadataKeys.DisplayName, (k, m) => GetDisplayName(symbol), true),
                Metadata.Create(MetadataKeys.QualifiedName, (k, m) => GetQualifiedName(symbol), true),
                Metadata.Create(MetadataKeys.Kind, (k, m) => symbol.Kind.ToString()),
                Metadata.Create(MetadataKeys.ContainingNamespace, DocumentFor(symbol.ContainingNamespace))
            };

            // Add metadata that's specific to initially-processed symbols
            if (!_finished)
            {
                metadata.AddRange(new[]
                {
                    Metadata.Create(MetadataKeys.WritePath, (k, m) => _writePath(m), true),
                    Metadata.Create(MetadataKeys.Syntax, (k, m) => GetSyntax(symbol), true)
                });

                // XML Documentation
                if(xmlDocumentationParser != null)
                {
                    metadata.AddRange(new []
                    {
                        Metadata.Create(MetadataKeys.DocumentationCommentXml, (k, m) => symbol.GetDocumentationCommentXml(expandIncludes: true), true),
                        Metadata.Create(MetadataKeys.ExampleHtml, (k, m) => xmlDocumentationParser.GetExampleHtml()),
                        Metadata.Create(MetadataKeys.RemarksHtml, (k, m) => xmlDocumentationParser.GetRemarksHtml()),
                        Metadata.Create(MetadataKeys.SummaryHtml, (k, m) => xmlDocumentationParser.GetSummaryHtml()),
                        Metadata.Create(MetadataKeys.ReturnsHtml, (k, m) => xmlDocumentationParser.GetReturnsHtml()),
                        Metadata.Create(MetadataKeys.ValueHtml, (k, m) => xmlDocumentationParser.GetValueHtml()),
                        Metadata.Create(MetadataKeys.ExceptionHtml, (k, m) => xmlDocumentationParser.GetExceptionHtml()),
                        Metadata.Create(MetadataKeys.PermissionHtml, (k, m) => xmlDocumentationParser.GetPermissionHtml()),
                        Metadata.Create(MetadataKeys.ParamHtml, (k, m) => xmlDocumentationParser.GetParamHtml()),
                        Metadata.Create(MetadataKeys.TypeParamHtml, (k, m) => xmlDocumentationParser.GetTypeParamHtml()),
                        Metadata.Create(MetadataKeys.SeeAlsoHtml, (k, m) => xmlDocumentationParser.GetSeeAlsoHtml()),
                    });
                }
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