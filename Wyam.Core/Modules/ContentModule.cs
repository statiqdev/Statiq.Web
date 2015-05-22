using System;
using System.Collections.Generic;
using System.Linq;
using Wyam.Abstractions;

namespace Wyam.Core.Modules
{
    public abstract class ContentModule : IModule
    {
        private readonly Func<IDocument, object> _content;
        private readonly IModule[] _modules;
        private bool _forEachDocument;
        
        protected ContentModule(object content)
        {
            _content = x => content;
        }

        protected ContentModule(Func<IDocument, object> content)
        {
            _content = content ?? (x => null);
        }

        // For performance reasons, the specified modules will only be run once with a newly initialized, isolated document
        // Otherwise, we'd need to run the whole set for each input document (I.e., multiple duplicate file reads, transformations, etc. for each input)
        // Each input will be applied against each result from the specified modules (I.e., if 2 inputs and the module chain results in 2 outputs, there will be 4 total outputs)
        protected ContentModule(params IModule[] modules)
        {
            _modules = modules;
        }

        // Setting true for forEachDocument results in the whole sequence of modules being executed for every input document
        // (as opposed to only being executed once with an empty initial document)
        // You use this when the content modules rely on the input document - I.e., to read specific files based on metadata such as original file name in each input document
        public IModule ForEachDocument()
        {
            _forEachDocument = true;
            return this;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            if (_modules != null)
            {
                if (_forEachDocument)
                {
                    return inputs.SelectMany(input => context.Execute(_modules, new[] { input }).SelectMany(result => Execute(result.Content, input, context)));
                }
                return context.Execute(_modules, null).SelectMany(result => inputs.SelectMany(input => Execute(result.Content, input, context)));
            }
            return inputs.SelectMany(x => Execute(_content(x), x, context));
        }

        // Note that content can be passed in as null, implementors should guard against that
        protected abstract IEnumerable<IDocument> Execute(object content, IDocument input, IExecutionContext context);
    }
}