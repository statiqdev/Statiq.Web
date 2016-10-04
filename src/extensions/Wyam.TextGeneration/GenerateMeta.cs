using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Procedurally generates metadata using flexible Rant text templates.
    /// </summary>
    /// <category>Metadata</category>
    public class GenerateMeta : RantModule
    {
        private readonly string _key;

        /// <summary>
        /// The specified text template is processed and added as metadata for the specified key for every input document.
        /// </summary>
        /// <param name="key">The metadata key for the generated text.</param>
        /// <param name="template">The template to use.</param>
        public GenerateMeta(string key, object template) : base(template)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            _key = key;
        }

        /// <summary>
        /// Uses a function to determine a text template which is processed and added as metadata for 
        /// each document. This allows you to specify different metadata for each document depending on the context.
        /// </summary>
        /// <param name="key">The metadata key for the generated text.</param>
        /// <param name="template">A delegate that returns the template to use.</param>
        public GenerateMeta(string key, ContextConfig template) : base(template)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            _key = key;
        }

        /// <summary>
        /// Uses a function to determine a text template which is processed and added as metadata for each document. 
        /// This allows you to specify different metadata for each document depending on the input.
        /// </summary>
        /// <param name="key">The metadata key for the generated text.</param>
        /// <param name="template">A delegate that returns the template to use.</param>
        public GenerateMeta(string key, DocumentConfig template) : base(template)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            _key = key;
        }

        /// <summary>
        /// The specified modules are executed against an empty initial document and the resulting content from 
        /// evaluating the entire child module chain is processed as a text template and added as metadata to each input document.
        /// </summary>
        /// <param name="key">The metadata key for the generated text.</param>
        /// <param name="modules">The modules to execute.</param>
        public GenerateMeta(string key, params IModule[] modules) : base(modules)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            _key = key;
        }

        protected override IDocument Execute(string content, IDocument input, IExecutionContext context)
        {
            return context.GetDocument(input, new[] { new KeyValuePair<string, object>(_key, content) });
        }
    }
}
