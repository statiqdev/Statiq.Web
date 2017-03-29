using Wyam.Common.Documents;
using Wyam.Common.Execution;

namespace Wyam.Common.Configuration
{
    /// <summary>
    /// A delegate that uses a document and the execution context.
    /// </summary>
    /// <param name="doc">The document.</param>
    /// <param name="ctx">The execution context.</param>
    /// <returns>A result object.</returns>
    public delegate object DocumentConfig(IDocument doc, IExecutionContext ctx);
}
