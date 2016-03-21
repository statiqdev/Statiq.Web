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
        /// Minifies the HTML content.
        /// </summary>
        /// <param name="minificationSettings">Pre-defined minification settings (<see cref="WebMarkupMin.Core.HtmlMinificationSettings"/> object) to be passed in.</param>
        public HtmlMinify(HtmlMinificationSettings minificationSettings)
        {
            _minificationSettings = minificationSettings;
        }

        /// <summary>
        /// HTML attribute quotes removal mode. Can take the following values:
        /// <list type="bullets">
        /// <item><description><code>KeepQuotes</code>.Keep quotes.</description></item>
        /// <item><description><code>Html4</code>.Removes a quotes in accordance with standard HTML 4.X.</description></item>
        /// <item><description><code>Html5</code>.Removes a quotes in accordance with standard HTML5.</description></item>
        /// </list>
        /// </summary>
        /// <param name="attributeQuotesRemovalMode">Enum type <see cref="WebMarkupMin.Core.HtmlAttributeQuotesRemovalMode"/>; default value is <code>HtmlAttributeQuotesRemovalMode.Html5</code></param>
        /// <returns></returns>
        public HtmlMinify AttributeQuotesRemovalMode(HtmlAttributeQuotesRemovalMode attributeQuotesRemovalMode = HtmlAttributeQuotesRemovalMode.Html5)
        {
            _minificationSettings.AttributeQuotesRemovalMode = attributeQuotesRemovalMode;
            return this;
        }

        /// <summary>
        /// Flag for whether to remove values from boolean attributes (for example, <code>checked="checked"</code> is transforms to <code>checked</code>).
        /// </summary>
        /// <param name="collapseBooleanAttributes">Default value is `true`.</param>
        /// <returns>The current instance.</returns>
        public HtmlMinify CollapseBooleanAttributes(bool collapseBooleanAttributes = true)
        {
            _minificationSettings.CollapseBooleanAttributes = collapseBooleanAttributes;
            return this;
        }

        /// <summary>
        /// Comma-separated list of names of custom AngularJS directives (e.g. <code>"myDir, btfCarousel"</code>), that contain expressions. If value of the <code>MinifyAngularBindingExpressions</code> property equal to <code>true</code>, then the expressions in custom directives will be minified.
        /// </summary>
        /// <param name="customAngularDirectiveList"></param>
        /// <returns>The current instance.</returns>
        public HtmlMinify CustomAngularDirectiveList(string customAngularDirectiveList)
        {
            _minificationSettings.CustomAngularDirectiveList = customAngularDirectiveList;
            return this;
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
        /// Flag for whether to minify CSS code in <code>style</code> tags.
        /// </summary>
        /// <param name="minifyEmbeddedCssCode">Default value is `true`.</param>
        /// <returns>The current instance.</returns>
        public HtmlMinify MinifyEmbeddedCssCode(bool minifyEmbeddedCssCode = true)
        {
            _minificationSettings.MinifyEmbeddedCssCode = minifyEmbeddedCssCode;
            return this;
        }

        /// <summary>
        /// Flag for whether to minify JS code in <code>script</code> tags.
        /// </summary>
        /// <param name="minifyEmbeddedJsCode">Default value is `true`.</param>
        /// <returns>The current instance.</returns>
        public HtmlMinify MinifyEmbeddedJsCode(bool minifyEmbeddedJsCode = true)
        {
            _minificationSettings.MinifyEmbeddedJsCode = minifyEmbeddedJsCode;
            return this;
        }

        /// <summary>
        /// Flag for whether to minify CSS code in <code>style</code> attributes.
        /// </summary>
        /// <param name="minifyInlineCssCode">Default value is `true`.</param>
        /// <returns>The current instance.</returns>
        public HtmlMinify MinifyInlineCssCode(bool minifyInlineCssCode = true)
        {
            _minificationSettings.MinifyInlineCssCode = minifyInlineCssCode;
            return this;
        }

        /// <summary>
        /// Flag for whether to minify JS code in event attributes and hyperlinks with <code>javascript:</code> pseudo-protocol.
        /// </summary>
        /// <param name="minifyInlineJsCode">Default value is `true`.</param>
        /// <returns>The current instance.</returns>
        public HtmlMinify MinifyInlineJsCode(bool minifyInlineJsCode = true)
        {
            _minificationSettings.MinifyInlineJsCode = minifyInlineJsCode;
            return this;
        }

        /// <summary>
        /// Flag for whether to minify the AngularJS binding expressions in Mustache-style tags (<code>{{}}</code>) and directives.
        /// </summary>
        /// <param name="minifyAngularBindingExpressions">Default value is `false`.</param>
        /// <returns>The current instance.</returns>
        public HtmlMinify MinifyAngularBindingExpressions(bool minifyAngularBindingExpressions = false)
        {
            _minificationSettings.MinifyAngularBindingExpressions = minifyAngularBindingExpressions;
            return this;
        }

        /// <summary>
        /// Flag for whether to minify the KnockoutJS binding expressions in <code>data-bind</code> attributes and containerless comments.
        /// </summary>
        /// <param name="minifyKnockoutBindingExpressions">Default value is `false`.</param>
        /// <returns>The current instance.</returns>
        public HtmlMinify MinifyKnockoutBindingExpressions(bool minifyKnockoutBindingExpressions = false)
        {
            _minificationSettings.MinifyKnockoutBindingExpressions = minifyKnockoutBindingExpressions;
            return this;
        }

        /// <summary>
        /// Comma-separated list of types of <code>script</code> tags, that are processed by minifier (e.g. <code>"text/html, text/ng-template"</code>). Currently only supported the KnockoutJS, Kendo UI MVVM and AngularJS views.
        /// </summary>
        /// <param name="processableScriptTypeList"></param>
        /// <returns>The current instance.</returns>
        public HtmlMinify ProcessableScriptTypeList(string processableScriptTypeList)
        {
            _minificationSettings.ProcessableScriptTypeList = processableScriptTypeList;
            return this;
        }

        /// <summary>
        /// Flag for whether to remove CDATA sections from <code>script</code> and <code>style</code> tags.
        /// </summary>
        /// <param name="removeCdataSectionsFromScriptsAndStyles">Default value is `true`.</param>
        /// <returns>The current instance.</returns>
        public HtmlMinify RemoveCdataSectionsFromScriptsAndStyles(bool removeCdataSectionsFromScriptsAndStyles = true)
        {
            _minificationSettings.RemoveCdataSectionsFromScriptsAndStyles = removeCdataSectionsFromScriptsAndStyles;
            return this;
        }

        /// <summary>
        /// Flag for whether to remove <code>type="text/css"</code> attributes from <code>style</code> and <code>link</code> tags.
        /// </summary>
        /// <param name="removeCssTypeAttributes">Default value is `true`.</param>
        /// <returns>The current instance.</returns>
        public HtmlMinify RemoveCssTypeAttributes(bool removeCssTypeAttributes = true)
        {
            _minificationSettings.RemoveCssTypeAttributes = removeCssTypeAttributes;
            return this;
        }

        /// <summary>
        /// Flag for whether to remove attributes, which have empty value (valid attributes are: <code>class</code>, <code>id</code>, <code>name</code>, <code>style</code>, <code>title</code>, <code>lang</code>, <code>dir</code>, event attributes, <code>action</code> attribute of <code>form</code> tag and <code>value</code> attribute of <code>input</code> tag).
        /// </summary>
        /// <param name="removeEmptyAttributes">Default value is `true`.</param>
        /// <returns>The current instance.</returns>
        public HtmlMinify RemoveEmptyAttributes(bool removeEmptyAttributes = true)
        {
            _minificationSettings.RemoveEmptyAttributes = removeEmptyAttributes;
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
        /// Flag for whether to remove HTML comments from <code>script</code> and <code>style</code> tags.
        /// </summary>
        /// <param name="removeHtmlCommentsFromScriptsAndStyles">Default value is `true`.</param>
        /// <returns>The current instance.</returns>
        public HtmlMinify RemoveHtmlCommentsFromScriptsAndStyles(bool removeHtmlCommentsFromScriptsAndStyles = true)
        {
            _minificationSettings.RemoveHtmlCommentsFromScriptsAndStyles = removeHtmlCommentsFromScriptsAndStyles;
            return this;
        }

        /// <summary>
        /// Flag for whether to remove the HTTP protocol portion (<code>http:</code>) from URI-based attributes (tags marked with <code>rel="external"</code> are skipped).
        /// </summary>
        /// <param name="removeHttpProtocolFromAttributes">Default value is `false`.</param>
        /// <returns>The current instance.</returns>
        public HtmlMinify RemoveHttpProtocolFromAttributes(bool removeHttpProtocolFromAttributes = false)
        {
            _minificationSettings.RemoveHttpProtocolFromAttributes = removeHttpProtocolFromAttributes;
            return this;
        }

        /// <summary>
        /// Flag for whether to remove the HTTPS protocol portion (<code>https:</code>) from URI-based attributes (tags marked with <code>rel="external"</code> are skipped).
        /// </summary>
        /// <param name="removeHttpsProtocolFromAttributes">Default value is `false`.</param>
        /// <returns>The current instance.</returns>
        public HtmlMinify RemoveHttpsProtocolFromAttributes(bool removeHttpsProtocolFromAttributes = false)
        {
            _minificationSettings.RemoveHttpsProtocolFromAttributes = removeHttpsProtocolFromAttributes;
            return this;
        }

        /// <summary>
        /// Flag for whether to remove the <code>javascript:</code> pseudo-protocol portion from event attributes.
        /// </summary>
        /// <param name="removeJsProtocolFromAttributes1">Default value is `true`.</param>
        /// <returns>The current instance.</returns>
        public HtmlMinify RemoveJsProtocolFromAttributes(bool removeJsProtocolFromAttributes = true)
        {
            _minificationSettings.RemoveJsProtocolFromAttributes = removeJsProtocolFromAttributes;
            return this;
        }

        /// <summary>
        /// Flag for whether to remove <code>type="text/javascript"</code> attributes from <code>script</code> tags.
        /// </summary>
        /// <param name="removeJsTypeAttributes">Default value is `true`.</param>
        /// <returns>The current instance.</returns>
        public HtmlMinify RemoveJsTypeAttributes(bool removeJsTypeAttributes = true)
        {
            _minificationSettings.RemoveJsTypeAttributes = removeJsTypeAttributes;
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
        /// Flag for whether to remove redundant attributes from tags, such as:
        /// <list type="bullet">
        /// <item><description><code>&lt;script language="javascript" &hellip;&gt;</code></description></item>
        /// <item><description><code>&lt;script src="&hellip;" charset="&hellip;" &hellip;&gt;</code></description></item>
        /// <item><description><code>&lt;link rel="stylesheet" charset="&hellip;" &hellip;&gt;</code></description></item>
        /// <item><description><code>&lt;form method="get" &hellip;&gt;</code></description></item>
        /// <item><description><code>&lt;input type="text" &hellip;&gt;</code></description></item>
        /// <item><description><code>&lt;a id="&hellip;" name="&hellip;" &hellip;&gt;</code></description></item>
        /// <item><description><code>&lt;area shape="rect" &hellip;&gt;</code></description></item>
        /// </list>
        /// </summary>
        /// <param name="removeRedundantAttributes">Default value is `false`.</param>
        /// <returns>The current instance.</returns>
        public HtmlMinify RemoveRedundantAttributes(bool removeRedundantAttributes = false)
        {
            _minificationSettings.RemoveRedundantAttributes = removeRedundantAttributes;
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
        /// Flag for whether to replace <code>&lt;meta http-equiv="content-type" content="text/html; charset=&hellip;"&gt;</code> tag by <code>&lt;meta charset="&hellip;"&gt;</code> tag
        /// </summary>
        /// <param name="useMetaCharsetTag">Default value is `true`.</param>
        /// <returns>The current instance.</returns>
        public HtmlMinify UseMetaCharsetTag(bool useMetaCharsetTag = true)
        {
            _minificationSettings.UseMetaCharsetTag = useMetaCharsetTag;
            return this;
        }

        /// <summary>
        /// Flag for whether to replace existing document type declaration by short declaration - <code>&lt;!DOCTYPE html&gt;</code>.
        /// </summary>
        /// <param name="useShortDoctype">Default value is `true`.</param>
        /// <returns>The current instance.</returns>
        public HtmlMinify UseShortDoctype(bool useShortDoctype = true)
        {
            _minificationSettings.UseShortDoctype = useShortDoctype;
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