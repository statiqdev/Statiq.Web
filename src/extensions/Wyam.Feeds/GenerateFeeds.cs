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
using Wyam.Common.Util;
using Wyam.Feeds.Syndication;

namespace Wyam.Feeds
{
    /// <summary>
    /// Generates syndication feeds including RSS, Atom, and RDF.
    /// </summary>
    /// <remarks>
    /// Each input document represents an item in the feed, up to the specified maximum number of
    /// documents. Note that documents will be used in the order in which they are input
    /// into this module, so a <c>OrderBy</c> module or similar should be used
    /// to order the documents prior to this module. You should also set the <code>Settings.Host</code> value
    /// in your configuration file since that's used to create the absolute links for feed readers.
    /// This module outputs a document for each of the selected feed types. Input documents
    /// are not output by this module.
    /// </remarks>
    /// <metadata cref="FeedKeys.Title" usage="Input"/>
    /// <metadata cref="FeedKeys.Description" usage="Input"/>
    /// <metadata cref="FeedKeys.Author" usage="Input"/>
    /// <metadata cref="FeedKeys.Image" usage="Input"/>
    /// <metadata cref="FeedKeys.Copyright" usage="Input"/>
    /// <metadata cref="FeedKeys.Excerpt" usage="Input"/>
    /// <metadata cref="FeedKeys.Published" usage="Input"/>
    /// <metadata cref="FeedKeys.Updated" usage="Input"/>
    /// <metadata cref="FeedKeys.Content" usage="Input"/>
    /// <metadata cref="Keys.RelativeFilePath" usage="Output">Relative path to the output feed file.</metadata>
    /// <metadata cref="Keys.WritePath" usage="Output" />
    /// <category>Content</category>
    public class GenerateFeeds : IModule
    {
        /// <summary>
        /// The default path for RSS files.
        /// </summary>
        public static readonly FilePath DefaultRssPath = new FilePath("feed.rss");

        /// <summary>
        /// The default path for Atom files.
        /// </summary>
        public static readonly FilePath DefaultAtomPath = new FilePath("feed.atom");

        /// <summary>
        /// The default path for RDF files.
        /// </summary>
        public static readonly FilePath DefaultRdfPath = null;

        private int _maximumItems = 20;
        private ContextConfig _rssPath = _ => DefaultRssPath;
        private ContextConfig _atomPath = _ => DefaultAtomPath;
        private ContextConfig _rdfPath = _ => DefaultRdfPath;

        private ContextConfig _feedId = ctx => ctx.GetLink();
        private ContextConfig _feedTitle = ctx => ctx.String(FeedKeys.Title);
        private ContextConfig _feedDescription = ctx => ctx.String(FeedKeys.Description);
        private ContextConfig _feedAuthor = ctx => ctx.String(FeedKeys.Author);
        private ContextConfig _feedPublished = _ => DateTime.UtcNow;
        private ContextConfig _feedUpdated = _ => DateTime.UtcNow;
        private ContextConfig _feedLink = ctx => ctx.GetLink();
        private ContextConfig _feedImageLink = ctx => ctx.GetLink(ctx, FeedKeys.Image, true);
        private ContextConfig _feedCopyright = ctx => ctx.String(FeedKeys.Copyright) ?? DateTime.UtcNow.Year.ToString();

        private DocumentConfig _itemId = (doc, ctx) => ctx.GetLink(doc, true);
        private DocumentConfig _itemTitle = (doc, _) => doc.String(FeedKeys.Title);
        private DocumentConfig _itemDescription = (doc, _) => doc.String(FeedKeys.Description) ?? doc.String(FeedKeys.Excerpt);
        private DocumentConfig _itemAuthor = (doc, _) => doc.String(FeedKeys.Author);
        private DocumentConfig _itemPublished = (doc, _) => doc.String(FeedKeys.Published);
        private DocumentConfig _itemUpdated = (doc, _) => doc.String(FeedKeys.Updated);
        private DocumentConfig _itemLink = (doc, ctx) => ctx.GetLink(doc, true);
        private DocumentConfig _itemImageLink = (doc, ctx) => ctx.GetLink(doc, FeedKeys.Image, true);
        private DocumentConfig _itemContent = (doc, _) => doc.String(FeedKeys.Content);
        private DocumentConfig _itemThreadLink = null;
        private DocumentConfig _itemThreadCount = null;
        private DocumentConfig _itemThreadUpdated = null;

