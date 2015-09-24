using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Wyam.Common;

namespace Wyam.Modules.CodeAnalysis
{
    internal class SymbolDocumentValues : IMetadataValue
    {
        private readonly ConcurrentDictionary<ISymbol, IDocument> _documents;
        private readonly IEnumerable<ISymbol> _symbols;
        private bool _cached;
        private ImmutableArray<IDocument> _values;

        public SymbolDocumentValues(ConcurrentDictionary<ISymbol, IDocument> documents, IEnumerable<ISymbol> symbols)
        {
            _documents = documents;
            _symbols = symbols;
        }

        public object Get(string key, IMetadata metadata)
        {
            if (!_cached)
            {
                _values = _symbols
                    .Select(x =>
                    {
                        IDocument document;
                        return !_documents.TryGetValue(x, out document) ? null : document;
                    })
                    .Where(x => x != null)
                    .ToImmutableArray();
                _cached = true;
            }
            return _values;
        }
    }
}