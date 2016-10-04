using System;

namespace Wyam.Feeds.Syndication
{
    public abstract class FeedMetadata : IFeedMetadata
    {
        public Uri ID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public DateTime? Published { get; set; }
        public DateTime? Updated { get; set; }
        public Uri Link { get; set; }
        public Uri ImageLink { get; set; }
    }
}