        /// <summary>
        /// Sets how many items the feed will contain. The default value is 20.
        /// Note that documents will be used in the order in which they are input
        /// into this module, so a <c>OrderBy</c> module or similar should be used
        /// to order the documents prior to this module.
        /// </summary>
        /// <param name="maximumItems">The maximum number of items.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds MaximumItems(int maximumItems)
        {
            _maximumItems = maximumItems;
            return this;
        }

        /// <summary>
        /// Sets the path to the generated RSS file. The default behavior is to generate a RSS feed with
        /// a path of "feed.rss".
        /// </summary>
        /// <param name="rssPath">A delegate that should return a <see cref="FilePath"/> for the RSS path.
        /// If the delegate is <c>null</c> or returns <c>null</c>, no RSS file will be generated.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithRssPath(ContextConfig rssPath)
        {
            _rssPath = rssPath;
            return this;
        }

        /// <summary>
        /// Sets the path to the generated Atom file. The default behavior is to generate a RSS feed with
        /// a path of "feed.atom".
        /// </summary>
        /// <param name="atomPath">A delegate that should return a <see cref="FilePath"/> for the Atom path.
        /// If the delegate is <c>null</c> or returns <c>null</c>, no Atom file will be generated.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithAtomPath(ContextConfig atomPath)
        {
            _atomPath = atomPath;
            return this;
        }

        /// <summary>
        /// Sets the path to the generated RDF file. The default behavior is not to generate a RDF feed.
        /// </summary>
        /// <param name="rdfPath">A delegate that should return a <see cref="FilePath"/> for the RDF path.
        /// If the delegate is <c>null</c> or returns <c>null</c>, no RDF file will be generated.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithRdfPath(ContextConfig rdfPath)
        {
            _rdfPath = rdfPath;
            return this;
        }

        /// <summary>
        /// Sets the feed identifier. The default value is a link to the site.
        /// </summary>
        /// <param name="feedId">A delegate that should return a <c>Uri</c> with
        /// the feed identifier.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithFeedId(ContextConfig feedId)
        {
            _feedId = feedId;
            return this;
        }

        /// <summary>
        /// Sets the feed title. The default value is the value for the "Title" key
        /// in the global metadata.
        /// </summary>
        /// <param name="feedTitle">A delegate that should return a <c>string</c> with
        /// the feed title.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithFeedTitle(ContextConfig feedTitle)
        {
            _feedTitle = feedTitle;
            return this;
        }

        /// <summary>
        /// Sets the feed description. The default value is the value for the "Description" key
        /// in the global metadata.
        /// </summary>
        /// <param name="feedDescription">A delegate that should return a <c>string</c> with
        /// the feed description.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithFeedDescription(ContextConfig feedDescription)
        {
            _feedDescription = feedDescription;
            return this;
        }

        /// <summary>
        /// Sets the feed author. The default value is the value for the "Author" key
        /// in the global metadata.
        /// </summary>
        /// <param name="feedAuthor">A delegate that should return a <c>string</c> with
        /// the feed author.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithFeedAuthor(ContextConfig feedAuthor)
        {
            _feedAuthor = feedAuthor;
            return this;
        }

        /// <summary>
        /// Sets the feed published time. The default value is the current UTC time.
        /// </summary>
        /// <param name="feedPublished">A delegate that should return a <c>DateTime</c> with
        /// the feed published time.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithFeedPublished(ContextConfig feedPublished)
        {
            _feedPublished = feedPublished;
            return this;
        }

        /// <summary>
        /// Sets the feed updated time. The default value is the current UTC time.
        /// </summary>
        /// <param name="feedUpdated">A delegate that should return a <c>DateTime</c> with
        /// the feed updated time.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithFeedUpdated(ContextConfig feedUpdated)
        {
            _feedUpdated = feedUpdated;
            return this;
        }

