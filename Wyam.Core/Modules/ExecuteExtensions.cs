using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Core.Modules;

namespace Wyam.Core
{
    public static class ExecuteExtensions
    {
        public static IPipelineBuilder Execute(this IPipelineBuilder builder, 
            Func<IPipelineContext, IEnumerable<IPipelineContext>> prepare, 
            Func<IPipelineContext, string, string> execute)
        {
            return builder.AddModule(new Execute(prepare, execute));
        }
    }
}
