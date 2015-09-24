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

        public AnalyzeSymbolVisitor(IExecutionContext context)
        {
            _context = context;
        }

        public IEnumerable<IDocument> GetNamespaceOrTypeDocuments()
        {
            return _documents
                .Where(x => x.Key is INamespaceOrTypeSymbol)
                .Select(x => x.Value);
        }

        public override void VisitNamespace(INamespaceSymbol symbol)
        {
            _documents.GetOrAdd(symbol, _ => _context.GetNewDocument(new[]
            {
                MetadataHelper.New(MetadataKeys.Symbol, symbol),
                MetadataHelper.New(MetadataKeys.Name, (k, m) => symbol.Name),
                MetadataHelper.New(MetadataKeys.DisplayString, (k, m) => symbol.ToDisplayString()),
                MetadataHelper.New(MetadataKeys.Kind, (k, m) => symbol.Kind.ToString()),
                MetadataHelper.New(MetadataKeys.ContainingNamespace, Document(symbol.ContainingNamespace)),
                MetadataHelper.New(MetadataKeys.MemberNamespaces, Documents(symbol.GetNamespaceMembers())),
                MetadataHelper.New(MetadataKeys.MemberTypes, Documents(symbol.GetTypeMembers()))
            }));
            Parallel.ForEach(symbol.GetMembers(), s => s.Accept(this));
        }

        public override void VisitNamedType(INamedTypeSymbol symbol)
        {
            _documents.GetOrAdd(symbol, _ => _context.GetNewDocument(new[]
            {
                MetadataHelper.New(MetadataKeys.Symbol, symbol),
                MetadataHelper.New(MetadataKeys.Name, (k, m) => symbol.Name),
                MetadataHelper.New(MetadataKeys.DisplayString, (k, m) => symbol.ToDisplayString()),
                MetadataHelper.New(MetadataKeys.Kind, (k, m) => symbol.Kind.ToString()),
                MetadataHelper.New(MetadataKeys.ContainingNamespace, Document(symbol.ContainingNamespace)),
                MetadataHelper.New(MetadataKeys.ContainingType, Document(symbol.ContainingType)),
                MetadataHelper.New(MetadataKeys.MemberTypes, Documents(symbol.GetTypeMembers()))
            }));
            Parallel.ForEach(symbol.GetMembers().Where(x => x.CanBeReferencedByName), s => s.Accept(this));
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