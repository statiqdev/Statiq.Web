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
        private readonly ConcurrentDictionary<ISymbol, IDocument> _documents = new ConcurrentDictionary<ISymbol, IDocument>();
        private readonly IExecutionContext _context;
        private readonly List<KeyValuePair<string, ConfigHelper<object>>> _withMetadata;

        public AnalyzeSymbolVisitor(IExecutionContext context, List<KeyValuePair<string, ConfigHelper<object>>> withMetadata)
        {
            _context = context;
            _withMetadata = withMetadata;
        }

        public IEnumerable<IDocument> GetNamespaceOrTypeDocuments()
        {
            return _documents
                .Where(x => x.Key is INamespaceOrTypeSymbol)
                .Select(x => x.Value);
        }

        public override void VisitNamespace(INamespaceSymbol symbol)
        {
            AddDocument(symbol, string.Empty, new[]
            {
                MetadataHelper.New(MetadataKeys.SpecificKind, (k, m) => symbol.Kind.ToString()),
                MetadataHelper.New(MetadataKeys.ContainingNamespace, Document(symbol.ContainingNamespace)),
                MetadataHelper.New(MetadataKeys.MemberNamespaces, Documents(symbol.GetNamespaceMembers())),
                MetadataHelper.New(MetadataKeys.MemberTypes, Documents(symbol.GetTypeMembers()))
            });
            Parallel.ForEach(symbol.GetMembers(), s => s.Accept(this));
        }

        public override void VisitNamedType(INamedTypeSymbol symbol)
        {
            AddDocument(symbol, "DOCS GO HERE", new[]
            {
                MetadataHelper.New(MetadataKeys.SpecificKind, (k, m) => symbol.TypeKind.ToString()),
                MetadataHelper.New(MetadataKeys.ContainingNamespace, Document(symbol.ContainingNamespace)),
                MetadataHelper.New(MetadataKeys.ContainingType, Document(symbol.ContainingType)),
                MetadataHelper.New(MetadataKeys.MemberTypes, Documents(symbol.GetTypeMembers())),
                MetadataHelper.New(MetadataKeys.BaseType, Document(symbol.BaseType)),
                MetadataHelper.New(MetadataKeys.AllInterfaces, Documents(symbol.AllInterfaces))
            });
            Parallel.ForEach(symbol.GetMembers().Where(x => x.CanBeReferencedByName), s => s.Accept(this));
        }

        private void AddDocument(ISymbol symbol, string documentation, IEnumerable<KeyValuePair<string, object>> items)
        {
            IDocument document = _context.GetNewDocument(symbol.ToDisplayString(), null, items.Concat(new[]
            {
                // In general, cache the values that need calculation and don't cache the ones that are just properties of ISymbol
                MetadataHelper.New(MetadataKeys.SymbolId, (k, m) => GetId(symbol), true),
                MetadataHelper.New(MetadataKeys.Symbol, symbol),
                MetadataHelper.New(MetadataKeys.Name, (k, m) => symbol.Name),
                MetadataHelper.New(MetadataKeys.FullName, (k, m) => GetFullName(symbol), true),
                MetadataHelper.New(MetadataKeys.DisplayName, (k, m) => GetDisplayName(symbol), true),
                MetadataHelper.New(MetadataKeys.QualifiedName, (k, m) => GetQualifiedName(symbol), true),
                MetadataHelper.New(MetadataKeys.Kind, (k, m) => symbol.Kind.ToString()),
                MetadataHelper.New(MetadataKeys.DocumentationCommentXml, (k, m) => symbol.GetDocumentationCommentXml(), true),
                MetadataHelper.New(MetadataKeys.Documentation, documentation)
            }));
            if (_withMetadata.Count > 0)
            {
                foreach (KeyValuePair<string, ConfigHelper<object>> withMetadata in _withMetadata)
                {
                    document = document.Clone(new [] { MetadataHelper.New(withMetadata.Key, withMetadata.Value.GetValue(document, _context)) });
                }
            }
            _documents.GetOrAdd(symbol, _ => document);
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
            return new SymbolDocumentValue(_documents, symbol);
        }

        private SymbolDocumentValues Documents(IEnumerable<ISymbol> symbols)
        {
            return new SymbolDocumentValues(_documents, symbols);
        }
    }
}