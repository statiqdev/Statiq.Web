using System.Collections.Generic;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Execution;

namespace Wyam.Core.Modules.Contents
{
    /// <summary>
    /// Appends the specified content to the existing content of each document.
    /// </summary>
    /// <category>Content</category>
    public class Append : ContentModule
    {
        /// <summary>
        /// Appends the string value of the specified object to the content of every input document.
        /// </summary>
        /// <param name="content">The content to append.</param>
        public Append(object content)
            : base(content)
        {
        }

        /// <summary>
        /// Appends the string value of the returned object to to content of each document. This
        /// allows you to specify different content to append depending on the execution context.
        /// </summary>
        /// <param name="content">A delegate that returns the content to append.</param>
        public Append(ContextConfig content)
            : base(content)
        {
        }

        /// <summary>
        /// Appends the string value of the returned object to to content of each document.
        /// This allows you to specify different content to append for each document depending
        /// on the input document.
        /// </summary>
        /// <param name="content">A delegate that returns the content to append.</param>
        public Append(DocumentConfig content)
            : base(content)
        {
        }

        /// <summary>
        /// The specified modules are executed against an empty initial document and the results
        /// are appended to the content of every input document (possibly creating more
        /// than one output document for each input document).
        /// </summary>
        /// <param name="modules">The modules to execute.</param>
        public Append(params IModule[] modules)
            : base(modules)
        {
        }

        protected override IEnumerable<IDocument> Execute(object content, IDocument input, IExecutionContext context)
        {
            return new [] { content == null ? input : context.GetDocument(input, context.GetContentStream(input.Content + content)) };
        }
    }
}