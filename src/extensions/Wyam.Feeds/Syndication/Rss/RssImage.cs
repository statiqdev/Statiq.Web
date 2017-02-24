using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Wyam.Feeds.Syndication.Rss
{
    /// <summary>
    /// RSS 2.0 Image
    ///     http://blogs.law.harvard.edu/tech/rss#ltimagegtSubelementOfLtchannelgt
    /// </summary>
    [Serializable]
    public class RssImage : RssBase, IUriProvider
    {
        // required
        private Uri _url = null;
        private string _title = null;
        private Uri _link = null;

        //optional
        private int _width = int.MinValue;
        private int _height = int.MinValue;

        /// <summary>
        /// Gets and sets the url to which the image is linked.
        /// </summary>
        [DefaultValue(null)]
        [XmlElement("url")]
        public string Url
        {
            get { return ConvertToString(_url); }
            set { _url = ConvertToUri(value); }
        }

        /// <summary>
        /// Gets and sets the title of the image (alternate text).
        /// </summary>
        [DefaultValue(null)]
        [XmlElement("title")]
        public string Title
        {
            get { return _title; }
            set { _title = string.IsNullOrEmpty(value) ? null : value; }
        }

        /// <summary>
        /// Gets and sets the url to which the image is linked.
        /// </summary>
        /// <remarks>
        /// Required even if empty.
        /// </remarks>
        [XmlElement("link")]
        public string Link
        {
            get { return ConvertToString(_link); }
            set { _link = ConvertToUri(value); }
        }

        /// <summary>
        /// Gets and sets the width of the image.
        /// </summary>
        [DefaultValue(int.MinValue)]
        [XmlElement("width")]
        public int Width
        {
            get { return _width; }
            set
            {
                if (value <= 0)
                    _width = int.MinValue;
                else
                    _width = value;
            }
        }

        /// <summary>
        /// Gets and sets the height of the image.
        /// </summary>
        [DefaultValue(int.MinValue)]
        [XmlElement("height")]
        public int Height
        {
            get { return _height; }
            set
            {
                if (value <= 0)
                    _height = int.MinValue;
                else
                    _height = value;
            }
        }

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(Url) &&
                   string.IsNullOrEmpty(Title) &&
                   string.IsNullOrEmpty(Link) &&
                   Width <= 0 ||
                   Height <= 0;
        }

        Uri IUriProvider.Uri => _url;
    }
}