using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Common
{
    public delegate T DocumentConfig<out T>(IDocument doc, IExecutionContext ctx);

    public delegate object DocumentConfig(IDocument doc, IExecutionContext ctx);
}
