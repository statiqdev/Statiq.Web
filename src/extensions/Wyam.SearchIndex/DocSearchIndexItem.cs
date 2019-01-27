using Wyam.Common.Documents;
using Wyam.Common.Execution;

namespace Wyam.SearchIndex
{
    /// <summary>
    /// A search item for a document.
    /// </summary>
    public class DocSearchIndexItem : ISearchIndexItem
    {
        /// <summary>
        /// The document the search item points to.
        /// </summary>
        public IDocument Document { get; set; }

        /// <inheritdoc />
        public string Title { get; set; }

        /// <inheritdoc />
        public string Description { get; set; }

        /// <inheritdoc />
        public string Content { get; set; }

        /// <inheritdoc />
        public string Tags { get; set; }

        /// <summary>
        /// Creates the search item.
        /// </summary>
        /// <param name="document">The document this search item should point to.</param>
        /// <param name="title">The title of the search item.</param>
        /// <param name="content">The search item content.</param>
        public DocSearchIndexItem(IDocument document, string title, string content)
        {
            Document = document;
            Title = title;
            Content = content;
        }

        /// <inheritdoc />
        public string GetLink(IExecutionContext context, bool includeHost) =>
            context.GetLink(Document, includeHost);
    }
}