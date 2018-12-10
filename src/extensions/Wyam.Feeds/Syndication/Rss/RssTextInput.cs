using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Wyam.Feeds.Syndication.Rss
{
    /// <summary>
    /// RSS 2.0 TextInput
    ///     http://blogs.law.harvard.edu/tech/rss#lttextinputgtSubelementOfLtchannelgt
    /// </summary>
    [Serializable]
    public class RssTextInput : RssBase
    {
        private string _title = null;
        private string _description = null;
        private string _name = null;
        private Uri _link = null;

        /// <summary>
        /// Gets and sets the title of the submit button.
        /// </summary>
        [DefaultValue(null)]
        [XmlElement("title")]
        public string Title
        {
            get { return _title; }
            set { _title = string.IsNullOrEmpty(value) ? null : value; }
        }

        /// <summary>
        /// Gets and sets the description of the text input area.
        /// </summary>
        [DefaultValue(null)]
        [XmlElement("description")]
        public string Description
        {
            get { return _description; }
            set { _description = string.IsNullOrEmpty(value) ? null : value; }
        }

        /// <summary>
        /// Gets and sets the name of the text input field.
        /// </summary>
        [DefaultValue(null)]
        [XmlElement("name")]
        public string Name
        {
            get { return _name; }
            set { _name = string.IsNullOrEmpty(value) ? null : value; }
        }

        /// <summary>
        /// Gets and sets the text input request url.
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

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(Title)
                   && string.IsNullOrEmpty(Description)
                   && string.IsNullOrEmpty(Name)
                   && string.IsNullOrEmpty(Link);
        }
    }
}