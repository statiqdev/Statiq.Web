using System;

namespace Wyam.Core.Modules.Contents
{
    /// <summary>
    /// Represents an item in the site map.
    /// </summary>
    public class SitemapItem
    {
        public string Location { get; set; }
        public DateTime? LastModUtc { get; set; }
        public ChangeFrequency? ChangeFrequency { get; set; }
        public double? Priority { get; set; }

        public SitemapItem(string location)
        {
            Location = location;
        }
    }
}