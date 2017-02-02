using System;
using Markdig;
using Markdig.Helpers;
using System.Collections.Generic;
using System.Linq;
using Wyam.Common.Caching;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Tracing;

namespace Wyam.Markdown
{
    /// <summary>
    /// Parses markdown content and renders it to HTML.
    /// </summary>
    /// <remarks>
    /// Parses markdown content in each input document and outputs documents with rendered HTML content.
    /// Note that @ (at) symbols will be automatically escaped for better compatibility with downstream
    /// Razor modules. Use the <c>EscapeAt()</c> fluent method to modify this behavior.
    /// </remarks>
    /// <category>Templates</category>
    public class Markdown : IModule
    {
        private readonly string _sourceKey;
        private readonly string _destinationKey;
        private readonly OrderedList<IMarkdownExtension> _extensions = new OrderedList<IMarkdownExtension>();
        private string _configuration = "common";
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

        /// <summary>
        /// Includes a set of useful advanced extensions, e.g., citations, footers, footnotes, math,
        /// grid-tables, pipe-tables, and tasks, in the Markdown processing pipeline.
        /// </summary>
        public Markdown UseExtensions()
        {
            _configuration = "advanced";
            return this;
        }

        /// <summary>
        /// Includes a set of extensions defined as a string, e.g., "pipetables", "citations",
        /// "mathematics", or "abbreviations". Separate different extensions with a '+'.
        /// </summary>
        public Markdown UseConfiguration(string extensions)
        {
            _configuration = extensions;
            return this;
        }

        /// <summary>
        /// Includes a custom extension in the markdown processing given by a class implementing
        /// the IMarkdownExtension interface.
        /// </summary>
        /// <typeparam name="TExtension">The type of the extension to use.</typeparam>
        public Markdown UseExtension<TExtension>()
            where TExtension : class, IMarkdownExtension, new()
        {
            _extensions.AddIfNotAlready<TExtension>();
            return this;
        }

        /// <summary>
        /// Includes multiple custom extension in the markdown processing given by classes implementing
        /// the IMarkdownExtension interface.
        /// </summary>
        public Markdown UseExtensions(IEnumerable<Type> extensions)
        {
            if (extensions == null)
            {
                return this;
            }

            foreach (var type in extensions)
            {
                var extension = Activator.CreateInstance(type) as IMarkdownExtension;
                if (extension != null)
                {
                    _extensions.AddIfNotAlready(extension);
                }
            }

            return this;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return inputs.AsParallel().Select(input =>
            {
                Trace.Verbose("Processing Markdown {0} for {1}", 
                    string.IsNullOrEmpty(_sourceKey) ? string.Empty : ("in" + _sourceKey), input.SourceString());
                string result;
                IExecutionCache executionCache = context.ExecutionCache;

                if (!executionCache.TryGetValue<string>(input, _sourceKey, out result))
                {
                    if (string.IsNullOrEmpty(_sourceKey))
                    {
                        MarkdownPipeline pipeline = CreatePipeline();
                        result = Markdig.Markdown.ToHtml(input.Content, pipeline);
                    }
                    else if (input.ContainsKey(_sourceKey))
                    {
                        MarkdownPipeline pipeline = CreatePipeline();
                        result = Markdig.Markdown.ToHtml(input.String(_sourceKey) ?? string.Empty, pipeline);
                    }
                    else
                    {
                        // Don't do anything if the key doesn't exist
                        return input;
                    }

                    if (_escapeAt)
                    {
                        result = result.Replace("@", "&#64;");
                    }

                    executionCache.Set(input, _sourceKey, result);
                }

                return string.IsNullOrEmpty(_sourceKey)
                    ? context.GetDocument(input, result)
                    : context.GetDocument(input, new MetadataItems
                    {
                        { string.IsNullOrEmpty(_destinationKey) ? _sourceKey : _destinationKey, result }
                    });
            });
        }

        private MarkdownPipeline CreatePipeline()
        {
            MarkdownPipelineBuilder pipelineBuilder = new MarkdownPipelineBuilder();
            pipelineBuilder.Configure(_configuration);
            pipelineBuilder.Extensions.AddRange(_extensions);
            return pipelineBuilder.Build();
        }
    }
}
