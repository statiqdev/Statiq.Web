using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Wyam.Common;

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
                MetadataHelper.New(MetadataKeys.SpecificKind, (k, m) => symbol.Kind.ToString()),
                MetadataHelper.New(MetadataKeys.MemberNamespaces, Documents(symbol.GetNamespaceMembers())),
                MetadataHelper.New(MetadataKeys.MemberTypes, Documents(symbol.GetTypeMembers()))
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
                MetadataHelper.New(MetadataKeys.SpecificKind, (k, m) => symbol.TypeKind.ToString()),
                MetadataHelper.New(MetadataKeys.ContainingType, Document(symbol.ContainingType)),
                MetadataHelper.New(MetadataKeys.MemberTypes, Documents(symbol.GetTypeMembers())),
                MetadataHelper.New(MetadataKeys.BaseType, Document(symbol.BaseType)),
                MetadataHelper.New(MetadataKeys.AllInterfaces, Documents(symbol.AllInterfaces)),
                MetadataHelper.New(MetadataKeys.Members, Documents(symbol.GetMembers().Where(x => x.CanBeReferencedByName && !x.IsImplicitlyDeclared)))
            };
            if (!_finished)
            {
                metadata.AddRange(new[]
                {
                    MetadataHelper.New(MetadataKeys.DerivedTypes, (k, m) => GetDerivedTypes(symbol), true)
                });
            }
            AddDocument(symbol, xmlDocumentationParser, metadata);
            if (!_finished)
            {
                Parallel.ForEach(symbol.GetMembers().Where(x => x.CanBeReferencedByName && !x.IsImplicitlyDeclared), s => s.Accept(this));
            }
        }

        public override void VisitMethod(IMethodSymbol symbol)
        {
            XmlDocumentationParser xmlDocumentationParser 
                = new XmlDocumentationParser(symbol, _commentIdToDocument, _cssClasses, _context.Trace);
            AddDocumentForMember(symbol, xmlDocumentationParser, new[]
            {
                MetadataHelper.New(MetadataKeys.SpecificKind, (k, m) => symbol.MethodKind.ToString())
            });
        }

        public override void VisitField(IFieldSymbol symbol)
        {
            XmlDocumentationParser xmlDocumentationParser
                = new XmlDocumentationParser(symbol, _commentIdToDocument, _cssClasses, _context.Trace);
            AddDocumentForMember(symbol, xmlDocumentationParser, new[]
            {
                MetadataHelper.New(MetadataKeys.SpecificKind, (k, m) => symbol.Kind.ToString())
            });
        }

        public override void VisitEvent(IEventSymbol symbol)
        {
            XmlDocumentationParser xmlDocumentationParser
                = new XmlDocumentationParser(symbol, _commentIdToDocument, _cssClasses, _context.Trace);
            AddDocumentForMember(symbol, xmlDocumentationParser, new[]
            {
                MetadataHelper.New(MetadataKeys.SpecificKind, (k, m) => symbol.Kind.ToString())
            });
        }

        public override void VisitProperty(IPropertySymbol symbol)
        {
            XmlDocumentationParser xmlDocumentationParser
                = new XmlDocumentationParser(symbol, _commentIdToDocument, _cssClasses, _context.Trace);
            AddDocumentForMember(symbol, xmlDocumentationParser, new[]
            {
                MetadataHelper.New(MetadataKeys.SpecificKind, (k, m) => symbol.Kind.ToString())
            });
        }

        // Helpers below...

        private void AddDocumentForMember(ISymbol symbol, XmlDocumentationParser xmlDocumentationParser, IEnumerable<KeyValuePair<string, object>> items)
        {
            AddDocument(symbol, xmlDocumentationParser, items.Concat(new[]
            {
                MetadataHelper.New(MetadataKeys.ContainingType, Document(symbol.ContainingType))
            }));
        }

        private void AddDocument(ISymbol symbol, XmlDocumentationParser xmlDocumentationParser, IEnumerable<KeyValuePair<string, object>> items)
        {
            // Get universal metadata
            List<KeyValuePair<string, object>> metadata = new List<KeyValuePair<string, object>>
            {
                // In general, cache the values that need calculation and don't cache the ones that are just properties of ISymbol
                MetadataHelper.New(MetadataKeys.SymbolId, (k, m) => GetId(symbol), true),
                MetadataHelper.New(MetadataKeys.Symbol, symbol),
                MetadataHelper.New(MetadataKeys.Name, (k, m) => symbol.Name),
                MetadataHelper.New(MetadataKeys.FullName, (k, m) => GetFullName(symbol), true),
                MetadataHelper.New(MetadataKeys.DisplayName, (k, m) => GetDisplayName(symbol), true),
                MetadataHelper.New(MetadataKeys.QualifiedName, (k, m) => GetQualifiedName(symbol), true),
                MetadataHelper.New(MetadataKeys.Kind, (k, m) => symbol.Kind.ToString()),
                MetadataHelper.New(MetadataKeys.ContainingNamespace, Document(symbol.ContainingNamespace))
            };

            // Add metadata that's specific to initially-processed symbols
            if (!_finished)
            {
                metadata.AddRange(new[]
                {
                    MetadataHelper.New(MetadataKeys.WritePath, (k, m) => _writePath(m), true),

                    // XML Documentation
                    MetadataHelper.New(MetadataKeys.DocumentationCommentXml, (k, m) => symbol.GetDocumentationCommentXml(expandIncludes: true), true),
                    MetadataHelper.New(MetadataKeys.ExampleHtml, (k, m) => xmlDocumentationParser.GetExampleHtml()),
                    MetadataHelper.New(MetadataKeys.RemarksHtml, (k, m) => xmlDocumentationParser.GetRemarksHtml()),
                    MetadataHelper.New(MetadataKeys.SummaryHtml, (k, m) => xmlDocumentationParser.GetSummaryHtml()),
                    MetadataHelper.New(MetadataKeys.ReturnsHtml, (k, m) => xmlDocumentationParser.GetReturnsHtml()),
                    MetadataHelper.New(MetadataKeys.ValueHtml, (k, m) => xmlDocumentationParser.GetValueHtml()),
                    MetadataHelper.New(MetadataKeys.ExceptionHtml, (k, m) => xmlDocumentationParser.GetExceptionHtml()),
                    MetadataHelper.New(MetadataKeys.PermissionHtml, (k, m) => xmlDocumentationParser.GetPermissionHtml()),
                    MetadataHelper.New(MetadataKeys.ParamHtml, (k, m) => xmlDocumentationParser.GetParamHtml()),
                    MetadataHelper.New(MetadataKeys.TypeParamHtml, (k, m) => xmlDocumentationParser.GetTypeParamHtml()),
                    MetadataHelper.New(MetadataKeys.SeeAlsoHtml, (k, m) => xmlDocumentationParser.GetSeeAlsoHtml())
                });
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
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters));
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

        private SymbolDocumentValue Document(ISymbol symbol)
        {
            return new SymbolDocumentValue(symbol, this);
        }

        private SymbolDocumentValues Documents(IEnumerable<ISymbol> symbols)
        {
            return new SymbolDocumentValues(symbols, this);
        }
    }
}