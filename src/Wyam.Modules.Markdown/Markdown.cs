using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common;
using Wyam.Common.Caching;
using Wyam.Common.Documents;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Pipelines;
using Wyam.Common.Tracing;

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
        private readonly string _sourceKey;
        private readonly string _destinationKey;
        private bool _escapeAt = true;


        /// <summary>
        /// Processes Markdown in the content of the document.
        /// </summary>
        public Markdown()
        {
        }

        /// <summary>
        /// Processes Markdown in the metadata of the document. The rendered HTML will be placed
        /// </summary>
        /// <param name="sourceKey">The metadata key of the Markdown to process.</param>
        /// <param name="destinationKey">The metadata key to store the rendered HTML (if null, it gets placed back in the source metadata key).</param>
        public Markdown(string sourceKey, string destinationKey = null)
        {
            _sourceKey = sourceKey;
            _destinationKey = destinationKey;
        }

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
                Trace.Verbose("Processing Markdown {0}for {1}", 
                    string.IsNullOrEmpty(_sourceKey) ? string.Empty : ("in" + _sourceKey), x.Source);
                string result;
                IExecutionCache executionCache = context.ExecutionCache;
                if (!executionCache.TryGetValue<string>(x, _sourceKey, out result))
                {
                    if (string.IsNullOrEmpty(_sourceKey))
                    {
                        result = CommonMark.CommonMarkConverter.Convert(x.Content);
                    }
                    else
                    {
                        if (!x.ContainsKey(_sourceKey))
                        {
                            // Don't do anything if the key doesn't exist
                            return x;
                        }
                        result = CommonMark.CommonMarkConverter.Convert(x.String(_sourceKey) ?? string.Empty);
                    }
                    if (_escapeAt)
                    {
                        result = result.Replace("@", "&#64;");
                    }
                    executionCache.Set(x, _sourceKey, result);
                }
                return string.IsNullOrEmpty(_sourceKey)
                    ? x.Clone(result)
                    : x.Clone(new MetadataItems
                    {
                        { string.IsNullOrEmpty(_destinationKey) ? _sourceKey : _destinationKey, result }
                    });
            });
        }
    }
}
