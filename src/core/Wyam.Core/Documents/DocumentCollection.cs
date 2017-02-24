using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Wyam.Common;
using Wyam.Common.Documents;

namespace Wyam.Core.Documents
{
    internal class DocumentCollection : IDocumentCollection
    {
        private readonly Dictionary<string, IReadOnlyList<IDocument>> _documents
            = new Dictionary<string, IReadOnlyList<IDocument>>();

        public void Clear()
        {
            _documents.Clear();
        }

        public void Set(string pipeline, IReadOnlyList<IDocument> documents)
        {
            _documents[pipeline] = documents;
        }

        public IReadOnlyList<IDocument> Get(string pipeline)
        {
            IReadOnlyList<IDocument> documents;
            return _documents.TryGetValue(pipeline, out documents) ? documents : null;
        }

        // IDocumentCollection

        public IEnumerator<IDocument> GetEnumerator()
        {
            return _documents.SelectMany(x => x.Value).Distinct().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IReadOnlyDictionary<string, IEnumerable<IDocument>> ByPipeline()
        {
            return _documents.ToDictionary(x => x.Key, x => x.Value.Distinct());
        }

        public IEnumerable<IDocument> FromPipeline(string pipeline)
        {
            return _documents[pipeline].Distinct();
        }

        public IEnumerable<IDocument> ExceptPipeline(string pipeline)
        {
            return _documents.Where(x => x.Key != pipeline).SelectMany(x => x.Value).Distinct();
        }

        public IEnumerable<IDocument> this[string pipline] => FromPipeline(pipline);
    }
}
