using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Execution;

namespace Wyam.Core.Documents
{
    public class CustomDocumentFactory<T> : IDocumentFactory
        where T : CustomDocument, new()
    {
        private readonly IDocumentFactory _documentFactory;

        public CustomDocumentFactory(IDocumentFactory documentFactory)
        {
            _documentFactory = documentFactory;
        }

        public IDocument GetDocument(IExecutionContext context)
        {
            return GetCustomDocument(null, _documentFactory.GetDocument(context));
        }

        public IDocument GetDocument(
            IExecutionContext context,
            IDocument sourceDocument,
            FilePath source,
            IEnumerable<KeyValuePair<string, object>> items = null)
        {
            CustomDocument customDocument = (CustomDocument)sourceDocument;
            IDocument document = _documentFactory.GetDocument(context, customDocument?.Document, source, items);
            return GetCustomDocument(customDocument, document);
        }

        public IDocument GetDocument(
            IExecutionContext context,
            IDocument sourceDocument,
            string content,
            IEnumerable<KeyValuePair<string, object>> items = null)
        {
            CustomDocument customDocument = (CustomDocument)sourceDocument;
            IDocument document = _documentFactory.GetDocument(context, customDocument?.Document, content, items);
            return GetCustomDocument(customDocument, document);
        }

        public IDocument GetDocument(
            IExecutionContext context,
            IDocument sourceDocument,
            FilePath source,
            Stream stream,
            IEnumerable<KeyValuePair<string, object>> items = null,
            bool disposeStream = true)
        {
            CustomDocument customDocument = (CustomDocument)sourceDocument;
            IDocument document = _documentFactory.GetDocument(context, customDocument?.Document, source, stream, items, disposeStream);
            return GetCustomDocument(customDocument, document);
        }

        public IDocument GetDocument(
            IExecutionContext context,
            IDocument sourceDocument,
            Stream stream,
            IEnumerable<KeyValuePair<string, object>> items = null,
            bool disposeStream = true)
        {
            CustomDocument customDocument = (CustomDocument)sourceDocument;
            IDocument document = _documentFactory.GetDocument(context, customDocument?.Document, stream, items, disposeStream);
            return GetCustomDocument(customDocument, document);
        }

        public IDocument GetDocument(
            IExecutionContext context,
            IDocument sourceDocument,
            IEnumerable<KeyValuePair<string, object>> items)
        {
            CustomDocument customDocument = (CustomDocument)sourceDocument;
            IDocument document = _documentFactory.GetDocument(context, customDocument?.Document, items);
            return GetCustomDocument(customDocument, document);
        }

        private IDocument GetCustomDocument(CustomDocument customDocument, IDocument document)
        {
            CustomDocument newCustomDocument = customDocument == null
                ? Activator.CreateInstance<T>()
                : customDocument.Clone();
            if (newCustomDocument == null || newCustomDocument == customDocument)
            {
                throw new Exception("Custom document type must return new instance from Clone method");
            }
            newCustomDocument.Document = document;
            return newCustomDocument;
        }
    }
}