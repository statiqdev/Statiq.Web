using System;
using System.Collections.Generic;
using WebMarkupMin.Core;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Execution;

namespace Wyam.Minification
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
    ///     MinifyXhtml(),
    ///     WriteFiles(".html")
    /// );
    /// </code>
    /// </example>
    /// <category>Content</category>
    public class MinifyXhtml : MinifierBase, IModule
    {
        private readonly XhtmlMinificationSettings _minificationSettings;

        /// <summary>
        /// Minifies the XHTML content.
        /// </summary>
        /// <param name="useEmptyMinificationSettings">
        /// Boolean to specify whether to use empty minification settings.
        /// Default value is <c>false</c>, this will use commonly accepted settings.
        /// </param>
        public MinifyXhtml(bool useEmptyMinificationSettings = false)
        {
            // https://github.com/Taritsyn/WebMarkupMin/wiki/XHTML-Minifier
            _minificationSettings = new XhtmlMinificationSettings(useEmptyMinificationSettings);
        }

        /// <summary>
        /// Flag for whether to remove all HTML comments, except conditional, noindex, KnockoutJS containerless comments and AngularJS comment directives.
        /// </summary>
        /// <param name="removeHtmlComments">Default value is <c>true</c>.</param>
        /// <returns>The current instance.</returns>
        public MinifyXhtml RemoveHtmlComments(bool removeHtmlComments = true)
        {
            _minificationSettings.RemoveHtmlComments = removeHtmlComments;
            return this;
        }

        /// <summary>
        /// Flag for whether to remove tags without content, except for <c>textarea</c>, <c>tr</c>, <c>th</c> and <c>td</c> tags, and tags with <c>class</c>, <c>id</c>, <c>name</c>, <c>role</c>, <c>src</c> and <c>data-*</c> attributes.
        /// </summary>
        /// <param name="removeTagsWithoutContent">Default value is <c>false</c>.</param>
        /// <returns>The current instance.</returns>
        public MinifyXhtml RemoveTagsWithoutContent(bool removeTagsWithoutContent = false)
        {
            _minificationSettings.RemoveTagsWithoutContent = removeTagsWithoutContent;
            return this;
        }

        /// <summary>
        /// Flag for whether to allow the inserting space before slash in empty tags (for example, <c>true</c> - <c><br /></c>; <c>false</c> - <c><br/></c>).
        /// </summary>
        /// <param name="renderEmptyTagsWithSpace">Default value is <c>true</c>.</param>
        /// <returns>The current instance.</returns>
        public MinifyXhtml RenderEmptyTagsWithSpace(bool renderEmptyTagsWithSpace = true)
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
        /// MinifyXhtml()
        ///     .WithSettings(settings => settings.RemoveHtmlComments = false)
        /// </code>
        /// </example>
        public MinifyXhtml WithSettings(Action<XhtmlMinificationSettings> action)
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