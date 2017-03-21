using System.Collections.Generic;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Execution;

namespace Wyam.Core.Modules.Contents
{
    /// <summary>
    /// Prepends the specified content to the existing content of each document.
    /// </summary>
    /// <category>Content</category>
    public class Prepend : ContentModule
    {
        /// <summary>
        /// Prepends the string value of the specified object to the content of every input document.
        /// </summary>
        /// <param name="content">The content to prepend.</param>
        public Prepend(object content)
            : base(content)
        {
        }

        /// <summary>
        /// Prepends the string value of the returned object to to content of each document. This
        /// allows you to specify different content to prepend depending on the execution context.
        /// </summary>
        /// <param name="content">A delegate that returns the content to prepend.</param>
        public Prepend(ContextConfig content)
            : base(content)
        {
        }

        /// <summary>
        /// Prepends the string value of the returned object to to content of each document. This
        /// allows you to specify different content to prepend for each document depending on the input document.
        /// </summary>
        /// <param name="content">A delegate that returns the content to prepend.</param>
        public Prepend(DocumentConfig content)
            : base(content)
        {
        }

        /// <summary>
        /// The specified modules are executed against an empty initial document and the results are
        /// prepended to the content of every input document (possibly creating more than one output
        /// document for each input document).
        /// </summary>
        /// <param name="modules">The modules to execute.</param>
        public Prepend(params IModule[] modules)
            : base(modules)
        {
        }

        protected override IEnumerable<IDocument> Execute(object content, IDocument input, IExecutionContext context)
        {
            return new[] { content == null ? input : context.GetDocument(input, context.GetContentStream(content + input.Content)) };
        }
    }
}
