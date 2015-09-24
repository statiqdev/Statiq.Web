using System.Collections.Concurrent;
using Microsoft.CodeAnalysis;
using Wyam.Common;

namespace Wyam.Modules.CodeAnalysis
{
    internal class SymbolDocumentValue : IMetadataValue
    {
        private readonly ConcurrentDictionary<ISymbol, IDocument> _documents;
        private readonly ISymbol _symbol;
        private bool _cached;
        private IDocument _value;

        public SymbolDocumentValue(ConcurrentDictionary<ISymbol, IDocument> documents, ISymbol symbol)
        {
            _documents = documents;
            _symbol = symbol;
        }

        public object Get(string key, IMetadata metadata)
        {
            if (!_cached)
            {
                if (_symbol == null || !_documents.TryGetValue(_symbol, out _value))
                {
                    _value = null;
                }
                _cached = true;
            }
            return _value;
        }
    }
}