        /// <summary>
        /// Sets the feed image link. The default value is the site link.
        /// </summary>
        /// <param name="feedLink">A delegate that should return a <c>Uri</c> with
        /// the feed link.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithFeedLink(ContextConfig feedLink)
        {
            _feedLink = feedLink;
            return this;
        }

        /// <summary>
        /// Sets the feed image link. The default value is the value for the "Image" key
        /// in the global metadata.
        /// </summary>
        /// <param name="feedImageLink">A delegate that should return a <c>Uri</c> with
        /// the feed image link.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithFeedImageLink(ContextConfig feedImageLink)
        {
            _feedImageLink = feedImageLink;
            return this;
        }

        /// <summary>
        /// Sets the feed copyright. The default value is the value for the "Copyright" key
        /// in the global metadata.
        /// </summary>
        /// <param name="feedCopyright">A delegate that should return a <c>string</c> with
        /// the feed copyright.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithFeedCopyright(ContextConfig feedCopyright)
        {
            _feedCopyright = feedCopyright;
            return this;
        }

        /// <summary>
        /// Sets the item identifier. The default value is the absolute link to the input document.
        /// </summary>
        /// <param name="itemId">A delegate that should return a <c>Uri</c> with
        /// the item identifier.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithItemId(DocumentConfig itemId)
        {
            _itemId = itemId;
            return this;
        }

        /// <summary>
        /// Sets the item title. The default value is the value for the "Title" key
        /// in the input document.
        /// </summary>
        /// <param name="itemTitle">A delegate that should return a <c>string</c> with
        /// the item title.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithItemTitle(DocumentConfig itemTitle)
        {
            _itemTitle = itemTitle;
            return this;
        }

        /// <summary>
        /// Sets the item description. The default value is the value for the "Description" key
        /// in the input document.
        /// </summary>
        /// <param name="itemDescription">A delegate that should return a <c>string</c> with
        /// the item description.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithItemDescription(DocumentConfig itemDescription)
        {
            _itemDescription = itemDescription;
            return this;
        }

        /// <summary>
        /// Sets the item author. The default value is the value for the "Author" key
        /// in the input document.
        /// </summary>
        /// <param name="itemAuthor">A delegate that should return a <c>string</c> with
        /// the item author.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithItemAuthor(DocumentConfig itemAuthor)
        {
            _itemAuthor = itemAuthor;
            return this;
        }

        /// <summary>
        /// Sets the item published time. The default value is the value for the "Published" key
        /// in the input document.
        /// </summary>
        /// <param name="itemPublished">A delegate that should return a <c>DateTime</c> with
        /// the item published time.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithItemPublished(DocumentConfig itemPublished)
        {
            _itemPublished = itemPublished;
            return this;
        }

        /// <summary>
        /// Sets the item updated time. The default value is the value for the "Updated" key
        /// in the input document.
        /// </summary>
        /// <param name="itemUpdated">A delegate that should return a <c>DateTime</c> with
        /// the item updated time.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithItemUpdated(DocumentConfig itemUpdated)
        {
            _itemUpdated = itemUpdated;
            return this;
        }

        /// <summary>
        /// Sets the item link. The default value is the absolute link to the input document.
        /// </summary>
        /// <param name="itemLink">A delegate that should return a <c>Uri</c> with
        /// the item link.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithItemLink(DocumentConfig itemLink)
        {
            _itemLink = itemLink;
            return this;
        }

        /// <summary>
        /// Sets the item image link. The default value is the value for the "Image" key
        /// in the input document.
        /// </summary>
        /// <param name="itemImageLink">A delegate that should return a <c>Uri</c> with
        /// the item image link.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithItemImageLink(DocumentConfig itemImageLink)
        {
            _itemImageLink = itemImageLink;
            return this;
        }

        /// <summary>
        /// Sets the content of the item. The default value is the value for the "Content" key
        /// in the input document. Note that the entire document content is not used because
        /// it will most likely contain layout, scripts, and other code that shouldn't be part
        /// of the feed item.
        /// </summary>
        /// <param name="itemContent">A delegate that should return a <c>string</c> with
        /// the content of the item.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithItemContent(DocumentConfig itemContent)
        {
            _itemContent = itemContent;
            return this;
        }

