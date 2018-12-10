using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using Wyam.Feeds.Syndication.Extensions;

namespace Wyam.Feeds.Syndication.Rdf
{
    /// <summary>
    /// RDF 1.0 Root
    ///     http://web.resource.org/rss/1.0/spec#s5.2
    /// </summary>
    /// <remarks>
    /// XmlSerializer serializes public fields before public properties
    /// and serializes base class members before derriving class members.
    /// Since RssChannel uses a readonly field for Items it must be placed
    /// in a derriving class in order to make sure items serialize last.
    /// </remarks>
    [XmlRoot(RootElement, Namespace=NamespaceRdf)]
    public class RdfFeed : RdfFeedBase, IFeed
    {
        public RdfFeed()
        {
        }

        public RdfFeed(IFeed source)
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
                Channel.DublinCore[DublinCore.TermName.Creator] = author;
            }

            // Published
            DateTime? published = source.Published;
            if (published.HasValue)
            {
                Channel.DublinCore[DublinCore.TermName.Date] = ConvertToString(published.Value);
            }

            // Updated
            DateTime? updated = source.Updated;
            if (updated.HasValue)
            {
                Channel.DublinCore[DublinCore.TermName.Date] = ConvertToString(updated.Value);
            }

            // Link
            Uri link = source.Link;
            if (link != null)
            {
                Channel.Link = link.ToString();
            }

            // ImageLink
            // No images in RDF

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
                Items.AddRange(sourceItems.Select(x => new RdfItem(x)));
            }
        }

        [XmlElement("item", Namespace = NamespaceRss10)]
        public List<RdfItem> Items { get; } = new List<RdfItem>();

        [XmlIgnore]
        public bool ItemsSpecified
        {
            get { return Items.Count > 0; }
            set { }
        }

        [XmlIgnore]
        public RdfItem this[int index]
        {
            get { return Items[index]; }
            set { Items[index] = value; }
        }

        [XmlIgnore]
        FeedType IFeed.FeedType => FeedType.Rdf;

        string IFeed.MimeType => MimeType;

        string IFeed.Copyright => Channel.Copyright;

        IList<IFeedItem> IFeed.Items => Items.Cast<IFeedItem>().ToArray();

        Uri IFeedMetadata.ID => ((IFeedMetadata)Channel).ID;

        string IFeedMetadata.Title => ((IFeedMetadata)Channel).Title;

        string IFeedMetadata.Description => ((IFeedMetadata)Channel).Description;

        string IFeedMetadata.Author => ((IFeedMetadata)Channel).Author;

        DateTime? IFeedMetadata.Published => ((IFeedMetadata)Channel).Published;

        DateTime? IFeedMetadata.Updated => ((IFeedMetadata)Channel).Updated;

        Uri IFeedMetadata.Link => ((IFeedMetadata)Channel).Link;

        Uri IFeedMetadata.ImageLink => ((IUriProvider)Image).Uri;

        public override void AddNamespaces(XmlSerializerNamespaces namespaces)
        {
            namespaces.Add(string.Empty, NamespaceRss10);
            namespaces.Add("rdf", NamespaceRdf);

            Channel.AddNamespaces(namespaces);
            if (ImageSpecified)
            {
                Image.AddNamespaces(namespaces);
            }
            if (TextInputSpecified)
            {
                TextInput.AddNamespaces(namespaces);
            }

            foreach (RdfItem item in Items)
            {
                item.AddNamespaces(namespaces);
            }

            base.AddNamespaces(namespaces);
        }
    }
}
