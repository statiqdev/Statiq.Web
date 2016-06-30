using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Feeds.Syndication;

namespace Wyam.Feeds
{
    // TODO: What about item content?!

    public class GenerateFeeds : IModule
    {
        private ContextConfig _rssPath = _ => new FilePath("feed.rss");
        private ContextConfig _atomPath = _ => new FilePath("feed.atom");
        private ContextConfig _rdfPath = null;

        private ContextConfig _feedId = ctx => ctx.GetLink();
        private ContextConfig _feedTitle = null;
        private ContextConfig _feedDescription = null;
        private ContextConfig _feedAuthor = null;
        private ContextConfig _feedPublished = _ => DateTime.UtcNow;
        private ContextConfig _feedUpdated = _ => DateTime.UtcNow;
        private ContextConfig _feedImageLink = null;
        private ContextConfig _feedCopyright = null;

        private DocumentConfig _itemId = (doc, ctx) => ctx.GetLink(doc, true);
        private DocumentConfig _itemTitle = null;
        private DocumentConfig _itemDescription = null;
        private DocumentConfig _itemAuthor = null;
        private DocumentConfig _itemPublished = null;
        private DocumentConfig _itemUpdated = null;
        private DocumentConfig _itemLink = (doc, ctx) => ctx.GetLink(doc, true);
        private DocumentConfig _itemImageLink = null;
        private DocumentConfig _itemThreadLink = null;
        private DocumentConfig _itemThreadCount = null;
        private DocumentConfig _itemThreadUpdated = null;

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            // Get the feed
            Feed feed = new Feed
            {
                ID = _feedId?.Invoke<Uri>(context),
                Title = _feedTitle?.Invoke<string>(context),
                Description = _feedDescription?.Invoke<string>(context),
                Author = _feedAuthor?.Invoke<string>(context),
                Published = _feedPublished?.Invoke<DateTime?>(context),
                Updated = _feedUpdated?.Invoke<DateTime?>(context),
                ImageLink = _feedImageLink?.Invoke<Uri>(context)
            };

            // Add items
            foreach (IDocument input in inputs)
            {
                feed.Items.Add(new FeedItem
                {
                    ID = _itemId?.Invoke<Uri>(input, context),
                    Title = _itemTitle?.Invoke<string>(input, context),
                    Description = _itemDescription?.Invoke<string>(input, context),
                    Author = _itemAuthor?.Invoke<string>(input, context),
                    Published = _itemPublished?.Invoke<DateTime?>(input, context),
                    Updated = _itemUpdated?.Invoke<DateTime?>(input, context),
                    Link = _itemLink?.Invoke<Uri>(input, context),
                    ImageLink = _itemImageLink?.Invoke<Uri>(input, context),
                    ThreadLink = _itemThreadLink?.Invoke<Uri>(input, context),
                    ThreadCount = _itemThreadCount?.Invoke<int?>(input, context),
                    ThreadUpdated = _itemThreadUpdated?.Invoke<DateTime?>(input, context)
                });
            }

            // Generate the feeds
            return new []
            {
                GenerateFeed(FeedType.Rss, feed, _rssPath, context),
                GenerateFeed(FeedType.Atom, feed, _atomPath, context),
                GenerateFeed(FeedType.Rdf, feed, _rdfPath, context)
            }.Where(x => x != null);
        }

        public IDocument GenerateFeed(FeedType feedType, Feed feed, ContextConfig path, IExecutionContext context)
        {
            // Get the output path
            FilePath outputPath = path?.Invoke<FilePath>(context);
            if (outputPath == null)
            {
                return null;
            }
            if (!outputPath.IsRelative)
            {
                throw new ArgumentException("The feed output path must be relative");
            }

            // Reset the feed link based on the output path
            feed.Link = new Uri(context.GetLink(outputPath, true));

            // Generate the feed and document
            MemoryStream stream = new MemoryStream();
            FeedSerializer.SerializeXml(feedType, feed, stream);
            stream.Position = 0;
            return context.GetDocument(stream, new MetadataItems
            {
                new MetadataItem(Keys.RelativeFilePath, outputPath),
                new MetadataItem(Keys.WritePath, outputPath)
            });
        }
    }
}
