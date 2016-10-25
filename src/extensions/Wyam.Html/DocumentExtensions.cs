using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using Wyam.Common.Documents;
using Wyam.Common.Tracing;

namespace Wyam.Html
{
    public static class DocumentExtensions
    {
        /// <summary>
        /// Gets an <see cref="IHtmlDocument"/> by parsing the content of an <see cref="IDocument"/>.
        /// </summary>
        /// <param name="document">The document to parse.</param>
        /// <returns>The parsed HTML document.</returns>
        public static IHtmlDocument ParseHtml(this IDocument document) =>
            ParseHtml(document, new HtmlParser());

        /// <summary>
        /// Gets an <see cref="IHtmlDocument"/> by parsing the content of an <see cref="IDocument"/>.
        /// </summary>
        /// <param name="document">The document to parse.</param>
        /// <param name="parser">A parser instance.</param>
        /// <returns>The parsed HTML document.</returns>
        public static IHtmlDocument ParseHtml(this IDocument document, HtmlParser parser)
        {
            try
            {
                using (Stream stream = document.GetStream())
                {
                    return parser.Parse(stream);
                }
            }
            catch (Exception ex)
            {
                Trace.Warning("Exception while parsing HTML for {0}: {1}", document.SourceString(), ex.Message);
            }
            return null;
        }
    }
}
