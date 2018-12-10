using System;
using System.Xml.Serialization;

namespace Wyam.Feeds.Syndication.Rss
{
    /// <summary>
    /// RSS 2.0 Email
    ///     http://blogs.law.harvard.edu/tech/rss#ltauthorgtSubelementOfLtitemgt
    /// </summary>
    [Serializable]
    public class RssPerson : RssBase
    {
        private const string EmailFormat = "{0} ({1})";

        private string _name = null;
        private string _email = null;

        [XmlIgnore]
        public string Email
        {
            get { return _email; }
            set { _email = value; }
        }

        [XmlIgnore]
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        [XmlText]
        public string Value
        {
            get
            {
                if (string.IsNullOrEmpty(_name))
                {
                    return _email;
                }

                if (string.IsNullOrEmpty(_email))
                {
                    return _name;
                }

                return string.Format(EmailFormat, _email.Trim(), _name.Trim());
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _name = _email = null;
                    return;
                }

                int start = value.IndexOf("(");
                int end = value.LastIndexOf(")");
                if (end <= start || start < 0 || end < 0)
                {
                    _name = value;
                    _email = null;
                    return;
                }

                _name = value.Substring(start + 1, end - start - 1);
                _email = value.Substring(0, start);
            }
        }

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(Email)
                   && string.IsNullOrEmpty(Name);
        }
    }
}