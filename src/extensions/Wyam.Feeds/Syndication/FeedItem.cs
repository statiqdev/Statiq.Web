using System;

namespace Wyam.Feeds.Syndication
{
    public class FeedItem : FeedMetadata, IFeedItem
    {
        public string Content { get; set; }
        public Uri ThreadLink { get; set; }
        public int? ThreadCount { get; set; }
        public DateTime? ThreadUpdated { get; set; }
    }
}