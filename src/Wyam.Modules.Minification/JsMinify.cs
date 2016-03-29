using System.Collections.Generic;
using WebMarkupMin.Core;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Pipelines;

namespace Wyam.Modules.Minification
{
    /// <summary>
    /// Minifies the JS content.
    /// </summary>
    /// <remarks>
    /// This module takes the JS content and uses minification to reduce the output.
    /// </remarks>
    /// <example>
    /// <code>
    /// Pipelines.Add("JS",
    ///     ReadFiles("*.js"),
    ///     Minification(),
    ///     WriteFiles(".js")
    /// );
    /// </code>
    /// </example>
    /// <category>Minification</category>
    public class JsMinify : MinifierBase, IModule
    {
        private bool _isInlineCode;

        /// <summary>
        /// Minifies the JS content.
        /// </summary>
        /// <param name="isInlineCode">
        /// Boolean to specify whether the content has inline JS code. Default value is <code>false</code>.
        /// </param>
        public JsMinify(bool isInlineCode = false)
        {
            // https://github.com/Taritsyn/WebMarkupMin/wiki/Built-in-JS-minifiers
            _isInlineCode = isInlineCode;
        }

        /// <summary>
        /// Flag for whether the content has inline JS code.
        /// </summary>
        /// <param name="isInlineCode">Default value is <code>true</code>.</param>
        /// <returns>The current instance.</returns>
        public JsMinify IsInlineCode(bool isInlineCode = true)
        {
            _isInlineCode = isInlineCode;
            return this;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            CrockfordJsMinifier minifier = new CrockfordJsMinifier();

            return Minify(inputs, context, (x) => minifier.Minify(x, _isInlineCode), "JS");
        }
    }
}