using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.Pipelines;

namespace Wyam.Core.Documents
{
    public class CustomDocumentFactory<T> : IDocumentFactory where T : CustomDocument, new()
    {
        private readonly IDocumentFactory _documentFactory;

        public CustomDocumentFactory(IDocumentFactory documentFactory)
        {
            _documentFactory = documentFactory;
        }

        public IDocument GetDocument(IExecutionContext context) => 
            GetCustomDocument(_documentFactory.GetDocument(context));

        public IDocument GetDocument(IExecutionContext context, IDocument sourceDocument, string source, string content,
            IEnumerable<KeyValuePair<string, object>> items = null) => 
            GetCustomDocument(_documentFactory.GetDocument(context, sourceDocument, source, content, items));

        public IDocument GetDocument(IExecutionContext context, IDocument sourceDocument, 
            string content, IEnumerable<KeyValuePair<string, object>> items = null) => 
            GetCustomDocument(_documentFactory.GetDocument(context, sourceDocument, content, items));

        public IDocument GetDocument(IExecutionContext context, IDocument sourceDocument, string source, Stream stream,
            IEnumerable<KeyValuePair<string, object>> items = null, bool disposeStream = true) => 
            GetCustomDocument(_documentFactory.GetDocument(context, sourceDocument, source, stream, items, disposeStream));

        public IDocument GetDocument(IExecutionContext context, IDocument sourceDocument, Stream stream, 
            IEnumerable<KeyValuePair<string, object>> items = null, bool disposeStream = true) => 
            GetCustomDocument(_documentFactory.GetDocument(context, sourceDocument, stream, items, disposeStream));

        public IDocument GetDocument(IExecutionContext context, IDocument sourceDocument, 
            IEnumerable<KeyValuePair<string, object>> items) => 
            GetCustomDocument(_documentFactory.GetDocument(context, sourceDocument, items));

        private IDocument GetCustomDocument(IDocument document)
        {
            CustomDocument customDocument = Activator.CreateInstance<T>();
            customDocument.SetDocument(document);
            return customDocument;
        }
    }
}