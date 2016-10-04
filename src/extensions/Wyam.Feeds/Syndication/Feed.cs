using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Wyam.Feeds.Syndication
{
    public class Feed : FeedMetadata, IFeed
    {
        public FeedType FeedType => null;
        public string MimeType => null;
        public string Copyright { get; set; }
        IList<IFeedItem> IFeed.Items => Items.Cast<IFeedItem>().ToArray();
        public List<FeedItem> Items { get; } = new List<FeedItem>();

        public void AddNamespaces(XmlSerializerNamespaces namespaces)
        {
        }
    }
}