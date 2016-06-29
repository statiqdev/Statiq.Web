using System;

namespace Wyam.Core.Syndication
{
    public class FeedItem : IFeedItem
    {
        public Uri ID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public DateTime? Published { get; set; }
        public DateTime? Updated { get; set; }
        public Uri Link { get; set; }
        public Uri ImageLink { get; set; }
        public Uri ThreadLink { get; set; }
        public int? ThreadCount { get; set; }
        public DateTime? ThreadUpdated { get; set; }

        public IFeedItem ToFeedItem(FeedType feedType)
        {
            IFeedItem feedItem = feedType.GetFeedItem();
        }
    }
}