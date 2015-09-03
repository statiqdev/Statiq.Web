using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Common
{
    public delegate T ContextConfig<out T>(IExecutionContext ctx);

    public delegate object ContextConfig(IExecutionContext ctx);
}
