using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Wyam.Feeds.Syndication.Atom
{
    /// <summary>
    /// http://tools.ietf.org/html/rfc4287#section-4.2.7
    /// </summary>
    [Serializable]
    public class AtomLink : AtomCommonAttributes, IUriProvider
    {
        private AtomLinkRelation _relation = AtomLinkRelation.None;
        private Uri _rel = null;
        private string _type = null;
        private Uri _href = null;
        private string _hreflang = null;
        private string _title = null;
        private string _length = null;

        private int _threadCount = 0;
        private AtomDate _threadUpdated;

        /// <summary>
        /// Ctor
        /// </summary>
        public AtomLink()
        {
        }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="link"></param>
        public AtomLink(string link)
        {
            Href = link;
        }

        [XmlIgnore]
        [DefaultValue(AtomLinkRelation.None)]
        public AtomLinkRelation Relation
        {
            get
            {
                // http://tools.ietf.org/html/rfc4685#section-4
                if (ThreadCount > 0)
                {
                    return AtomLinkRelation.Replies;
                }
                return _relation;
            }
            set
            {
                _relation = value;
                _rel = null;
            }
        }

        [XmlAttribute("rel")]
        [DefaultValue(null)]
        public string Rel
        {
            get
            {
                if (Relation == AtomLinkRelation.None)
                {
                    return ConvertToString(_rel);
                }

                // TODO: use XmlEnum values
                switch (_relation)
                {
                    case AtomLinkRelation.NextArchive:
                    {
                        return "next-archive";
                    }
                    case AtomLinkRelation.PrevArchive:
                    {
                        return "prev-archive";
                    }
                    default:
                    {
                        return _relation.ToString().ToLowerInvariant();
                    }
                }
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _relation = AtomLinkRelation.None;
                    _rel = null;
                }

                try
                {
                    // TODO: use XmlEnum values
                    _relation = (AtomLinkRelation)Enum.Parse(typeof(AtomLinkRelation), value.Replace("-",""), true);
                }
                catch
                {
                    _relation = AtomLinkRelation.None;
                    _rel = ConvertToUri(value);
                }
            }
        }

        [XmlAttribute("type")]
        [DefaultValue(null)]
        public string Type
        {
            get
            {
                // http://tools.ietf.org/html/rfc4685#section-4
                string value = _type;
                if (ThreadCount > 0 && string.IsNullOrEmpty(value))
                {
                    return AtomFeed.MimeType;
                }
                return value;
            }
            set { _type = string.IsNullOrEmpty(value) ? null : value; }
        }

        [XmlAttribute("href")]
        [DefaultValue(null)]
        public string Href
        {
            get { return ConvertToString(_href); }
            set { _href = ConvertToUri(value); }
        }

        [XmlAttribute("hreflang")]
        [DefaultValue(null)]
        public string HrefLang
        {
            get { return _hreflang; }
            set { _hreflang = string.IsNullOrEmpty(value) ? null : value; }
        }

        [XmlAttribute("length")]
        [DefaultValue(null)]
        public string Length
        {
            get { return _length; }
            set { _length = string.IsNullOrEmpty(value) ? null : value; }
        }

        [XmlAttribute("title")]
        [DefaultValue(null)]
        public string Title
        {
            get { return _title; }
            set { _title = string.IsNullOrEmpty(value) ? null : value; }
        }

        /// <summary>
        /// http://tools.ietf.org/html/rfc4685#section-4
        /// </summary>
        [XmlAttribute("count", Namespace=ThreadingNamespace)]
        public int ThreadCount
        {
            get { return _threadCount; }
            set { _threadCount = (value < 0) ? 0 : value; }
        }

        [XmlIgnore]
        public bool ThreadCountSpecified
        {
            get { return (Relation == AtomLinkRelation.Replies); }
            set { }
        }

        /// <summary>
        /// http://tools.ietf.org/html/rfc4685#section-4
        /// </summary>
        [XmlIgnore]
        public AtomDate ThreadUpdated
        {
            get { return _threadUpdated; }
            set { _threadUpdated = value; }
        }

        /// <summary>
        /// Gets and sets the DateTime using ISO-8601 date format.
        /// For serialization purposes only, use the ThreadUpdated property instead.
        /// </summary>
        [DefaultValue(null)]
        [XmlAttribute("updated", Namespace=ThreadingNamespace)]
        public string ThreadUpdatedIso8601
        {
            get { return _threadUpdated.ValueIso8601; }
            set { _threadUpdated.ValueIso8601 = value; }
        }

        [XmlIgnore]
        public bool ThreadUpdatedSpecified
        {
            get { return (Relation == AtomLinkRelation.Replies) && _threadUpdated.HasValue; }
            set { }
        }

        Uri IUriProvider.Uri => _href;

        public override string ToString()
        {
            return Href;
        }

        public override void AddNamespaces(XmlSerializerNamespaces namespaces)
        {
            if (ThreadCount > 0)
            {
                namespaces.Add(ThreadingPrefix, ThreadingNamespace);
            }

            base.AddNamespaces(namespaces);
        }
    }
}