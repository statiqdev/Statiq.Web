using System;
using System.Collections.Generic;
using WebMarkupMin.Core;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Execution;

namespace Wyam.Minification
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
    ///     MinifyHtml(),
    ///     WriteFiles(".html")
    /// );
    /// </code>
    /// </example>
    /// <category>Content</category>
    public class MinifyHtml : MinifierBase, IModule
    {
        private readonly HtmlMinificationSettings _minificationSettings;

        /// <summary>
        /// Minifies the HTML content.
        /// </summary>
        /// <param name="useEmptyMinificationSettings">
        /// Boolean to specify whether to use empty minification settings.
        /// Default value is <c>false</c>, this will use commonly accepted settings.
        /// </param>
        public MinifyHtml(bool useEmptyMinificationSettings = false)
        {
            // https://github.com/Taritsyn/WebMarkupMin/wiki/HTML-Minifier
            _minificationSettings = new HtmlMinificationSettings(useEmptyMinificationSettings);
        }

        /// <summary>
        /// Render mode of HTML empty tag. Can take the following values:
        /// <list type="bullets">
        /// <item><description><c>NoSlash</c>.Without slash(for example, <c>&lt;br&gt;</c>).</description></item>
        /// <item><description><c>Slash</c>.With slash(for example, <c>&lt;br/&gt;</c>).</description></item>
        /// <item><description><c>SpaceAndSlash</c>.With space and slash(for example, <c>&lt;br /&gt;</c>).</description></item>
        /// </list>
        /// </summary>
        /// <param name="emptyTagRenderMode">Enum type <see cref="WebMarkupMin.Core.HtmlEmptyTagRenderMode"/>; default value is <c>HtmlEmptyTagRenderMode.NoSlash</c></param>
        /// <returns>The current instance.</returns>
        public MinifyHtml EmptyTagRenderMode(HtmlEmptyTagRenderMode emptyTagRenderMode = HtmlEmptyTagRenderMode.NoSlash)
        {
            _minificationSettings.EmptyTagRenderMode = emptyTagRenderMode;
            return this;
        }

        /// <summary>
        /// Flag for whether to remove all HTML comments, except conditional, noindex, KnockoutJS containerless comments and AngularJS comment directives.
        /// </summary>
        /// <param name="removeHtmlComments">Default value is <c>true</c>.</param>
        /// <returns>The current instance.</returns>
        public MinifyHtml RemoveHtmlComments(bool removeHtmlComments = true)
        {
            _minificationSettings.RemoveHtmlComments = removeHtmlComments;
            return this;
        }

        /// <summary>
        /// Flag for whether to remove tags without content, except for <c>textarea</c>, <c>tr</c>, <c>th</c> and <c>td</c> tags, and tags with <c>class</c>, <c>id</c>, <c>name</c>, <c>role</c>, <c>src</c> and <c>data-*</c> attributes.
        /// </summary>
        /// <param name="removeTagsWithoutContent">Default value is <c>false</c>.</param>
        /// <returns>The current instance.</returns>
        public MinifyHtml RemoveTagsWithoutContent(bool removeTagsWithoutContent = false)
        {
            _minificationSettings.RemoveTagsWithoutContent = removeTagsWithoutContent;
            return this;
        }

        /// <summary>
        /// Flag for whether to remove optional end tags (<c>html</c>, <c>head</c>, <c>body</c>, <c>p</c>, <c>li</c>, <c>dt</c>, <c>dd</c>, <c>rt</c>, <c>rp</c>, <c>optgroup</c>, <c>option</c>, <c>colgroup</c>, <c>thead</c>, <c>tfoot</c>, <c>tbody</c>, <c>tr</c>, <c>th</c> and <c>td</c>).
        /// </summary>
        /// <param name="removeOptionalEndTags">Default value is <c>true</c>.</param>
        /// <returns>The current instance.</returns>
        public MinifyHtml RemoveOptionalEndTags(bool removeOptionalEndTags = true)
        {
            _minificationSettings.RemoveOptionalEndTags = removeOptionalEndTags;
            return this;
        }

        /// <summary>
        /// Updates the minification settings.
        /// </summary>
        /// <param name="action">A function to update the minification settings with.</param>
        /// <returns>The current instance.</returns>
        /// <example>
        /// <code>
        /// MinifyHtml()
        ///     .WithSettings(settings =>
        ///     {
        ///         settings.CollapseBooleanAttributes = false;
        ///         settings.AttributeQuotesRemovalMode = HtmlAttributeQuotesRemovalMode.KeepQuotes;
        ///     })
        /// </code>
        /// </example>
        public MinifyHtml WithSettings(Action<HtmlMinificationSettings> action)
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

            return Minify(inputs, context, minifier.Minify, "HTML");
        }
    }
}