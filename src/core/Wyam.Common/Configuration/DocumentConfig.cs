using Wyam.Common.Documents;
using Wyam.Common.Execution;

namespace Wyam.Common.Configuration
{
    public delegate object DocumentConfig(IDocument doc, IExecutionContext ctx);
}
