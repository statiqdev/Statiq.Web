using System;
using System.Collections.Generic;
using WebMarkupMin.Core;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Execution;

namespace Wyam.Modules.Minification
{
    /// <summary>
    /// Minifies the XHTML content.
    /// </summary>
    /// <remarks>
    /// This module takes the XHTML content and uses minification to reduce the output.
    /// </remarks>
    /// <example>
    /// <code>
    /// Pipelines.Add("Content",
    ///     ReadFiles("*.md"),
    ///     FrontMatter(Yaml()),
    ///     Markdown(),
    ///     Razor(),
    ///     XhtmlMinify(),
    ///     WriteFiles(".html")
    /// );
    /// </code>
    /// </example>
    /// <category>Content</category>
    public class XhtmlMinify : MinifierBase, IModule
    {
        private readonly XhtmlMinificationSettings _minificationSettings;

        /// <summary>
        /// Minifies the XHTML content.
        /// </summary>
        /// <param name="useEmptyMinificationSettings">
        /// Boolean to specify whether to use empty minification settings.
        /// Default value is <code>false</code>, this will use commonly accepted settings.
        /// </param>
        public XhtmlMinify(bool useEmptyMinificationSettings = false)
        {
            // https://github.com/Taritsyn/WebMarkupMin/wiki/XHTML-Minifier
            _minificationSettings = new XhtmlMinificationSettings(useEmptyMinificationSettings);
        }

        /// <summary>
        /// Flag for whether to remove all HTML comments, except conditional, noindex, KnockoutJS containerless comments and AngularJS comment directives.
        /// </summary>
        /// <param name="removeHtmlComments">Default value is <code>true</code>.</param>
        /// <returns>The current instance.</returns>
        public XhtmlMinify RemoveHtmlComments(bool removeHtmlComments = true)
        {
            _minificationSettings.RemoveHtmlComments = removeHtmlComments;
            return this;
        }

        /// <summary>
        /// Flag for whether to remove tags without content, except for <code>textarea</code>, <code>tr</code>, <code>th</code> and <code>td</code> tags, and tags with <code>class</code>, <code>id</code>, <code>name</code>, <code>role</code>, <code>src</code> and <code>data-*</code> attributes.
        /// </summary>
        /// <param name="removeTagsWithoutContent">Default value is <code>false</code>.</param>
        /// <returns>The current instance.</returns>
        public XhtmlMinify RemoveTagsWithoutContent(bool removeTagsWithoutContent = false)
        {
            _minificationSettings.RemoveTagsWithoutContent = removeTagsWithoutContent;
            return this;
        }

        /// <summary>
        /// Flag for whether to allow the inserting space before slash in empty tags (for example, <code>true</code> - <code><br /></code>; <code>false</code> - <code><br/></code>).
        /// </summary>
        /// <param name="renderEmptyTagsWithSpace">Default value is <code>true</code>.</param>
        /// <returns>The current instance.</returns>
        public XhtmlMinify RenderEmptyTagsWithSpace(bool renderEmptyTagsWithSpace = true)
        {
            _minificationSettings.RenderEmptyTagsWithSpace = renderEmptyTagsWithSpace;
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
        public XhtmlMinify WithSettings(Action<XhtmlMinificationSettings> action)
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
            XhtmlMinifier minifier = new XhtmlMinifier(_minificationSettings);

            return Minify(inputs, context, minifier.Minify, "XHTML");
        }
    }
}