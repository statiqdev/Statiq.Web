using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Execution;
using Wyam.Common.IO;

namespace Wyam.SearchIndex
{
    /// <summary>
    /// A search item with an arbitrary URL.
    /// </summary>
    public class SearchIndexItem : ISearchIndexItem
    {
        /// <summary>
        /// The URL of the search item.
        /// </summary>
        public string Url { get; set; }

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
        /// <param name="url">The URL this search item should point to.</param>
        /// <param name="title">The title of the search item.</param>
        /// <param name="content">The search item content.</param>
        public SearchIndexItem(string url, string title, string content)
        {
            Url = url;
            Title = title;
            Content = content;
        }

        /// <inheritdoc />
        public string GetLink(IExecutionContext context, bool includeHost) =>
            context.GetLink(new FilePath(Url), includeHost);
    }
}
