using Wyam.Common.Documents;
using Wyam.Common.Pipelines;

namespace Wyam.Common.Configuration
{
    public delegate object DocumentConfig(IDocument doc, IExecutionContext ctx);
}
