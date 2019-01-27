using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Wyam.Feeds.Syndication.Atom
{
    /// <summary>
    /// http://tools.ietf.org/html/rfc4287#section-4.1.2
    /// </summary>
    /// <remarks>
    /// atomEntry : atomBase
    ///     atomContent?
    ///     atomPublished?
    ///     atomSource?
    ///     atomSummary?
    /// </remarks>
    [Serializable]
    public class AtomEntry : AtomBase, IFeedItem
    {
        private AtomContent _content = null;
        private AtomDate _published;
        private AtomSource _source = null;
        private AtomText _summary = null;
        private int? _threadTotal = null;

        public AtomEntry()
        {
        }

        public AtomEntry(IFeedItem source)
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
                Summary = description;
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
                Links.Add(new AtomLink(link.ToString()));
            }

            // ImageLink
            Uri imageLink = source.ImageLink;
            if (imageLink != null)
            {
                Links.Add(new AtomLink
                {
                    Type = "image",
                    Href = imageLink.ToString(),
                    Relation = AtomLinkRelation.Enclosure
                });
            }

            // ** IFeedItem

            // Content
            string content = source.Content;
            if (!string.IsNullOrEmpty(content))
            {
                Content = new AtomContent(content);
            }

            // ThreadLink
            Uri threadLink = source.ThreadLink;
            AtomLink replies = null;
            if (threadLink != null)
            {
                replies = new AtomLink
                {
                    Href = threadLink.ToString(),
                    Relation = AtomLinkRelation.Replies
                };
                Links.Add(replies);
            }

            // ThreadCount
            int? threadCount = source.ThreadCount;
            if (threadCount.HasValue && replies != null)
            {
                replies.ThreadCount = threadCount.Value;
            }

            // ThreadUpdated
            DateTime? threadUpdated = source.ThreadUpdated;
            if (threadUpdated.HasValue && replies != null)
            {
                replies.ThreadUpdated = threadUpdated.Value;
            }
        }

        [DefaultValue(null)]
        [XmlElement("content")]
        public AtomContent Content
        {
            get { return _content; }
            set { _content = value; }
        }

        [DefaultValue(null)]
        [XmlElement("published")]
        public virtual AtomDate Published
        {
            get { return _published; }
            set { _published = value; }
        }

        [XmlIgnore]
        public virtual bool PublishedSpecified
        {
            get { return _published.HasValue; }
            set { }
        }

        [DefaultValue(null)]
        [XmlElement("source")]
        public AtomSource Source
        {
            get { return _source; }
            set { _source = value; }
        }

        [DefaultValue(null)]
        [XmlElement("summary")]
        public AtomText Summary
        {
            get { return _summary; }
            set { _summary = value; }
        }

        [XmlElement("in-reply-to", Namespace = ThreadingNamespace)]
        public List<AtomInReplyTo> InReplyToReferences { get; } = new List<AtomInReplyTo>();

        [XmlIgnore]
        public bool InReplyToReferencesSpecified
        {
            get { return InReplyToReferences.Count > 0; }
            set { }
        }

        /// <summary>
        /// http://tools.ietf.org/html/rfc4685#section-5
        /// </summary>
        [XmlElement(ElementName="total", Namespace=ThreadingNamespace)]
        public int ThreadTotal
        {
            get
            {
                if (!_threadTotal.HasValue)
                {
                    return 0;
                }
                return _threadTotal.Value;
            }
            set
            {
                if (value < 0)
                {
                    _threadTotal = null;
                }
                else
                {
                    _threadTotal = value;
                }
            }
        }

        [XmlIgnore]
        public bool ThreadTotalSpecified
        {
            get { return _threadTotal.HasValue; }
            set { }
        }

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

        string IFeedMetadata.Description =>
            _summary == null ? _content?.StringValue : _summary.StringValue;

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

        DateTime? IFeedMetadata.Published
        {
            get
            {
                if (!Published.HasValue)
                {
                    return ((IFeedMetadata)this).Updated;
                }

                return Published.Value;
            }
        }

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

                if (alternate == null && _content != null)
                {
                    alternate = ((IUriProvider)_content).Uri;
                }

                return alternate;
            }
        }

        Uri IFeedMetadata.ImageLink
        {
            get
            {
                if (!LinksSpecified)
                {
                    return null;
                }

                foreach (AtomLink link in Links)
                {
                    if (link.Relation == AtomLinkRelation.Enclosure)
                    {
                        string type = link.Type;
                        if (!string.IsNullOrEmpty(type)
                            && type.StartsWith("image", StringComparison.InvariantCultureIgnoreCase))
                        {
                            return ((IUriProvider)link).Uri;
                        }
                    }
                }

                return null;
            }
        }

        string IFeedItem.Content => _content?.StringValue;

        Uri IFeedItem.ThreadLink
        {
            get
            {
                if (!LinksSpecified)
                {
                    return null;
                }

                foreach (AtomLink link in Links)
                {
                    if (link.Relation == AtomLinkRelation.Replies)
                    {
                        return ((IUriProvider)link).Uri;
                    }
                }

                return null;
            }
        }

        int? IFeedItem.ThreadCount
        {
            get
            {
                if (LinksSpecified)
                {
                    foreach (AtomLink link in Links)
                    {
                        if (link.Relation == AtomLinkRelation.Replies
                        && link.ThreadCountSpecified)
                        {
                            return link.ThreadCount;
                        }
                    }
                }
                return _threadTotal;
            }
        }

        DateTime? IFeedItem.ThreadUpdated
        {
            get
            {
                if (LinksSpecified)
                {
                    foreach (AtomLink link in Links)
                    {
                        if (link.Relation == AtomLinkRelation.Replies
                        && link.ThreadUpdatedSpecified)
                        {
                            return link.ThreadUpdated.Value;
                        }
                    }
                }
                return null;
            }
        }

        public override void AddNamespaces(XmlSerializerNamespaces namespaces)
        {
            if (ThreadTotal > 0)
            {
                namespaces.Add(ThreadingPrefix, ThreadingNamespace);
            }

            if (InReplyToReferencesSpecified)
            {
                foreach (AtomInReplyTo inReplyTo in InReplyToReferences)
                {
                    inReplyTo.AddNamespaces(namespaces);
                }
            }

            base.AddNamespaces(namespaces);
        }
    }
}
