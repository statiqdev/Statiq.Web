using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.Meta;
using Wyam.Core.Pipelines;

namespace Wyam.Core.Documents
{
    internal class DocumentFactory : IDocumentFactory
    {
        private readonly IInitialMetadata _initialMetadata;

        public DocumentFactory(IInitialMetadata initialMetadata)
        {
            _initialMetadata = initialMetadata;
        }

        public IDocument GetDocument()
        {
            return new Document(_initialMetadata, string.Empty, null, null, null, true);
        }

        public IDocument GetDocument(IDocument sourceDocument, string source, string content, IEnumerable<KeyValuePair<string, object>> items = null)
        {
            // TODO: Add check for AsNewDocuments() in this condition...
            if (sourceDocument == null)
            {
                return new Document(_initialMetadata, source, null, content, items, true);
            }
            return ((Document) sourceDocument).Clone(source, content, items);
        }

        public IDocument GetDocument(IDocument sourceDocument, string content, IEnumerable<KeyValuePair<string, object>> items = null)
        {
            if (sourceDocument == null)
            {
                return new Document(_initialMetadata, string.Empty, null, content, items, true);
            }
            return ((Document)sourceDocument).Clone(content, items);
        }

        public IDocument GetDocument(IDocument sourceDocument, string source, Stream stream, IEnumerable<KeyValuePair<string, object>> items = null, bool disposeStream = true)
        {
            if (sourceDocument == null)
            {
                return new Document(_initialMetadata, source, stream, null, items, disposeStream);
            }
            return ((Document)sourceDocument).Clone(source, stream, items, disposeStream);
        }

        public IDocument GetDocument(IDocument sourceDocument, Stream stream, IEnumerable<KeyValuePair<string, object>> items = null, bool disposeStream = true)
        {
            if (sourceDocument == null)
            {
                return new Document(_initialMetadata, string.Empty, stream, null, items, disposeStream);
            }
            return ((Document)sourceDocument).Clone(stream, items, disposeStream);
        }

        public IDocument GetDocument(IDocument sourceDocument, IEnumerable<KeyValuePair<string, object>> items)
        {
            if (sourceDocument == null)
            {
                return new Document(_initialMetadata, string.Empty, null, null, items, true);
            }
            return ((Document)sourceDocument).Clone(items);
        }
    }
}
