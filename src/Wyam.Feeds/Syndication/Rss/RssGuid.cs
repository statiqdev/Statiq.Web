using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Wyam.Feeds.Syndication.Rss
{
    /// <summary>
    /// RSS 2.0 Guid
    ///		http://blogs.law.harvard.edu/tech/rss#ltguidgtSubelementOfLtitemgt
    /// </summary>
    [Serializable]
    public class RssGuid : RssBase, IUriProvider
    {
        private bool _isPermaLink = true;
        private Uri _value = null;

        /// <summary>
        /// Gets and sets if the identifier is a permanent URL.
        /// </summary>
        [DefaultValue(true)]
        [XmlAttribute("isPermaLink")]
        public bool IsPermaLink
        {
            get
            {
                string link = Value;

                return _isPermaLink &&
                       (link != null) &&
                       link.StartsWith(Uri.UriSchemeHttp);
            }
            set { _isPermaLink = value; }
        }

        /// <summary>
        /// Gets and sets the globally unique identifier, may be an url or other unique string.
        /// </summary>
        [DefaultValue(null)]
        [XmlText]
        public string Value
        {
            get { return ConvertToString(_value); }
            set { _value = ConvertToUri(value); }
        }

        [XmlIgnore]
        public bool HasValue => !string.IsNullOrEmpty(Value);

        Uri IUriProvider.Uri => _value;
    }
}