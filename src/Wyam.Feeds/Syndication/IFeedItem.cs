using System;

namespace Wyam.Feeds.Syndication
{
    /// <summary>
    /// Item interface
    /// </summary>
    public interface IFeedItem : IFeedMetadata
    {
        /// <summary>
        /// Gets the link to comments on this item
        /// </summary>
        Uri ThreadLink { get; }

        /// <summary>
        /// Gets the number of comments on this item
        /// </summary>
        int? ThreadCount { get; }

        /// <summary>
        /// Gets the number of comments on this item
        /// </summary>
        DateTime? ThreadUpdated { get; }
    }
}