using System;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;

namespace Wyam.Core.Syndication.Atom
{
    /// <summary>
    /// http://tools.ietf.org/html/rfc4287#section-3.1
    /// </summary>
    [Serializable]
    public class AtomText : AtomCommonAttributes
    {
        private AtomTextType _type = AtomTextType.Text;
        private string _mediaType = null;
        private string _value = null;

        /// <summary>
        /// Ctor
        /// </summary>
        public AtomText()
        {
        }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="text"></param>
        public AtomText(string text)
        {
            _value = text;
        }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="xhtml"></param>
        public AtomText(XmlNode xhtml)
        {
            XhtmlValue = xhtml;
        }

        [DefaultValue(AtomTextType.Text)]
        [XmlIgnore]
        public AtomTextType Type
        {
            get { return _type; }
            set
            {
                _type = value;
                _mediaType = null;
            }
        }

        [DefaultValue(null)]
        [XmlAttribute("type")]
        public string MediaType
        {
            get
            {
                if (_type == AtomTextType.Text)
                {
                    return _mediaType;
                }

                return _type.ToString().ToLowerInvariant();
            }
            set
            {
                try
                {
                    // Enum.IsDefined doesn't allow case-insensitivity
                    _type = (AtomTextType)Enum.Parse(typeof(AtomTextType), value, true);
                    _mediaType = null;
                }
                catch
                {
                    _type = AtomTextType.Text;
                    _mediaType = value;
                }
            }
        }

        [XmlText]
        [DefaultValue(null)]
        public virtual string Value
        {
            get
            {
                if (_type == AtomTextType.Xhtml)
                {
                    return null;
                }
                return _value;
            }
            set { _value = value; }
        }

        /// <summary>
        /// Gets and sets the Value using XmlNodes.
        /// For serialization purposes only, use the Value property instead.
        /// </summary>
        [DefaultValue(null)]
        [XmlAnyElement(Namespace="http://www.w3.org/1999/xhtml")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public XmlNode XhtmlValue
        {
            get
            {
                if (_type != AtomTextType.Xhtml)
                {
                    return null;
                }
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(_value);
                return doc;
            }
            set
            {
                _type = AtomTextType.Xhtml;
                _value = value.OuterXml;
            }
        }

        [XmlIgnore]
        public string StringValue
        {
            get { return _value; }
        }

        public override string ToString()
        {
            return Value;
        }

        public static implicit operator AtomText(string value)
        {
            return new AtomText(value);
        }

        public static implicit operator AtomText(XmlNode value)
        {
            return new AtomText(value);
        }

        public static explicit operator string(AtomText value)
        {
            return value.Value;
        }

        public static explicit operator XmlNode(AtomText value)
        {
            return value.XhtmlValue;
        }
    }
}