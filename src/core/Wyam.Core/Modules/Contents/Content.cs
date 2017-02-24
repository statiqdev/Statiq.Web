using System.Collections.Generic;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Execution;

namespace Wyam.Core.Modules.Contents
{
    /// <summary>
    /// Replaces the content of each input document with the string value of the specified content object.
    /// </summary>
    /// <remarks>
    /// In the case where modules are provided, they are executed against an
    /// empty initial document and the results are applied to each input document.
    /// </remarks>
    /// <category>Content</category>
    public class Content : ContentModule
    {
        /// <summary>
        /// Uses the string value of the specified object as the new content for every input document.
        /// </summary>
        /// <param name="content">The new content to use.</param>
        public Content(object content)
            : base(content)
        {
        }

        /// <summary>
        /// Uses the string value of the returned object as the new content for each document.
        /// This allows you to specify different content depending on the execution context.
        /// </summary>
        /// <param name="content">A delegate that gets the new content to use.</param>
        public Content(ContextConfig content)
            : base(content)
        {
        }

        /// <summary>
        /// Uses the string value of the returned object as the new content for each document. This
        /// allows you to specify different content for each document depending on the input document.
        /// </summary>
        /// <param name="content">A delegate that gets the new content to use.</param>
        public Content(DocumentConfig content)
            : base(content)
        {
        }

        /// <summary>
        /// The specified modules are executed against an empty initial document and the results
        /// are applied to every input document (possibly creating more than one output
        /// document for each input document).
        /// </summary>
        /// <param name="modules">The modules to execute.</param>
        public Content(params IModule[] modules)
            : base(modules)
        {
        }

        protected override IEnumerable<IDocument> Execute(object content, IDocument input, IExecutionContext context)
        {
            return new [] { content == null ? input : context.GetDocument(input, content.ToString()) };
        }
    }
}
