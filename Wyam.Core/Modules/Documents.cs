using System;
using System.Collections.Generic;
using System.Linq;
using Wyam.Common;

namespace Wyam.Core.Modules
{
    // This allows you to replace your pipeline with previously processed documents
    public class Documents : IModule
    {
        private readonly string _pipeline;
        private Func<IDocument, IExecutionContext, bool> _predicate;

        public Documents(string pipeline = null)
        {
            _pipeline = pipeline;
        }

        public Documents Where(Func<IDocument, IExecutionContext, bool> predicate)
        {
            _predicate = predicate;
            return this;
        }

        public virtual IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            IEnumerable<IDocument> documents = string.IsNullOrEmpty(_pipeline)
                ? context.Documents.ExceptPipeline(context.Pipeline.Name)
                : context.Documents.FromPipeline(_pipeline);
            if (_predicate != null)
            {
                documents = documents.Where(x => _predicate(x, context));
            }
            return documents;
        }
    }
}