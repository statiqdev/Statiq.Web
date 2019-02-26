using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Html;
using AngleSharp.Parser.Html;
using Wyam.Common.Configuration;
using Wyam.Common.Execution;
using Wyam.Common.Modules;
using Wyam.Common.Tracing;
using Wyam.Common.Util;

namespace Wyam.Html
{
    /// <summary>
    /// Queries HTML content of the input documents and inserts new content into the elements that
    /// match a query selector.
    /// </summary>
    /// <category>Content</category>
    public class HtmlInsert : IModule
    {
        private readonly string _querySelector;
        private readonly DocumentConfig _content;
        private bool _first;
        private AdjacentPosition _position = AdjacentPosition.BeforeEnd;

        /// <summary>
        /// Creates the module with the specified query selector.
        /// </summary>
        /// <param name="querySelector">The query selector to use.</param>
        /// <param name="content">The content to insert.</param>
        public HtmlInsert(string querySelector, string content)
        {
            _querySelector = querySelector;
            _content = (doc, ctx) => content;
        }

        /// <summary>
        /// Creates the module with the specified query selector.
        /// </summary>
        /// <param name="querySelector">The query selector to use.</param>
        /// <param name="content">The content to insert as a delegate that should return a <c>string</c>.</param>
        public HtmlInsert(string querySelector, DocumentConfig content)
        {
            _querySelector = querySelector;
            _content = content;
        }

        /// <summary>
        /// Specifies that only the first query result should be processed (the default is <c>false</c>).
        /// </summary>
        /// <param name="first">If set to <c>true</c>, only the first result is processed.</param>
        /// <returns>The current module instance.</returns>
        public HtmlInsert First(bool first = true)
        {
            _first = first;
            return this;
        }

        /// <summary>
        /// Specifies where in matching elements the new content should be inserted.
        /// </summary>
        /// <param name="position">A <see cref="AdjacentPosition"/> indicating where the new content should be inserted.</param>
        /// <returns>The current module instance.</returns>
        public HtmlInsert AtPosition(AdjacentPosition position = AdjacentPosition.BeforeEnd)
        {
            _position = position;
            return this;
        }

        /// <inheritdoc />
        public IEnumerable<Common.Documents.IDocument> Execute(IReadOnlyList<Common.Documents.IDocument> inputs, IExecutionContext context)
        {
            HtmlParser parser = new HtmlParser();
            return inputs.AsParallel().Select(context, input =>
            {
                // Get the replacement content
                string content = _content.Invoke<string>(input, context);
                if (content == null)
                {
                    return input;
                }

                // Parse the HTML content
                IHtmlDocument htmlDocument = input.ParseHtml(parser);
                if (htmlDocument == null)
                {
                    return input;
                }

                // Evaluate the query selector
                try
                {
                    if (!string.IsNullOrWhiteSpace(_querySelector))
                    {
                        IElement[] elements = _first
                            ? new[] { htmlDocument.QuerySelector(_querySelector) }
                            : htmlDocument.QuerySelectorAll(_querySelector).ToArray();
                        if (elements.Length > 0 && elements[0] != null)
                        {
                            foreach (IElement element in elements)
                            {
                                element.Insert(_position, content);
                            }

                            Stream contentStream = context.GetContentStream();
                            using (StreamWriter writer = contentStream.GetWriter())
                            {
                                htmlDocument.ToHtml(writer, ProcessingInstructionFormatter.Instance);
                                writer.Flush();
                                return context.GetDocument(input, contentStream);
                            }
                        }
                    }
                    return input;
                }
                catch (Exception ex)
                {
                    Trace.Warning("Exception while processing HTML for {0}: {1}", input.SourceString(), ex.Message);
                    return input;
                }
            });
        }
    }
}
