using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Execution;

namespace Wyam.TextGeneration
{
    /// <summary>
    /// Procedurally generates content using flexible Rant text templates.
    /// </summary>
    /// <category>Content</category>
    public class GenerateContent : RantModule
    {
        /// <summary>
        /// The specified text template is processed and added as content for every input document.
        /// </summary>
        /// <param name="template">The template to use.</param>
        public GenerateContent(object template)
            : base(template)
        {
        }

        /// <summary>
        /// Uses a function to determine a text template which is processed and added as content
        /// for each document. This allows you to specify different content for each document depending on the context.
        /// </summary>
        /// <param name="template">A delegate that returns the template to use.</param>
        public GenerateContent(ContextConfig template)
            : base(template)
        {
        }

        /// <summary>
        /// Uses a function to determine a text template which is processed and added as content for
        /// each document. This allows you to specify different content for each document depending on the input.
        /// </summary>
        /// <param name="template">A delegate that returns the template to use.</param>
        public GenerateContent(DocumentConfig template)
            : base(template)
        {
        }

        /// <summary>
        /// The specified modules are executed against an empty initial document and the resulting content
        /// from evaluating the entire child module chain is processed as a text template and added as
        /// content to each input document.
        /// </summary>
        /// <param name="modules">The modules to execute.</param>
        public GenerateContent(params IModule[] modules)
            : base(modules)
        {
        }

        protected override IDocument Execute(string content, IDocument input, IExecutionContext context)
        {
            return context.GetDocument(input, context.GetContentStream(content));
        }
    }
}
