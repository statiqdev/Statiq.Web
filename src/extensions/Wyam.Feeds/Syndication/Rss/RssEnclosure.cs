using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Wyam.Feeds.Syndication.Rss
{
    /// <summary>
    /// RSS 2.0 Enclosure
    ///		http://blogs.law.harvard.edu/tech/rss#ltenclosuregtSubelementOfLtitemgt
    ///		http://www.thetwowayweb.com/payloadsforrss
    ///		http://www.reallysimplesyndication.com/discuss/msgReader$221
    /// </summary>
    [Serializable]
    public class RssEnclosure : RssBase, IUriProvider
    {
        private Uri _url = null;
        private long _length = 0;
        private string _type = null;

        /// <summary>
        /// Gets and sets the URL where the enclosure is located.
        /// </summary>
        [DefaultValue(null)]
        [XmlAttribute("url")]
        public string Url
        {
            get { return ConvertToString(_url); }
            set { _url = ConvertToUri(value); }
        }

        /// <summary>
        /// Gets and sets the length of the enclosure in bytes.
        /// </summary>
        [XmlAttribute("length")]
        public long Length
        {
            get { return _length; }
            set { _length = value < 0L ? 0L : value; }
        }

        /// <summary>
        /// Gets and sets the MIME type for the resource.
        /// </summary>
        [DefaultValue(null)]
        [XmlAttribute("type")]
        public string Type
        {
            get { return _type; }
            set { _type = string.IsNullOrEmpty(value) ? null : value; }
        }

        [XmlIgnore]
        public bool HasValue =>
            (Length > 0)
            || !string.IsNullOrEmpty(Url)
            && !string.IsNullOrEmpty(Type);

        Uri IUriProvider.Uri => _url;
    }
}