        /// <summary>
        /// Sets the item thread link. By default, no thread link is specified.
        /// </summary>
        /// <param name="itemThreadLink">A delegate that should return a <c>Uri</c> with
        /// the item thread link.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithItemThreadLink(DocumentConfig itemThreadLink)
        {
            _itemThreadLink = itemThreadLink;
            return this;
        }

        /// <summary>
        /// Sets the item thread count. By default, no thread count is specified.
        /// </summary>
        /// <param name="itemThreadCount">A delegate that should return an <c>int</c> with
        /// the item thread count.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithItemThreadCount(DocumentConfig itemThreadCount)
        {
            _itemThreadCount = itemThreadCount;
            return this;
        }

        /// <summary>
        /// Sets the item thread updated. By default, no thread updated time is specified.
        /// </summary>
        /// <param name="itemThreadUpdated">A delegate that should return a <c>DateTime</c> with
        /// the item thread updated time.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithItemThreadUpdated(DocumentConfig itemThreadUpdated)
        {
            _itemThreadUpdated = itemThreadUpdated;
            return this;
        }

        /// <inheritdoc />
        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            // Get the feed
            Feed feed = new Feed
            {
                ID = _feedId?.Invoke<Uri>(context, "while reading feed ID from settings"),
                Title = _feedTitle?.Invoke<string>(context, "while reading feed title from settings"),
                Description = _feedDescription?.Invoke<string>(context, "while reading feed description from settings"),
                Author = _feedAuthor?.Invoke<string>(context, "while reading feed author from settings"),
                Published = _feedPublished?.Invoke<DateTime?>(context, "while reading feed published date from settings"),
                Updated = _feedUpdated?.Invoke<DateTime?>(context, "while reading feed updated date from settings"),
                Link = _feedLink?.Invoke<Uri>(context, "while reading feed link from settings"),
                ImageLink = _feedImageLink?.Invoke<Uri>(context, "while reading feed image from settings"),
                Copyright = _feedCopyright?.Invoke<string>(context, "while reading feed copyright from settings")
            };

            // Add items
            context.ForEach(inputs.Take(_maximumItems), input =>
            {
                feed.Items.Add(new FeedItem
                {
                    ID = _itemId?.Invoke<Uri>(input, context, "while reading ID for feed"),
                    Title = _itemTitle?.Invoke<string>(input, context, "while reading title for feed"),
                    Description = _itemDescription?.Invoke<string>(input, context, "while reading description for feed"),
                    Author = _itemAuthor?.Invoke<string>(input, context, "while reading author for feed"),
                    Published = _itemPublished?.Invoke<DateTime?>(input, context, "while reading published date for feed"),
                    Updated = _itemUpdated?.Invoke<DateTime?>(input, context, "while reading updated date for feed"),
                    Link = _itemLink?.Invoke<Uri>(input, context, "while reading feed link for feed"),
                    ImageLink = _itemImageLink?.Invoke<Uri>(input, context, "while reading image for feed"),
                    Content = _itemContent?.Invoke<string>(input, context, "while reading content for feed"),
                    ThreadLink = _itemThreadLink?.Invoke<Uri>(input, context, "while reading thread link for feed"),
                    ThreadCount = _itemThreadCount?.Invoke<int?>(input, context, "while reading thread count for feed"),
                    ThreadUpdated = _itemThreadUpdated?.Invoke<DateTime?>(input, context, "while reading thread updated for feed")
                });
            });

            // Generate the feeds
            return new[]
            {
                GenerateFeed(FeedType.Rss, feed, _rssPath, context),
                GenerateFeed(FeedType.Atom, feed, _atomPath, context),
                GenerateFeed(FeedType.Rdf, feed, _rdfPath, context)
            }.Where(x => x != null);
        }

        private IDocument GenerateFeed(FeedType feedType, Feed feed, ContextConfig path, IExecutionContext context)
        {
            // Get the output path
            FilePath outputPath = path?.Invoke<FilePath>(context, "while getting output path");
            if (outputPath == null)
            {
                return null;
            }
            if (!outputPath.IsRelative)
            {
                throw new ArgumentException("The feed output path must be relative");
            }

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
