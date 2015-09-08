using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Wyam.Common;

namespace Wyam.Core.Modules
{
    // This allows you to replace your pipeline with previously processed documents
    public class Documents : IModule
    {
        private readonly string _pipeline;
        private readonly ContextConfig _contextDocuments;
        private readonly DocumentConfig _documentDocuments;
        private DocumentConfig _predicate;

        // This will get all previously processed documents from all pipelines
        public Documents()
        {
        }

        // This will get documents from other pipelines
        public Documents(string pipeline)
        {
            _pipeline = pipeline;
        }

        // This will get documents based on the context - the delegate will only be called once, regardless of the number of input documents
        // The delegate should return a IEnumerable<IDocument>
        public Documents(ContextConfig documents)
        {
            if (documents == null)
            {
                throw new ArgumentNullException(nameof(documents));
            }
            _contextDocuments = documents;
        }

        // This will get documents based on each input document - the result will be the aggregate of all returned documents for each input document
        // The delegate should return a IEnumerable<IDocument>
        public Documents(DocumentConfig documents)
        {
            if (documents == null)
            {
                throw new ArgumentNullException(nameof(documents));
            }
            _documentDocuments = documents;
        }

        // Generates count number of new documents
        public Documents(int count)
        {
            _contextDocuments = ctx =>
            {
                List<IDocument> documents = new List<IDocument>();
                for (int c = 0; c < count; c++)
                {
                    documents.Add(ctx.GetNewDocument());
                }
                return documents;
            };
        }

        // Generates new documents with the defined content
        public Documents(params string[] content)
        {
            _contextDocuments = ctx => content.Select(x => ctx.GetNewDocument().Clone(x));
        }

        // Generates new documents with the defined metadata
        public Documents(params IEnumerable<KeyValuePair<string, object>>[] metadata)
        {
            _contextDocuments = ctx => metadata.Select(ctx.GetNewDocument);
        }

        // Generates new documents with the defined content and metadata
        public Documents(params Tuple<string, IEnumerable<KeyValuePair<string, object>>>[] contentAndMetadata)
        {
            _contextDocuments = ctx => contentAndMetadata.Select(x => ctx.GetNewDocument(x.Item2).Clone(x.Item1));
        }

        // The delegate should return a bool
        public Documents Where(DocumentConfig predicate)
        {
            _predicate = predicate;
            return this;
        }

        public virtual IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            IEnumerable<IDocument> documents;
            if (_documentDocuments != null)
            {
                documents = inputs.SelectMany(x => _documentDocuments.Invoke<IEnumerable<IDocument>>(x, context));
            }
            else if (_contextDocuments != null)
            {
                documents = _contextDocuments.Invoke<IEnumerable<IDocument>>(context);
            }
            else
            {
                documents = string.IsNullOrEmpty(_pipeline)
                    ? context.Documents.ExceptPipeline(context.Pipeline.Name)
                    : context.Documents.FromPipeline(_pipeline);
            }
            if (_predicate != null)
            {
                documents = documents.Where(x => _predicate.Invoke<bool>(x, context));
            }
            return documents;
        }
    }
}