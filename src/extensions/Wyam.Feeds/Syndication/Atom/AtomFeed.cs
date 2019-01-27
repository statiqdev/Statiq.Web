using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;

namespace Wyam.Feeds.Syndication.Atom
{
    /// <summary>
    /// The Atom Syndication Format
    ///     http://tools.ietf.org/html/rfc4287#section-4.1.1
    /// </summary>
    /// <remarks>
    /// atomFeed : atomSource
    ///     atomLogo?
    ///     atomEntry*
    /// </remarks>
    [Serializable]
    [XmlRoot(RootElement, Namespace=Namespace)]
    public class AtomFeed : AtomSource, IFeed
    {
        public const string SpecificationUrl = "http://tools.ietf.org/html/rfc4287";
        protected internal const string Prefix = "";
        protected internal const string Namespace = "http://www.w3.org/2005/Atom";
        protected internal const string RootElement = "feed";
        public const string MimeType = "application/atom+xml";

        public AtomFeed()
        {
        }

        public AtomFeed(IFeed source)
        {
            // ** IFeedMetadata

            // ID
            Id = source.ID.ToString();

            // Title
            string title = source.Title;
            if (!string.IsNullOrWhiteSpace(title))
            {
                Title = title;
            }

            // Description
            string description = source.Description;
            if (!string.IsNullOrEmpty(description))
            {
                SubTitle = description;
            }

            // Author
            string author = source.Author;
            if (!string.IsNullOrEmpty(author))
            {
                Authors.Add(new AtomPerson
                {
                    Name = author
                });
            }

            // Published
            DateTime? published = source.Published;
            if (published.HasValue)
            {
                Updated = published.Value;
            }

            // Updated
            DateTime? updated = source.Updated;
            if (updated.HasValue)
            {
                Updated = updated.Value;
            }

            // Link
            Uri link = source.Link;
            if (link != null)
            {
                Links.Add(new AtomLink
                {
                    Href = link.ToString(),
                    Rel = "self"
                });
            }

            // ImageLink
            Uri imageLink = source.ImageLink;
            if (imageLink != null)
            {
                Logo = imageLink.ToString();
            }

            // ** IFeed

            // Copyright
            string copyright = source.Copyright;
            if (!string.IsNullOrEmpty(copyright))
            {
                Rights = new AtomText(copyright);
            }

            // Items
            IList<IFeedItem> sourceItems = source.Items;
            if (sourceItems != null)
            {
                Entries.AddRange(sourceItems.Select(x => new AtomEntry(x)));
            }
        }

        [XmlElement("entry")]
        public List<AtomEntry> Entries { get; } = new List<AtomEntry>();

        [XmlIgnore]
        public bool EntriesSpecified
        {
            get { return Entries.Count > 0; }
            set { }
        }

        [XmlIgnore]
        FeedType IFeed.FeedType => FeedType.Atom;

        string IFeed.MimeType => MimeType;

        string IFeed.Copyright => Rights?.StringValue;

        IList<IFeedItem> IFeed.Items => Entries.Cast<IFeedItem>().ToList();

        Uri IFeedMetadata.ID => ((IUriProvider)this).Uri;

        string IFeedMetadata.Title => Title?.StringValue;

        string IFeedMetadata.Description
        {
            get
            {
                if (SubTitle == null)
                {
                    return null;
                }
                return SubTitle.StringValue;
            }
        }

        string IFeedMetadata.Author
        {
            get
            {
                if (!AuthorsSpecified)
                {
                    if (!ContributorsSpecified)
                    {
                        return null;
                    }
                    foreach (AtomPerson person in Contributors)
                    {
                        if (!string.IsNullOrEmpty(person.Name))
                        {
                            return person.Name;
                        }
                        if (!string.IsNullOrEmpty(person.Email))
                        {
                            return person.Name;
                        }
                    }
                }

                foreach (AtomPerson person in Authors)
                {
                    if (!string.IsNullOrEmpty(person.Name))
                    {
                        return person.Name;
                    }
                    if (!string.IsNullOrEmpty(person.Email))
                    {
                        return person.Name;
                    }
                }

                return null;
            }
        }

        DateTime? IFeedMetadata.Published => ((IFeedMetadata)this).Updated;

        DateTime? IFeedMetadata.Updated
        {
            get
            {
                if (!Updated.HasValue)
                {
                    return null;
                }

                return Updated.Value;
            }
        }

        Uri IFeedMetadata.Link
        {
            get
            {
                if (!LinksSpecified)
                {
                    return null;
                }

                Uri self = null;
                Uri alternate = null;
                foreach (AtomLink link in Links)
                {
                    if ("self".Equals(link.Rel))
                    {
                        self = ((IUriProvider)link).Uri;
                    }
                    else if ("alternate".Equals(link.Rel))
                    {
                        alternate = ((IUriProvider)link).Uri;
                    }
                    else if (alternate == null && !"self".Equals(link.Rel))
                    {
                        alternate = ((IUriProvider)link).Uri;
                    }
                }
                return self ?? alternate;
            }
        }

        Uri IFeedMetadata.ImageLink
        {
            get
            {
                if (LogoUri == null)
                {
                    return IconUri;
                }
                return LogoUri;
            }
        }

        public override void AddNamespaces(XmlSerializerNamespaces namespaces)
        {
            namespaces.Add(Prefix, Namespace);
            namespaces.Add(XmlPrefix, XmlNamespace);

            foreach (AtomEntry entry in Entries)
            {
                entry.AddNamespaces(namespaces);
            }

            base.AddNamespaces(namespaces);
        }
    }
}
