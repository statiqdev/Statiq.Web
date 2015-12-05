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
        private bool _escapeAt = true;

        /// <summary>
        /// Specifies whether the <c>@</c> symbol should be escaped (the default is <c>true</c>).
        /// This is important if the Markdown documents are going to be passed to the Razor module,
        /// otherwise the Razor processor will interpret the unescaped <c>@</c> symbols as code
        /// directives.
        /// </summary>
        /// <param name="escapeAt">If set to <c>true</c>, <c>@</c> symbols are HTML escaped.</param>
        public Markdown EscapeAt(bool escapeAt = true)
        {
            _escapeAt = escapeAt;
            return this;
        }

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
                    if (_escapeAt)
                    {
                        result = result.Replace("@", "&#64;");
                    }
                    executionCache.Set(x, result);
                }
                return x.Clone(result);
            });
        }
    }
}
