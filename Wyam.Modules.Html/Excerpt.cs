using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Extensions;
using AngleSharp.Parser.Html;
using Wyam.Common;
using Wyam.Common.Modules;
using Wyam.Common.Pipelines;
using IDocument = Wyam.Common.Documents.IDocument;

namespace Wyam.Modules.Html
{
    /// <summary>
    /// Finds the first occurrence of a specified HTML element and stores it's contents as metadata.
    /// </summary>
    /// <remarks>
    /// This module is useful for situations like displaying the first paragraph of your most recent
    /// blog posts or <a href="http://wyam.io/knowledgebase/rss-and-atom-feeds">generating RSS and Atom feeds</a>. 
    /// By default, this module looks for the first <c>p</c> (paragraph) element and places it's outer HTML content 
    /// in metadata with a key of <c>Excerpt</c>. The content of the original input document is left unchanged.
    /// </remarks>
    /// <metadata name="Excerpt">Contains the content of the first result from the query 
    /// selector (unless an alternate metadata key is specified).</metadata>
    /// <category>Metadata</category>
    public class Excerpt : IModule
    {
        private string _querySelector = "p";
        private string _metadataKey = "Excerpt";
        private bool _outerHtml = true;

        /// <summary>
        /// Creates the module with the default query selector of <c>p</c>.
        /// </summary>
        public Excerpt()
        {
        }

        /// <summary>
        /// Specifies an alternate query selector for the content.
        /// </summary>
        /// <param name="querySelector">The query selector to use.</param>
        public Excerpt(string querySelector)
        {
            _querySelector = querySelector;
        }

        /// <summary>
        /// Allows you to specify an alternate metadata key.
        /// </summary>
        /// <param name="metadataKey">The metadata key to store the excerpt in.</param>
        public Excerpt SetMetadataKey(string metadataKey)
        {
            _metadataKey = metadataKey;
            return this;
        }

        /// <summary>
        /// Allows you to specify an alternate query selector.
        /// </summary>
        /// <param name="querySelector">The query selector to use.</param>
        public Excerpt WithQuerySelector(string querySelector)
        {
            _querySelector = querySelector;
            return this;
        }

        /// <summary>
        /// Controls whether the inner HTML (not including the containing element's HTML) or 
        /// outer HTML (including the containing element's HTML) of the first result from 
        /// the query selector is added to metadata. The default is to get outer HTML content.
        /// </summary>
        /// <param name="outerHtml">If set to <c>true</c>, outer HTML will be stored.</param>
        public Excerpt GetOuterHtml(bool outerHtml)
        {
            _outerHtml = outerHtml;
            return this;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            HtmlQuery query = new HtmlQuery(_querySelector).First();
            if (_outerHtml)
            {
                query.GetOuterHtml(_metadataKey);
            }
            else
            {
                query.GetInnerHtml(_metadataKey);
            }
            return query.Execute(inputs, context);
        }
    }
}
