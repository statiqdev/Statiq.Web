using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Wyam.Feeds.Syndication.Rss
{
    /// <summary>
    /// RSS 2.0 Source
    ///     http://blogs.law.harvard.edu/tech/rss#ltsourcegtSubelementOfLtitemgt
    /// </summary>
    [Serializable]
    public class RssSource : RssBase
    {
        private Uri _url = null;
        private string _value = null;

        /// <summary>
        /// Gets and sets the url of the source.
        /// </summary>
        [DefaultValue(null)]
        [XmlAttribute("url")]
        public string Url
        {
            get { return ConvertToString(_url); }
            set { _url = ConvertToUri(value); }
        }

        /// <summary>
        /// Gets and sets the name of the RSS channel that the item came from.
        /// </summary>
        [DefaultValue(null)]
        [XmlText]
        public string Value
        {
            get { return _value; }
            set { _value = string.IsNullOrEmpty(value) ? null : value; }
        }
    }
}