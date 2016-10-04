using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Wyam.Feeds.Syndication.Atom
{
    /// <summary>
    /// Adapter for Atom 0.3 compatibility
    /// </summary>
    [Serializable]
    [XmlRoot(RootElement, Namespace=Namespace)]
    public class AtomFeedOld : AtomSource, IFeed
    {
        public const string SpecificationUrl = "http://www.mnot.net/drafts/draft-nottingham-atom-format-02.html";
        protected internal const string Prefix = "";
        protected internal const string Namespace = "http://purl.org/atom/ns#";
        protected internal const string RootElement = "feed";
        protected internal const string MimeType = "application/atom+xml";

        private Version _version = new Version(0, 3);

        /// <summary>
        /// Ctor
        /// </summary>
        [Obsolete("Atom 0.3 is for backwards compatibility and should only be used for deserialization", true)]
        public AtomFeedOld()
        {
        }

        [DefaultValue(null)]
        [XmlAttribute("version")]
        public string Version
        {
            get { return (_version == null) ? null : _version.ToString(); }
            set { _version = string.IsNullOrEmpty(value) ? null : new Version(value); }
        }

        [DefaultValue(null)]
        [XmlElement("tagline")]
        public AtomText TagLine
        {
            get { return SubTitle; }
            set { SubTitle = value; }
        }

        [DefaultValue(null)]
        [XmlElement("copyright")]
        public AtomText Copyright
        {
            get { return Rights; }
            set { Rights = value; }
        }

        [DefaultValue(null)]
        [XmlElement("modified")]
        public AtomDate Modified
        {
            get { return base.Updated; }
            set { base.Updated = value; }
        }

        [DefaultValue(0)]
        [XmlElement("fullcount")]
        public int FullCount
        {
            get
            {
                if (Entries == null)
                {
                    return 0;
                }
                return Entries.Count;
            }
            set { }
        }

        [XmlElement("entry")]
        public readonly List<AtomEntryOld> Entries = new List<AtomEntryOld>();

        [XmlIgnore]
        public bool EntriesSpecified
        {
            get { return (Entries.Count > 0); }
            set { }
        }

        [XmlIgnore]
        public override bool SubTitleSpecified
        {
            get { return false; }
            set { }
        }

        [XmlIgnore]
        public override bool UpdatedSpecified
        {
            get { return false; }
            set { }
        }

        [XmlIgnore]
        FeedType IFeed.FeedType => FeedType.Atom;

        string IFeed.MimeType => MimeType;

        string IFeed.Copyright => Rights?.StringValue;

        IList<IFeedItem> IFeed.Items => Entries.ToArray();

        Uri IFeedMetadata.ID => ((IUriProvider)this).Uri;

        string IFeedMetadata.Title
        {
            get
            {
                if (Title == null)
                {
                    return null;
                }
                return Title.StringValue;
            }
        }

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

                Uri alternate = null;
                foreach (AtomLink link in Links)
                {
                    switch (link.Relation)
                    {
                        case AtomLinkRelation.Alternate:
                        {
                            return ((IUriProvider)link).Uri;
                        }
                        case AtomLinkRelation.Related:
                        case AtomLinkRelation.Enclosure:
                        {
                            if (alternate == null)
                            {
                                alternate = ((IUriProvider)link).Uri;
                            }
                            break;
                        }
                        default:
                        {
                            continue;
                        }
                    }
                }

                return alternate;
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