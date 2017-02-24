using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Xml.Serialization;

namespace Wyam.Feeds.Syndication.Rss
{
    /// <summary>
    /// RSS 2.0 Channel
    ///     http://blogs.law.harvard.edu/tech/rss
    /// </summary>
    [Serializable]
    public abstract class RssChannelBase : RssBase, IUriProvider
    {
        // required
        private string _title = string.Empty;
        private Uri _link = null;
        private string _description = string.Empty;

        // optional
        private CultureInfo _language = CultureInfo.InvariantCulture;
        private string _copyright = null;
        private RssPerson _managingEditor = null;
        private RssPerson _webMaster = null;
        private RssDate _pubDate;
        private RssDate _lastBuildDate;
        private string _generator = null;
        private string _docs = null;
        private RssCloud _cloud = null;
        private int _ttl = int.MinValue;
        private RssImage _image = null;
        private string _rating = null;
        private RssTextInput _textInput = null;
        private RssSkipHours _skipHours = null;
        private RssSkipDays _skipDays = null;

        /// <remarks>
        /// Required even if empty.
        /// </remarks>
        [XmlElement("title")]
        public string Title
        {
            get { return _title; }
            set { _title = string.IsNullOrEmpty(value) ? string.Empty : value; }
        }

        /// <remarks>
        /// Required even if empty.
        /// </remarks>
        [XmlElement("link")]
        public string Link
        {
            get
            {
                string value = ConvertToString(_link);
                return string.IsNullOrEmpty(value) ? string.Empty : value;
            }
            set { _link = ConvertToUri(value); }
        }

        /// <remarks>
        /// Required even if empty.
        /// </remarks>
        [XmlElement("description")]
        public string Description
        {
            get { return _description; }
            set { _description = string.IsNullOrEmpty(value) ? string.Empty : value; }
        }

        [DefaultValue("")]
        [XmlElement("language")]
        public string Language
        {
            get { return _language.Name; }
            set
            {
                if (value == null)
                {
                    value = string.Empty;
                }
                _language = CultureInfo.GetCultureInfo(value.Trim());
            }
        }

        [DefaultValue(null)]
        [XmlElement("copyright")]
        public string Copyright
        {
            get { return _copyright; }
            set { _copyright = string.IsNullOrEmpty(value) ? null : value; }
        }

        /// <summary>
        /// Gets and sets the managing editor of the channel.
        /// </summary>
        [XmlElement("managingEditor")]
        public RssPerson ManagingEditor
        {
            get
            {
                if (_managingEditor == null)
                {
                    _managingEditor = new RssPerson();
                }

                return _managingEditor;
            }
            set { _managingEditor = value; }
        }

        [XmlIgnore]
        public bool ManagingEditorSpecified
        {
            get { return (_managingEditor != null && !_managingEditor.IsEmpty()); }
            set { }
        }

        /// <summary>
        /// Gets and sets the webMaster of the channel.
        /// </summary>
        [XmlElement("webMaster")]
        public RssPerson WebMaster
        {
            get
            {
                if (_webMaster == null)
                {
                    _webMaster = new RssPerson();
                }

                return _webMaster;
            }
            set { _webMaster = value; }
        }

        [XmlIgnore]
        public bool WebMasterSpecified
        {
            get { return (_webMaster != null && !_webMaster.IsEmpty()); }
            set { }
        }

        [DefaultValue(null)]
        [XmlElement("pubDate")]
        public RssDate PubDate
        {
            get { return _pubDate; }
            set { _pubDate = value; }
        }

        [XmlIgnore]
        public bool PubDateSpecified
        {
            get { return _pubDate.HasValue; }
            set { }
        }

        [DefaultValue(null)]
        [XmlElement("lastBuildDate")]
        public RssDate LastBuildDate
        {
            get { return _lastBuildDate; }
            set { _lastBuildDate = value; }
        }

        [XmlIgnore]
        public bool LastBuildDateSpecified
        {
            get { return _lastBuildDate.HasValue; }
            set { }
        }

        [XmlElement("category")]
        public readonly List<RssCategory> Categories = new List<RssCategory>();

        [XmlIgnore]
        public bool CategoriesSpecified
        {
            get { return (Categories.Count > 0); }
            set { }
        }

        [DefaultValue(null)]
        [XmlElement("generator")]
        public string Generator
        {
            get { return _generator; }
            set { _generator = string.IsNullOrEmpty(value) ? null : value; }
        }

        [DefaultValue(null)]
        [XmlElement("docs")]
        public string Docs
        {
            get { return _docs; }
            set { _docs = string.IsNullOrEmpty(value) ? null : value; }
        }

        [DefaultValue(null)]
        [XmlElement("cloud")]
        public RssCloud Cloud
        {
            get
            {
                if (_cloud == null)
                {
                    _cloud = new RssCloud();
                }

                return _cloud;
            }
            set { _cloud = value; }
        }

        [XmlIgnore]
        public bool CloudSpecified
        {
            get { return (_cloud != null && !_cloud.IsEmpty()); }
            set { }
        }

        [DefaultValue(int.MinValue)]
        [XmlElement("ttl")]
        public int Ttl
        {
            get { return _ttl; }
            set
            {
                if (value < 0)
                    _ttl = int.MinValue;
                else
                    _ttl = value;
            }
        }

        [DefaultValue(null)]
        [XmlElement("image")]
        public RssImage Image
        {
            get
            {
                if (_image == null)
                {
                    _image = new RssImage();
                }

                return _image;
            }
            set { _image = value; }
        }

        [XmlIgnore]
        public bool ImageSpecified
        {
            get { return (_image != null && !_image.IsEmpty()); }
            set { }
        }

        [DefaultValue(null)]
        [XmlElement("rating")]
        public string Rating
        {
            get { return _rating; }
            set { _rating = string.IsNullOrEmpty(value) ? null : value; }
        }

        [XmlElement("textInput")]
        public RssTextInput TextInput
        {
            get
            {
                if (_textInput == null)
                {
                    _textInput = new RssTextInput();
                }

                return _textInput;
            }
            set { _textInput = value; }
        }

        [XmlIgnore]
        public bool TextInputSpecified
        {
            get { return (_textInput != null && !_textInput.IsEmpty()); }
            set { }
        }

        [DefaultValue(null)]
        [XmlElement("skipHours")]
        public RssSkipHours SkipHours
        {
            get
            {
                if (_skipHours == null)
                {
                    _skipHours = new RssSkipHours();
                }

                return _skipHours;
            }
            set { _skipHours = value; }
        }

        [XmlIgnore]
        public bool SkipHoursSpecified
        {
            get { return (_skipHours != null && !_skipHours.IsEmpty()); }
            set { }
        }

        [DefaultValue(null)]
        [XmlElement("skipDays")]
        public RssSkipDays SkipDays
        {
            get
            {
                if (_skipDays == null)
                {
                    _skipDays = new RssSkipDays();
                }

                return _skipDays;
            }
            set { _skipDays = value; }
        }

        [XmlIgnore]
        public bool SkipDaysSpecified
        {
            get { return (_skipDays != null && !_skipDays.IsEmpty()); }
            set { }
        }

        Uri IUriProvider.Uri => _link;
    }
}