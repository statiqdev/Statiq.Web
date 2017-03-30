using System;

namespace Wyam.Core.Modules.Contents
{
    /// <summary>
    /// Represents an item in the site map.
    /// </summary>
    public class SitemapItem
    {
        /// <summary>
        /// Gets or sets the location of the sitemap item.
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Gets or sets the last modified time of the item in UTC.
        /// </summary>
        public DateTime? LastModUtc { get; set; }

        /// <summary>
        /// Gets or sets the expected frequency of changes of the item.
        /// </summary>
        public ChangeFrequency? ChangeFrequency { get; set; }

        /// <summary>
        /// Gets or sets the priority of the item.
        /// </summary>
        public double? Priority { get; set; }

        /// <summary>
        /// Creates a new sitemap item at the specified location.
        /// </summary>
        /// <param name="location">The location of the sitemap item.</param>
        public SitemapItem(string location)
        {
            Location = location;
        }
    }
}