using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Wyam.Core.Syndication
{
    public class Feed : IFeed
    {
        private readonly List<XmlSerializerNamespaces> _namespaces
            = new List<XmlSerializerNamespaces>();
        private readonly List<IFeedItem> _feedItems = new List<IFeedItem>();

        public string MimeType => null;
        public string Copyright { get; set; }
        public IList<IFeedItem> Items => _feedItems;
        public Uri ID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public DateTime? Published { get; set; }
        public DateTime? Updated { get; set; }
        public Uri Link { get; set; }
        public Uri ImageLink { get; set; }

        public void AddNamespaces(XmlSerializerNamespaces namespaces)
        {
            throw new NotImplementedException();
        }

        public IFeed ToFeed(FeedType feedType)
        {
            // Convert the feed
            IFeed feed = feedType.GetFeed();
            feed.Copyright = Copyright;
        }
    }
}