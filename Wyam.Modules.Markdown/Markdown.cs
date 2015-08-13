using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common;

namespace Wyam.Modules.Markdown
{
    public class Markdown : IModule
    {
        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return inputs.AsParallel().Select(x =>
            {
                context.Trace.Verbose("Processing Markdown for {0}", x.Source);
                string result;
                IExecutionCache executionCache = context.ExecutionCache;
                if (!executionCache.TryGetValue<string>(x, out result))
                {
                    result = CommonMark.CommonMarkConverter.Convert(x.Content);
                    executionCache.Set(x, result);
                }
                return x.Clone(result);
            });
        }
    }
}
