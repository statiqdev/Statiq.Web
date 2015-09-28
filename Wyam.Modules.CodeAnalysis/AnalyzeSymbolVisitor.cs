using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Wyam.Common;

namespace Wyam.Modules.CodeAnalysis
{
    internal class AnalyzeSymbolVisitor : SymbolVisitor
    {
        private readonly ConcurrentDictionary<ISymbol, IDocument> _symbolToDocument = new ConcurrentDictionary<ISymbol, IDocument>();
        private readonly ConcurrentDictionary<string, IDocument> _commentIdToDocument = new ConcurrentDictionary<string, IDocument>();
        private readonly IExecutionContext _context;
        private readonly Func<IMetadata, string> _writePath;
        private readonly ConcurrentDictionary<string, string> _cssClasses;

        public AnalyzeSymbolVisitor(IExecutionContext context, Func<IMetadata, string> writePath, ConcurrentDictionary<string, string> cssClasses)
        {
            _context = context;
            _writePath = writePath;
            _cssClasses = cssClasses;
        }

        public IEnumerable<IDocument> GetNamespaceOrTypeDocuments()
        {
            return _symbolToDocument
                .Where(x => x.Key is INamespaceOrTypeSymbol)
                .Select(x => x.Value);
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
            Parallel.ForEach(symbol.GetMembers(), s => s.Accept(this));
        }

        public override void VisitNamedType(INamedTypeSymbol symbol)
        {
            XmlDocumentationParser xmlDocumentationParser 
                = new XmlDocumentationParser(symbol, _commentIdToDocument, _cssClasses, _context.Trace);
            AddDocument(symbol, xmlDocumentationParser, new[]
            {
                MetadataHelper.New(MetadataKeys.SpecificKind, (k, m) => symbol.TypeKind.ToString()),
                MetadataHelper.New(MetadataKeys.ContainingType, Document(symbol.ContainingType)),
                MetadataHelper.New(MetadataKeys.MemberTypes, Documents(symbol.GetTypeMembers())),
                MetadataHelper.New(MetadataKeys.BaseType, Document(symbol.BaseType)),
                MetadataHelper.New(MetadataKeys.AllInterfaces, Documents(symbol.AllInterfaces)),
                MetadataHelper.New(MetadataKeys.Members, Documents(symbol.GetMembers()))
            });
            Parallel.ForEach(symbol.GetMembers().Where(x => x.CanBeReferencedByName), s => s.Accept(this));
        }

        public override void VisitMethod(IMethodSymbol symbol)
        {
            XmlDocumentationParser xmlDocumentationParser 
                = new XmlDocumentationParser(symbol, _commentIdToDocument, _cssClasses, _context.Trace);
            AddDocument(symbol, xmlDocumentationParser, new[]
            {
                MetadataHelper.New(MetadataKeys.SpecificKind, (k, m) => symbol.MethodKind.ToString()),
                MetadataHelper.New(MetadataKeys.ContainingType, Document(symbol.ContainingType)),
                MetadataHelper.New(MetadataKeys.ExceptionHtml, (k, m) => xmlDocumentationParser.GetExceptionHtml())
            });

        }

        // Helpers below...

        private void AddDocument(ISymbol symbol, XmlDocumentationParser xmlDocumentationParser, IEnumerable<KeyValuePair<string, object>> items)
        {
            IDocument document = _context.GetNewDocument(symbol.ToDisplayString(), null, items.Concat(new[]
            {
                // In general, cache the values that need calculation and don't cache the ones that are just properties of ISymbol
                MetadataHelper.New(MetadataKeys.WritePath, (k, m) => _writePath(m), true),
                MetadataHelper.New(MetadataKeys.SymbolId, (k, m) => GetId(symbol), true),
                MetadataHelper.New(MetadataKeys.Symbol, symbol),
                MetadataHelper.New(MetadataKeys.Name, (k, m) => symbol.Name),
                MetadataHelper.New(MetadataKeys.FullName, (k, m) => GetFullName(symbol), true),
                MetadataHelper.New(MetadataKeys.DisplayName, (k, m) => GetDisplayName(symbol), true),
                MetadataHelper.New(MetadataKeys.QualifiedName, (k, m) => GetQualifiedName(symbol), true),
                MetadataHelper.New(MetadataKeys.Kind, (k, m) => symbol.Kind.ToString()),
                MetadataHelper.New(MetadataKeys.ContainingNamespace, Document(symbol.ContainingNamespace)),
                MetadataHelper.New(MetadataKeys.DocumentationCommentXml, (k, m) => symbol.GetDocumentationCommentXml(expandIncludes: true), true),
                MetadataHelper.New(MetadataKeys.ExampleHtml, (k, m) => xmlDocumentationParser.GetExampleHtml()),
                MetadataHelper.New(MetadataKeys.RemarksHtml, (k, m) => xmlDocumentationParser.GetRemarksHtml()),
                MetadataHelper.New(MetadataKeys.SummaryHtml, (k, m) => xmlDocumentationParser.GetSummaryHtml())
            }));
            _symbolToDocument.GetOrAdd(symbol, _ => document);
            string documentationCommentId = symbol.GetDocumentationCommentId();
            if (documentationCommentId != null)
            {
                _commentIdToDocument.GetOrAdd(documentationCommentId, _ => document);
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
                return GetQualifiedName(symbol);
            }
            return GetFullName(symbol);
        }

        private SymbolDocumentValue Document(ISymbol symbol)
        {
            return new SymbolDocumentValue(_symbolToDocument, symbol);
        }

        private SymbolDocumentValues Documents(IEnumerable<ISymbol> symbols)
        {
            return new SymbolDocumentValues(_symbolToDocument, symbols);
        }
    }
}