using System;
using System.Collections.Generic;
using System.Linq;
using WebMarkupMin.Core;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Pipelines;
using Wyam.Common.Tracing;

namespace Wyam.Modules.Html
{
    /// <summary>
    /// Minifies the HTML content.
    /// </summary>
    /// <remarks>
    /// This module takes the HTML content and uses minification to reduce the output.
    /// </remarks>
    /// <example>
    /// <code>
    /// Pipelines.Add("Content",
    ///     ReadFiles("*.md"),
    ///     FrontMatter(Yaml()),
    ///     Markdown(),
    ///     Razor(),
    ///     HtmlMinify(),
    ///     WriteFiles(".html")
    /// );
    /// </code>
    /// </example>
    /// <category>Content</category>
    public class HtmlMinify : IModule
    {
        private readonly HtmlMinificationSettings _minificationSettings;

        /// <summary>
        /// Minifies the HTML content.
        /// </summary>
        /// <param name="useEmptyMinificationSettings">
        /// Boolean to specify whether to use empty minification settings.
        /// Default value is <code>false</code>, this will use commonly accepted settings.
        /// </param>
        public HtmlMinify(bool useEmptyMinificationSettings = false)
        {
            // https://github.com/Taritsyn/WebMarkupMin/wiki/HTML-Minifier
            _minificationSettings = new HtmlMinificationSettings(useEmptyMinificationSettings);
        }

        /// <summary>
        /// Render mode of HTML empty tag. Can take the following values:
        /// <list type="bullets">
        /// <item><description><code>NoSlash</code>.Without slash(for example, <code>&lt;br&gt;</code>).</description></item>
        /// <item><description><code>Slash</code>.With slash(for example, <code>&lt;br/&gt;</code>).</description></item>
        /// <item><description><code>SpaceAndSlash</code>.With space and slash(for example, <code>&lt;br /&gt;</code>).</description></item>
        /// </list>
        /// </summary>
        /// <param name="emptyTagRenderMode">Enum type <see cref="WebMarkupMin.Core.HtmlEmptyTagRenderMode"/>; default value is <code>HtmlEmptyTagRenderMode.NoSlash</code></param>
        /// <returns></returns>
        public HtmlMinify EmptyTagRenderMode(HtmlEmptyTagRenderMode emptyTagRenderMode = HtmlEmptyTagRenderMode.NoSlash)
        {
            _minificationSettings.EmptyTagRenderMode = emptyTagRenderMode;
            return this;
        }

        /// <summary>
        /// Flag for whether to remove all HTML comments, except conditional, noindex, KnockoutJS containerless comments and AngularJS comment directives.
        /// </summary>
        /// <param name="removeHtmlComments">Default value is `true`.</param>
        /// <returns>The current instance.</returns>
        public HtmlMinify RemoveHtmlComments(bool removeHtmlComments = true)
        {
            _minificationSettings.RemoveHtmlComments = removeHtmlComments;
            return this;
        }

        /// <summary>
        /// Flag for whether to remove optional end tags (<code>html</code>, <code>head</code>, <code>body</code>, <code>p</code>, <code>li</code>, <code>dt</code>, <code>dd</code>, <code>rt</code>, <code>rp</code>, <code>optgroup</code>, <code>option</code>, <code>colgroup</code>, <code>thead</code>, <code>tfoot</code>, <code>tbody</code>, <code>tr</code>, <code>th</code> and <code>td</code>).
        /// </summary>
        /// <param name="removeOptionalEndTags">Default value is `true`.</param>
        /// <returns>The current instance.</returns>
        public HtmlMinify RemoveOptionalEndTags(bool removeOptionalEndTags = true)
        {
            _minificationSettings.RemoveOptionalEndTags = removeOptionalEndTags;
            return this;
        }

        /// <summary>
        /// Flag for whether to remove tags without content, except for <code>textarea</code>, <code>tr</code>, <code>th</code> and <code>td</code> tags, and tags with <code>class</code>, <code>id</code>, <code>name</code>, <code>role</code>, <code>src</code> and <code>data-*</code> attributes.
        /// </summary>
        /// <param name="removeTagsWithoutContent">Default value is `false`.</param>
        /// <returns>The current instance.</returns>
        public HtmlMinify RemoveTagsWithoutContent(bool removeTagsWithoutContent = false)
        {
            _minificationSettings.RemoveTagsWithoutContent = removeTagsWithoutContent;
            return this;
        }

        /// <summary>
        /// Updates the minification settings.
        /// </summary>
        /// <param name="action">A function to update the minification settings with.</param>
        /// <returns>The current instance.</returns>
        /// <example>
        /// <code>
        /// HtmlMinify()
        ///     .WithSettings(settings => settings.RemoveHtmlComments = false)
        /// </code>
        /// </example>
        public HtmlMinify WithSettings(Action<HtmlMinificationSettings> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            action(_minificationSettings);

            return this;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            HtmlMinifier minifier = new HtmlMinifier(_minificationSettings);

            return inputs.AsParallel().Select(input =>
            {
                try
                {
                    MarkupMinificationResult result = minifier.Minify(input.Content);

                    if (result.Errors.Count > 0)
                    {
                        Trace.Error("{0} errors found while minifing HTML for {1}:{2}{3}", result.Errors.Count, input.Source, Environment.NewLine, string.Join(Environment.NewLine, result.Errors.Select(x => MinificationErrorInfoToString(x))));
                        return input;
                    }

                    if (result.Warnings.Count > 0)
                    {
                        Trace.Warning("{0} warnings found while minifing HTML for {1}:{2}{3}", result.Warnings.Count, input.Source, Environment.NewLine, string.Join(Environment.NewLine, result.Warnings.Select(x => MinificationErrorInfoToString(x))));
                    }

                    return context.GetDocument(input, result.MinifiedContent);
                }
                catch (Exception ex)
                {
                    Trace.Error("Exception while minifing HTML for {0}: {1}", input.Source, ex.Message);
                    return input;
                }
            });
        }

        private string MinificationErrorInfoToString(MinificationErrorInfo info)
        {
            return string.Format("Line {0}, Column {1}:{5}{2} {3}{5}{4}", info.LineNumber, info.ColumnNumber, info.Category, info.Message, info.SourceFragment, Environment.NewLine);
        }
    }
}