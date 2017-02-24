using System.Collections.Generic;

namespace Wyam.Feeds.Syndication
{
    /// <summary>
    /// Feed interface
    /// </summary>
    public interface IFeed : IFeedMetadata, INamespaceProvider
    {
        /// <summary>
        /// Gets the type of the feed.
        /// </summary>
        FeedType FeedType { get; }

        /// <summary>
        /// Gets the MIME Type designation for the feed
        /// </summary>
        string MimeType { get; }

        /// <summary>
        /// Gets the copyright
        /// </summary>
        string Copyright { get; }

        /// <summary>
        /// Gets a list of feed items
        /// </summary>
        IList<IFeedItem> Items { get; }
    }
}
