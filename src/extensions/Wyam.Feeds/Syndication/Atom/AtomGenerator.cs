using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Wyam.Feeds.Syndication.Atom
{
    /// <summary>
    /// http://tools.ietf.org/html/rfc4287#section-4.2.4
    /// </summary>
    [Serializable]
    public class AtomGenerator : AtomCommonAttributes
    {
        private Uri _uri = null;
        private string _version = null;
        private string _value = null;

        /// <summary>
        /// Ctor
        /// </summary>
        public AtomGenerator()
        {
        }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="text"></param>
        public AtomGenerator(string text)
        {
            _value = text;
        }

        [XmlAttribute("uri")]
        [DefaultValue(null)]
        public string Uri
        {
            get { return ConvertToString(_uri); }
            set { _uri = ConvertToUri(value); }
        }

        [XmlAttribute("version")]
        [DefaultValue(null)]
        public string Version
        {
            get { return _version; }
            set { _version = string.IsNullOrEmpty(value) ? null : value; }
        }

        [XmlText]
        [DefaultValue(null)]
        public string Value
        {
            get { return _value; }
            set { _value = string.IsNullOrEmpty(value) ? null : value; }
        }

        public override string ToString()
        {
            return Value;
        }

        public static implicit operator AtomGenerator(string value)
        {
            return new AtomGenerator(value);
        }

        public static explicit operator string(AtomGenerator value)
        {
            return value.Value;
        }
    }
}