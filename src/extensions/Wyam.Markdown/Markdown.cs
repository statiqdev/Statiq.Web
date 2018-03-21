using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Markdig;
using Markdig.Helpers;

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
    /// Note that <c>@</c> (at) symbols will be automatically HTML escaped for better compatibility with downstream
    /// Razor modules. If you want to include a raw <c>@</c> symbol when <c>EscapeAt()</c> is <c>true</c>, use
    /// <c>\@</c>. Use the <c>EscapeAt()</c> fluent method to modify this behavior.
    /// </remarks>
    /// <category>Templates</category>
    public class Markdown : IModule
    {
        /// <summary>
        /// The default Markdown configuration.
        /// </summary>
        public const string DefaultConfiguration = "common";

        private static readonly Regex EscapeAtRegex = new Regex("(?<!\\\\)@");

        private readonly string _sourceKey;
        private readonly string _destinationKey;
        private readonly OrderedList<IMarkdownExtension> _extensions = new OrderedList<IMarkdownExtension>();
        private string _configuration = DefaultConfiguration;
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
        /// If you want to include a raw <c>@</c> symbol when <c>EscapeAt()</c> is <c>true</c>, use <c>\@</c>.
        /// </summary>
        /// <param name="escapeAt">If set to <c>true</c>, <c>@</c> symbols are HTML escaped.</param>
        /// <returns>The current module instance.</returns>
        public Markdown EscapeAt(bool escapeAt = true)
        {
            _escapeAt = escapeAt;
            return this;
        }

        /// <summary>
        /// Includes a set of useful advanced extensions, e.g., citations, footers, footnotes, math,
        /// grid-tables, pipe-tables, and tasks, in the Markdown processing pipeline.
        /// </summary>
        /// <returns>The current module instance.</returns>
        public Markdown UseExtensions()
        {
            _configuration = "advanced";
            return this;
        }

        /// <summary>
        /// Includes a set of extensions defined as a string, e.g., "pipetables", "citations",
        /// "mathematics", or "abbreviations". Separate different extensions with a '+'.
        /// </summary>
        /// <param name="extensions">The extensions string.</param>
        /// <returns>The current module instance.</returns>
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
        /// <returns>The current module instance.</returns>
        public Markdown UseExtension<TExtension>()
            where TExtension : class, IMarkdownExtension, new()
        {
            _extensions.AddIfNotAlready<TExtension>();
            return this;
        }

        /// <summary>
        /// Includes a custom extension in the markdown processing given by a object implementing
        /// the IMarkdownExtension interface.
        /// </summary>
        /// <param name="extension">A object that that implement <see cref="IMarkdownExtension"/>.</param>
        /// <typeparam name="TExtension">The type of the extension to use.</typeparam>
        /// <returns>The current module instance.</returns>
        public Markdown UseExtension<TExtension>(TExtension extension)
            where TExtension : IMarkdownExtension
        {
            if (extension != null)
            {
                _extensions.AddIfNotAlready(extension);
            }

            return this;
        }

        /// <summary>
        /// Includes multiple custom extension in the markdown processing given by classes implementing
        /// the <see cref="IMarkdownExtension"/> interface.
        /// </summary>
        /// <param name="extensions">A sequence of types that implement <see cref="IMarkdownExtension"/>.</param>
        /// <returns>The current module instance.</returns>
        public Markdown UseExtensions(IEnumerable<Type> extensions)
        {
            if (extensions == null)
            {
                return this;
            }

            foreach (Type type in extensions)
            {
                IMarkdownExtension extension = Activator.CreateInstance(type) as IMarkdownExtension;
                if (extension != null)
                {
                    // Need - public void AddIfNotAlready<TElement>(TElement telement) where TElement : T;
                    // Kind of hack'ish, but no other way to preserve types.
                    MethodInfo addIfNotAlready = typeof(OrderedList<IMarkdownExtension>).GetMethods()
                        .Where(x => x.IsGenericMethod && x.Name == nameof(OrderedList<IMarkdownExtension>.AddIfNotAlready) && x.GetParameters().Length == 1)
                        .Select(x => x.MakeGenericMethod(type))
                        .Single();
                    addIfNotAlready.Invoke(_extensions, new object[] { extension });
                }
            }

            return this;
        }

        /// <inheritdoc />
        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return inputs.AsParallel().Select(context, input =>
            {
                Trace.Verbose(
                    "Processing Markdown {0} for {1}",
                    string.IsNullOrEmpty(_sourceKey) ? string.Empty : ("in" + _sourceKey),
                    input.SourceString());
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
                        result = EscapeAtRegex.Replace(result, "&#64;");
                        result = result.Replace("\\@", "@");
                    }

                    executionCache.Set(input, _sourceKey, result);
                }

                return string.IsNullOrEmpty(_sourceKey)
                    ? context.GetDocument(input, context.GetContentStream(result))
                    : context.GetDocument(input, new MetadataItems
                    {
                        {string.IsNullOrEmpty(_destinationKey) ? _sourceKey : _destinationKey, result}
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