using System.Collections.Concurrent;
using Microsoft.CodeAnalysis;
using Wyam.Common;
using Wyam.Common.Documents;
using Wyam.Common.Meta;

namespace Wyam.Modules.CodeAnalysis
{
    internal class SymbolDocumentValue : IMetadataValue
    {
        private readonly ISymbol _symbol;
        private readonly AnalyzeSymbolVisitor _visitor;
        private bool _cached;
        private IDocument _value;

        public SymbolDocumentValue(ISymbol symbol, AnalyzeSymbolVisitor visitor)
        {
            _symbol = symbol;
            _visitor = visitor;
        }

        public object Get(string key, IMetadata metadata)
        {
            if (!_cached)
            {
                if (_symbol == null)
                {
                    _value = null;
                }
                else if (!_visitor.TryGetDocument(_symbol, out _value))
                {
                    // Visit the symbol and try again
                    _symbol.Accept(_visitor);
                    if (!_visitor.TryGetDocument(_symbol, out _value))
                    {
                        _value = null;
                    }
                }
                _cached = true;
            }
            return _value;
        }
    }
}