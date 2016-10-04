using System.Collections.Generic;
using System.Linq;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Execution;

namespace Wyam.Common.Modules
{
    // This class can be used as a base class for modules that operate on arbitrary content (as represented by an object)
    public abstract class ContentModule : IModule
    {
        private readonly ConfigHelper<object> _content;
        private readonly IModule[] _modules;

        protected ContentModule(object content)
        {
            _content = new ConfigHelper<object>(content);
        }

        protected ContentModule(ContextConfig content)
        {
            _content = new ConfigHelper<object>(content);
        }

        protected ContentModule(DocumentConfig content)
        {
            _content = new ConfigHelper<object>(content);
        }

        // If only one input document is available, it will be used as the initial document for the specified modules
        // If more than one document is available, an empty initial document will be used
        // To force usage of each input document in a set (I.e., A, B, and C input documents specify a unique "template" metadata value and you want to append
        // some result of operating on that template value to each), make the content module a child of the ForEach module
        // Each input will be applied against each result from the specified modules (I.e., if 2 inputs and the module chain results in 2 outputs, there will be 4 total outputs)
        protected ContentModule(params IModule[] modules)
        {
            _modules = modules;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            if (_modules != null)
            {
                return context.Execute(_modules, inputs.Count == 1 ? inputs : null).SelectMany(x => inputs.SelectMany(y => Execute(x.Content, y, context)));
            }
            return inputs.SelectMany(x => Execute(_content.GetValue(x, context), x, context));
        }

        // Note that content can be passed in as null, implementers should guard against that
        protected abstract IEnumerable<IDocument> Execute(object content, IDocument input, IExecutionContext context);
    }
}
