using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Html
{
    /// <summary>
    /// Metadata keys for use with the various HTML processing modules.
    /// </summary>
    public static class HtmlKeys
    {
        /// <summary>
        /// Contains the content of the first result from the query
        /// selector (unless an alternate metadata key is specified).
        /// </summary>
        /// <type><see cref="string"/></type>
        public const string Excerpt = nameof(Excerpt);

        /// <summary>
        /// Contains the outer HTML of the query result (unless an alternate metadata key is specified).
        /// </summary>
        /// <type><see cref="string"/></type>
        public const string OuterHtml = nameof(OuterHtml);

        /// <summary>
        /// Contains the inner HTML of the query result (unless an alternate metadata key is specified).
        /// </summary>
        /// <type><see cref="string"/></type>
        public const string InnerHtml = nameof(InnerHtml);

        /// <summary>
        /// Contains the text content of the query result (unless an alternate metadata key is specified).
        /// </summary>
        /// <type><see cref="string"/></type>
        public const string TextContent = nameof(TextContent);

        /// <summary>
        /// Documents that represent the headings in each input document.
        /// </summary>
        /// <type><see cref="IReadOnlyList{IDocument}"/></type>
        public const string Headings = nameof(Headings);

        /// <summary>
        /// The value of the <c>id</c> attribute of the current heading document
        /// if the heading contains one.
        /// </summary>
        /// <type><see cref="string"/></type>
        public const string Id = nameof(Id);

        /// <summary>
        /// The level of the heading of the current heading document.
        /// </summary>
        /// <type><see cref="int"/></type>
        public const string Level = nameof(Level);
    }
}
