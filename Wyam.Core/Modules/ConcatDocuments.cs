using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Abstractions;

namespace Wyam.Core.Modules
{
    // This allows you to insert previously processed documents into your pipeline
    public class ConcatDocuments : IModule
    {
        private readonly string _pipeline;
        private Func<IDocument, bool> _predicate;

        public ConcatDocuments(string pipeline = null)
        {
            _pipeline = pipeline;
        }

        public ConcatDocuments Where(Func<IDocument, bool> predicate)
        {
            _predicate = predicate;
            return this;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            IEnumerable<IDocument> documents = string.IsNullOrEmpty(_pipeline)
                ? context.Documents.Where(x => x.Key != context.Pipeline.Name).SelectMany(x => x.Value)
                : context.Documents[_pipeline];
            if (_predicate != null)
            {
                documents = documents.Where(_predicate);
            }
            return inputs.Concat(documents);
        }
    }
}
