using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common;
using Wyam.Common.Caching;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Pipelines;

namespace Wyam.Modules.Markdown
{
    /// <summary>
    /// Parses markdown content and renders it to HTML.
    /// </summary>
    /// <remarks>
    /// Parses markdown content in each input document and outputs documents with rendered HTML content.
    /// </remarks>
    /// <category>Templates</category>
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
