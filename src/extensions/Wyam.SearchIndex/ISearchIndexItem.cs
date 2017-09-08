using Wyam.Common.Execution;

namespace Wyam.SearchIndex
{
    /// <summary>
    /// Represents an item in the search index.
    /// </summary>
    public interface ISearchIndexItem
    {
        /// <summary>
        /// The title of the search item.
        /// </summary>
        string Title { get; }

        /// <summary>
        /// The description of the search item.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// The content of the search item.
        /// </summary>
        string Content { get; }

        /// <summary>
        /// Any tags for the search item.
        /// </summary>
        string Tags { get; }

        /// <summary>
        /// Gets a link to the search item result.
        /// </summary>
        /// <param name="context">The current execution context.</param>
        /// <param name="includeHost"><c>true</c> to include the hostname, <c>false otherwise</c>.</param>
        /// <returns>A link to the search item.</returns>
        string GetLink(IExecutionContext context, bool includeHost);
    }
}