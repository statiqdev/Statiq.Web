using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Wyam.Feeds.Syndication.Rss
{
    /// <summary>
    /// Really Simple Syndication (RSS 2.0)
    ///     http://www.rssboard.org/rss-specification
    ///     http://blogs.law.harvard.edu/tech/rss
    /// </summary>
    [XmlRoot(RootElement, Namespace=Namespace)]
    public class RssFeed : RssBase, IFeed
    {
        public const string SpecificationUrl = "http://blogs.law.harvard.edu/tech/rss";
        protected internal const string RootElement = "rss";
        protected internal const string Namespace = "";
        public const string MimeType = "application/rss+xml";

        private RssChannel _channel = null;
        private Version _version = new Version(2, 0);

        public RssFeed()
        {
        }

        public RssFeed(IFeed source)
        {
            // ** IFeedMetadata

            // ID
            Channel.Link = source.ID.ToString();

            // Title
            string title = source.Title;
            if (!string.IsNullOrWhiteSpace(title))
            {
                Channel.Title = title;
            }

            // Description
            string description = source.Description;
            if (!string.IsNullOrEmpty(description))
            {
                Channel.Description = description;
            }

            // Author
            string author = source.Author;
            if (!string.IsNullOrEmpty(author))
            {
                Channel.ManagingEditor = new RssPerson
                {
                    Name = author
                };
            }

            // Published
            DateTime? published = source.Published;
            if (published.HasValue)
            {
                Channel.PubDate = new RssDate(published.Value);
            }

            // Updated
            DateTime? updated = source.Updated;
            if (updated.HasValue)
            {
                Channel.LastBuildDate = new RssDate(updated.Value);
            }

            // Link
            Uri link = source.Link;
            if (link != null)
            {
                Channel.Link = link.ToString();
            }

            // ImageLink
            Uri imageLink = source.ImageLink;
            if (imageLink != null)
            {
                Channel.Image = new RssImage
                {
                    Url = imageLink.ToString()
                };
            }

            // ** IFeed

            // Copyright
            string copyright = source.Copyright;
            if (!string.IsNullOrEmpty(copyright))
            {
                Channel.Copyright = copyright;
            }

            // Items
            IList<IFeedItem> sourceItems = source.Items;
            if (sourceItems != null)
            {
                Channel.Items.AddRange(sourceItems.Select(x => new RssItem(x)));
            }
        }

        [XmlElement("channel")]
        public RssChannel Channel
        {
            get { return _channel ?? (_channel = new RssChannel()); }
            set { _channel = value; }
        }

        [XmlAttribute("version")]
        public string Version
        {
            get { return _version?.ToString(); }
            set { _version = string.IsNullOrEmpty(value) ? null : new Version(value); }
        }

        [XmlIgnore]
        FeedType IFeed.FeedType => FeedType.Rss;

        string IFeed.MimeType => MimeType;

        string IFeed.Copyright => Channel.Copyright;

        IList<IFeedItem> IFeed.Items => Channel.Items.Cast<IFeedItem>().ToArray();

        Uri IFeedMetadata.ID => ((IUriProvider)Channel).Uri;

        string IFeedMetadata.Title => Channel.Title;

        string IFeedMetadata.Description => Channel.Description;

        string IFeedMetadata.Author
        {
            get
            {
                if (!Channel.ManagingEditorSpecified)
                {
                    if (!Channel.WebMasterSpecified)
                    {
                        return null;
                    }
                    if (string.IsNullOrEmpty(Channel.WebMaster.Name))
                    {
                        return Channel.WebMaster.Email;
                    }
                    return Channel.WebMaster.Name;
                }
                if (string.IsNullOrEmpty(Channel.ManagingEditor.Name))
                {
                    return Channel.ManagingEditor.Email;
                }
                return Channel.ManagingEditor.Name;
            }
        }

        DateTime? IFeedMetadata.Published
        {
            get
            {
                if (!Channel.PubDate.HasValue)
                {
                    return ((IFeedMetadata)this).Updated;
                }

                return Channel.PubDate.Value;
            }
        }

        DateTime? IFeedMetadata.Updated
        {
            get
            {
                if (!Channel.LastBuildDate.HasValue)
                {
                    return null;
                }

                return Channel.LastBuildDate.Value;
            }
        }

        Uri IFeedMetadata.Link => ((IUriProvider)Channel).Uri;

        Uri IFeedMetadata.ImageLink
        {
            get
            {
                if (!Channel.ImageSpecified)
                {
                    return null;
                }
                return ((IUriProvider)Channel.Image).Uri;
            }
        }

        public override void AddNamespaces(XmlSerializerNamespaces namespaces)
        {
            namespaces.Add("", Namespace);

            Channel.AddNamespaces(namespaces);

            base.AddNamespaces(namespaces);
        }
    }
}
