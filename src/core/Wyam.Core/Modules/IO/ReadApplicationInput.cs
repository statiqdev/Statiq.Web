using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Execution;

namespace Wyam.Core.Modules.IO
{
    /// <summary>
    /// Reads text provided to the application on startup.
    /// </summary>
    /// <remarks>
    /// This modules creates a single document from text provided to Wyam on startup. In most cases, this will be text or file contents
    /// "piped" to the Wyam.exe via the command line from a file or prior chained executable. Also known as "Standard Input" or "STDIN".
    /// </remarks>
    /// <example>
    /// An example of piping the output of a prior executable to Wyam.
    /// <code>
    /// AnotherProgram.exe | Wyam.exe
    /// </code>
    /// </example>
    /// <example>
    /// An example of redirecting the contents of a file to Wyam.
    /// <code>
    /// Wyam.exe &lt; my_initial_document.txt
    /// </code>
    /// </example>
    /// <example>
    /// This would read the application input, and write it to a file called "stdin.html"
    /// <code>
    /// Pipelines.Add("StandardInputDoc",
    ///    ReadApplicationInput(),
    ///    Meta("WritePath", "stdin.html"),
    ///    WriteFiles()
    /// );
    /// </code>
    /// </example>
    /// <category>Input/Output</category>
    public class ReadApplicationInput : IModule
    {
        /// <inheritdoc />
        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            // If ApplicationInput is empty, return nothing
            if (string.IsNullOrWhiteSpace(context.ApplicationInput))
            {
                return new IDocument[0];
            }

            return new[] { context.GetDocument(context.GetContentStream(context.ApplicationInput)) };
        }
    }
}
