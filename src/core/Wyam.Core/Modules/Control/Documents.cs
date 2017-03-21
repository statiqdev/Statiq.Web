using System;
using System.Collections.Generic;
using System.Linq;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Execution;
using Wyam.Common.Util;

namespace Wyam.Core.Modules.Control
{
    /// <summary>
    /// Inserts documents into the current pipeline.
    /// </summary>
    /// <remarks>
    /// Documents can be inserted either by replacing pipeline documents with previously
    /// processed ones or by creating new ones. If getting previously processed documents from another pipeline,
    /// this module copies the documents and places them into the current pipeline. Note that because this module
    /// does not remove the documents from their original pipeline it's likely you will end up with documents that
    /// have the same content and metadata in two different pipelines. This module does not include the input
    /// documents as part of it's output. If you want to concatenate the result of this module with the input
    /// documents, wrap it with the <see cref="Concat"/> module.
    /// </remarks>
    /// <category>Control</category>
    public class Documents : IModule
    {
        private readonly string _pipeline;
        private readonly ContextConfig _contextDocuments;
        private readonly DocumentConfig _documentDocuments;
        private Func<IDocument, IExecutionContext, bool> _predicate;

        /// <summary>
        /// This outputs all existing documents from all pipelines (except the current one).
        /// </summary>
        public Documents()
        {
        }

        /// <summary>
        /// This outputs the documents from the specified pipeline.
        /// </summary>
        /// <param name="pipeline">The pipeline to output documents from.</param>
        public Documents(string pipeline)
        {
            _pipeline = pipeline;
        }

        /// <summary>
        /// This will get documents based on the context so you can perform custom document
        /// fetching behavior. The delegate will only be called once,
        /// regardless of the number of input documents. The return value
        /// is expected to be a <c>IEnumerable&lt;IDocument&gt;</c>.
        /// </summary>
        /// <param name="documents">A delegate that should return
        /// a <c>IEnumerable&lt;IDocument&gt;</c> containing the documents to output.</param>
        public Documents(ContextConfig documents)
        {
            if (documents == null)
            {
                throw new ArgumentNullException(nameof(documents));
            }
            _contextDocuments = documents;
        }

        /// <summary>
        /// This will get documents based on each input document. The output will be the
        /// aggregate of all returned documents for each input document. The return value
        /// is expected to be a <c>IEnumerable&lt;IDocument&gt;</c>.
        /// </summary>
        /// <param name="documents">A delegate that should return
        /// a <c>IEnumerable&lt;IDocument&gt;</c> containing the documents to
        /// output for each input document.</param>
        public Documents(DocumentConfig documents)
        {
            if (documents == null)
            {
                throw new ArgumentNullException(nameof(documents));
            }
            _documentDocuments = documents;
        }

        /// <summary>
        /// Generates a specified number of new empty documents.
        /// </summary>
        /// <param name="count">The number of new documents to output.</param>
        public Documents(int count)
        {
            _contextDocuments = ctx =>
            {
                List<IDocument> documents = new List<IDocument>();
                for (int c = 0; c < count; c++)
                {
                    documents.Add(ctx.GetDocument());
                }
                return documents;
            };
        }

        /// <summary>
        /// Generates new documents with the specified content.
        /// </summary>
        /// <param name="content">The content for each output document.</param>
        public Documents(params string[] content)
        {
            _contextDocuments = ctx => content.Select(x => ctx.GetDocument(ctx.GetContentStream(x)));
        }

        /// <summary>
        /// Generates new documents with the specified metadata.
        /// </summary>
        /// <param name="metadata">The metadata for each output document.</param>
        public Documents(params IEnumerable<KeyValuePair<string, object>>[] metadata)
        {
            _contextDocuments = ctx => metadata.Select(ctx.GetDocument);
        }

        /// <summary>
        /// Generates new documents with the specified content and metadata.
        /// </summary>
        /// <param name="contentAndMetadata">The content and metadata for each output document.</param>
        public Documents(params Tuple<string, IEnumerable<KeyValuePair<string, object>>>[] contentAndMetadata)
        {
            _contextDocuments = ctx => contentAndMetadata.Select(x => ctx.GetDocument(ctx.GetContentStream(x.Item1), x.Item2));
        }

        /// <summary>
        /// Only documents that satisfy the predicate will be output.
        /// </summary>
        /// <param name="predicate">A delegate that should return a <c>bool</c>.</param>
        public Documents Where(DocumentConfig predicate)
        {
            Func<IDocument, IExecutionContext, bool> currentPredicate = _predicate;
            _predicate = currentPredicate == null
                ? (Func<IDocument, IExecutionContext, bool>)predicate.Invoke<bool>
                : ((x, c) => currentPredicate(x, c) && predicate.Invoke<bool>(x, c));
            return this;
        }

        /// <inheritdoc />
        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            IEnumerable<IDocument> documents;
            if (_documentDocuments != null)
            {
                documents = inputs.SelectMany(context, x => _documentDocuments.Invoke<IEnumerable<IDocument>>(x, context));
            }
            else if (_contextDocuments != null)
            {
                documents = _contextDocuments.Invoke<IEnumerable<IDocument>>(context, "while getting documents");
            }
            else
            {
                documents = string.IsNullOrEmpty(_pipeline)
                    ? context.Documents.ExceptPipeline(context.Pipeline.Name)
                    : context.Documents.FromPipeline(_pipeline);
            }
            if (_predicate != null)
            {
                documents = documents.Where(context, x => _predicate(x, context));
            }
            return documents;
        }
    }
}