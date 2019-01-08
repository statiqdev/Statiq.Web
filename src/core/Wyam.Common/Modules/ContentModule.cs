using System.Collections.Generic;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Util;

namespace Wyam.Common.Modules
{
    /// <summary>
    /// This class can be used as a base class for modules that operate on arbitrary content (as represented by an object).
    /// </summary>
    public abstract class ContentModule : IModule
    {
        private readonly ConfigHelper<object> _content;
        private readonly IModule[] _modules;

        /// <summary>
        /// Creates a new content module with the specified content.
        /// </summary>
        /// <param name="content">The content.</param>
        protected ContentModule(object content)
        {
            _content = new ConfigHelper<object>(content);
        }

        /// <summary>
        /// Creates a new content module with the specified content delegate.
        /// </summary>
        /// <param name="content">The content delegate.</param>
        protected ContentModule(ContextConfig content)
        {
            _content = new ConfigHelper<object>(content);
        }

        /// <summary>
        /// Creates a new content module with the specified content delegate.
        /// </summary>
        /// <param name="content">The content delegate.</param>
        protected ContentModule(DocumentConfig content)
        {
            _content = new ConfigHelper<object>(content);
        }

        /// <summary>
        /// Creates a new content module with the content determined by child modules.
        /// </summary>
        /// <remarks>
        /// If only one input document is available, it will be used as the initial document for the specified modules.
        /// If more than one document is available, an empty initial document will be used.
        /// To force usage of each input document in a set (I.e., A, B, and C input documents specify a unique "template" metadata value and you want to append
        /// some result of operating on that template value to each), make the content module a child of the ForEach module.
        /// Each input will be applied against each result from the specified modules (I.e., if 2 inputs and the module chain results in 2 outputs, there will be 4 total outputs).
        /// </remarks>
        /// <param name="modules">The child modules.</param>
        protected ContentModule(params IModule[] modules)
        {
            _modules = modules;
        }

        /// <inheritdoc />
        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            if (_modules != null)
            {
                return context
                    .Execute(_modules, inputs.Count == 1 ? inputs : null)
                    .SelectMany(
                        context,
                        x => inputs.SelectMany(context, y => Execute(x.Content, y, context)));
            }
            return inputs.SelectMany(context, x => Execute(_content.GetValue(x, context), x, context));
        }

        /// <summary>
        /// Executes the module with the specified content against a single document.
        /// Note that content can be passed in as null, implementers should guard against that.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="input">The input document.</param>
        /// <param name="context">The execution context.</param>
        /// <returns>Result documents.</returns>
        protected abstract IEnumerable<IDocument> Execute(object content, IDocument input, IExecutionContext context);
    }
}
