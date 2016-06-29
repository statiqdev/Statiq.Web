using System;
using System.ComponentModel;
using System.Xml.Serialization;
using Wyam.Core.Syndication.Extensions;

namespace Wyam.Core.Syndication.Atom
{
    /// <summary>
    /// http://tools.ietf.org/html/rfc4287#section-3.2
    /// </summary>
    [Serializable]
    public class AtomPerson : AtomCommonAttributes
    {
        private string _name = null;
        private Uri _uri = null;
        private string _email = null;

        [XmlElement("name")]
        [DefaultValue(null)]
        public string Name
        {
            get { return _name; }
            set { _name = string.IsNullOrEmpty(value) ? null : value; }
        }

        [XmlElement("uri")]
        [DefaultValue(null)]
        public string Uri
        {
            get { return ConvertToString(_uri); }
            set { _uri = ConvertToUri(value); }
        }

        [XmlElement("email")]
        [DefaultValue(null)]
        public string Email
        {
            get { return _email; }
            set { _email = string.IsNullOrEmpty(value) ? null : value; }
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Email))
            {
                return $"\"{Name}\" <{Email}>";
            }
            if (!string.IsNullOrEmpty(Uri))
            {
                return $"\"{Name}\" <{Uri}>";
            }
            return $"\"{Name}\"";
        }
    }
}