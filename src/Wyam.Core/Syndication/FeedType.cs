using System;
using Wyam.Core.Syndication.Atom;
using Wyam.Core.Syndication.Rdf;
using Wyam.Core.Syndication.Rss;

namespace Wyam.Core.Syndication
{
    public class FeedType
    {
        public static FeedType Rdf = new FeedType(() => new RdfFeed(), () => new RdfItem());
        public static FeedType Rss = new FeedType(() => new RssFeed(), () => new RssItem());
        public static FeedType Atom = new FeedType(() => new AtomFeed(), () => new AtomEntry());

        private readonly Func<IFeed> _feedFactory;
        private readonly Func<IFeedItem> _itemFactory;

        private FeedType(Func<IFeed> feedFactory, Func<IFeedItem> itemFactory)
        {
            _feedFactory = feedFactory;
            _itemFactory = itemFactory;
        }

        public IFeed GetFeed() => _feedFactory();
        public IFeedItem GetFeedItem() => _itemFactory();
    }
}