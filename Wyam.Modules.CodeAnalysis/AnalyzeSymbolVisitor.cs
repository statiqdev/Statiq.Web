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
            AddDocument(symbol, new[]
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
            AddDocument(symbol, new[]
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

        private void AddDocument(ISymbol symbol, IEnumerable<KeyValuePair<string, object>> items)
        {
            IDocument document = _context.GetNewDocument(symbol.ToDisplayString(), null, items.Concat(new[]
            {
                MetadataHelper.New(MetadataKeys.SymbolId, GetId(symbol)),
                MetadataHelper.New(MetadataKeys.Symbol, symbol),
                MetadataHelper.New(MetadataKeys.Name, (k, m) => symbol.Name),
                MetadataHelper.New(MetadataKeys.DisplayString, (k, m) => symbol.ToDisplayString()),
                MetadataHelper.New(MetadataKeys.Kind, (k, m) => symbol.Kind.ToString())
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
            return BitConverter.ToString(BitConverter.GetBytes(Crc32.Calculate(symbol.GetDocumentationCommentId() ?? symbol.ToDisplayString()))).Replace("-", string.Empty);
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