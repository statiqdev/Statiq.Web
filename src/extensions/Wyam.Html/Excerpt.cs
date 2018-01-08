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
using Wyam.Common.Execution;
using Wyam.Common.Meta;
using Wyam.Common.Util;

namespace Wyam.Html
{
    /// <summary>
    /// Finds the first occurrence of a specified HTML comment or element and stores it's contents as metadata.
    /// </summary>
    /// <remarks>
    /// This module is useful for situations like displaying the first paragraph of your most recent
    /// blog posts or generating RSS and Atom feeds.
    /// This module looks for the first occurrence of an excerpt separator (default of <c>more</c> or <c>excerpt</c>)
    /// contained within an HTML comment (<c>&lt;!--more--&gt;</c>). If a separator comment isn't found, the module
    /// will fallback to looking for the first occurrence of a specific HTML element (<c>p</c> paragraph elements by default)
    /// and will use the outer HTML content. In both cases, the excerpt is placed in metadata with a key of <c>Excerpt</c>.
    /// The content of the original input document is left unchanged.
    /// </remarks>
    /// <metadata cref="HtmlKeys.Excerpt" usage="Output"/>
    /// <category>Metadata</category>
    public class Excerpt : IModule
    {
        private string[] _separators = { "more", "excerpt"};
        private string _querySelector = "p";
        private string _metadataKey = HtmlKeys.Excerpt;
        private bool _outerHtml = true;

        /// <summary>
        /// Creates the module with the default query selector of <c>p</c>.
        /// </summary>
        public Excerpt()
        {
        }

        /// <summary>
        /// Specifies alternate separators to be used in an HTML comment.
        /// Setting this to <c>null</c> will disable looking for separators
        /// and rely only on the query selector.
        /// </summary>
        /// <param name="separators">The excerpt separators.</param>
        public Excerpt(string[] separators)
        {
            _separators = separators;
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
        /// <returns>The current module instance.</returns>
        public Excerpt WithMetadataKey(string metadataKey)
        {
            _metadataKey = metadataKey;
            return this;
        }

        /// <summary>
        /// Specifies alternate separators to be used in an HTML comment.
        /// Setting this to <c>null</c> will disable looking for separators
        /// and rely only on the query selector.
        /// </summary>
        /// <param name="separators">The excerpt separators.</param>
        /// <returns>The current module instance.</returns>
        public Excerpt WithSeparators(string[] separators)
        {
            _separators = separators;
            return this;
        }

        /// <summary>
        /// Allows you to specify an alternate query selector. If a separator
        /// comment was found then the query selector will be used to determine which
        /// elements prior to the separator the excerpt should be taken from.
        /// </summary>
        /// <param name="querySelector">The query selector to use.</param>
        /// <returns>The current module instance.</returns>
        public Excerpt WithQuerySelector(string querySelector)
        {
            _querySelector = querySelector;
            return this;
        }

        /// <summary>
        /// Controls whether the inner HTML (not including the containing element's HTML) or
        /// outer HTML (including the containing element's HTML) of the first result from
        /// the query selector is added to metadata. The default is true, which gets the outer 
        /// HTML content. This setting has no effect if a separator comment is found.
        /// </summary>
        /// <param name="outerHtml">If set to <c>true</c>, outer HTML will be stored.</param>
        /// <returns>The current module instance.</returns>
        public Excerpt WithOuterHtml(bool outerHtml)
        {
            _outerHtml = outerHtml;
            return this;
        }

        /// <inheritdoc />
        public IEnumerable<Common.Documents.IDocument> Execute(IReadOnlyList<Common.Documents.IDocument> inputs, IExecutionContext context)
        {
            if (string.IsNullOrWhiteSpace(_metadataKey))
            {
                return inputs;
            }

            HtmlParser parser = new HtmlParser();
            return inputs.AsParallel().Select(context, input =>
            {
                // Parse the HTML content
                IHtmlDocument htmlDocument = input.ParseHtml(parser);
                if (htmlDocument == null)
                {
                    return input;
                }

                // Get the query string excerpt first
                string queryExcerpt = GetQueryExcerpt(htmlDocument);

                // Now try to get a excerpt separator
                string separatorExcerpt = GetSeparatorExcerpt(htmlDocument);

                // Set the metadata
                string excerpt = separatorExcerpt ?? queryExcerpt;
                if (excerpt != null)
                {
                    return context.GetDocument(input, new MetadataItems
                    {
                        {_metadataKey,  excerpt.Trim()}
                    });
                }
                return input;
            });
        }

        private string GetQueryExcerpt(IHtmlDocument htmlDocument)
        {
            if (!string.IsNullOrEmpty(_querySelector))
            {
                IElement element = htmlDocument.QuerySelector(_querySelector);
                return _outerHtml ? element?.OuterHtml : element?.InnerHtml;
            }
            return null;
        }

        // Use this after attempting to find the excerpt element because it destroys the HTML document
        private string GetSeparatorExcerpt(IHtmlDocument htmlDocument)
        {
            if (_separators != null && _separators.Length > 0)
            {
                ITreeWalker walker = htmlDocument.CreateTreeWalker(htmlDocument.DocumentElement, FilterSettings.Comment);
                IComment comment = (IComment)walker.ToFirst();
                while (comment != null && !_separators.Contains(comment.NodeValue.Trim(), StringComparer.OrdinalIgnoreCase))
                {
                    comment = (IComment)walker.ToNext();
                }

                // Found the first separator
                if (comment != null)
                {
                    // Get a clone of the parent element
                    IElement parent = comment.ParentElement;
                    if (parent.TagName.Equals("p", StringComparison.OrdinalIgnoreCase))
                    {
                        // If we were in a tag inside a paragraph, ascend to the paragraph's parent
                        parent = parent.ParentElement;
                    }

                    // Now remove everything after the separator
                    walker = htmlDocument.CreateTreeWalker(parent);
                    bool remove = false;
                    Stack<INode> removeStack = new Stack<INode>();
                    INode node = walker.ToFirst();
                    while (node != null)
                    {
                        if (node == comment)
                        {
                            remove = true;
                        }
                        if (remove ||
                            // Also remove if it's a top-level element that doesn't match the query selector
                            (node.Parent == parent
                            && node is IElement
                            && !string.IsNullOrEmpty(_querySelector)
                            && !((IElement)node).Matches(_querySelector)))
                        {
                            removeStack.Push(node);
                        }
                        node = walker.ToNext();
                    }
                    while (removeStack.Count > 0)
                    {
                        node = removeStack.Pop();
                        node.Parent.RemoveChild(node);
                    }

                    return parent.InnerHtml;
                }

            }
            return null;
        }
    }
}
