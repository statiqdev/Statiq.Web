using System;
using System.Collections.Generic;
using System.Linq;
using Wyam.Extensibility;

namespace Wyam.Core.Modules
{
    public abstract class ContentModule : IModule
    {
        private readonly Func<IDocument, object> _content;
        private readonly IModule[] _modules;
        private readonly bool _forEachDocument;
        
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
        protected ContentModule(bool forEachDocument, params IModule[] modules)
        {
            _forEachDocument = forEachDocument;
            _modules = modules;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IPipelineContext pipeline)
        {
            if (_modules != null)
            {
                if (_forEachDocument)
                {
                    return inputs.SelectMany(input => pipeline.Execute(_modules, new[] { input }).SelectMany(result => Execute(result.Content, input, pipeline)));
                }
                return pipeline.Execute(_modules, null).SelectMany(result => inputs.SelectMany(input => Execute(result.Content, input, pipeline)));
            }
            return inputs.SelectMany(x => Execute(_content(x), x, pipeline));
        }

        // Note that content can be passed in as null, implementors should guard against that
        protected abstract IEnumerable<IDocument> Execute(object content, IDocument input, IPipelineContext pipeline);
    }
}