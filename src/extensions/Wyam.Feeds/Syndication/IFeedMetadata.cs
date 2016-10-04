using System;

namespace Wyam.Feeds.Syndication
{
    /// <summary>
    /// Metadata about a feed or feed item
    /// </summary>
    public interface IFeedMetadata
    {
        /// <summary>
        /// Gets a unique identifier
        /// </summary>
        Uri ID { get; }

        /// <summary>
        /// Gets the title
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Gets the description
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Gets the author
        /// </summary>
        string Author { get; }

        /// <summary>
        /// Gets the initial date published
        /// </summary>
        DateTime? Published { get; }

        /// <summary>
        /// Gets the date last updated
        /// </summary>
        DateTime? Updated { get; }

        /// <summary>
        /// Gets the link to the full version
        /// </summary>
        Uri Link { get; }

        /// <summary>
        /// Gets a link to a related image
        /// </summary>
        Uri ImageLink { get; }
    }
}