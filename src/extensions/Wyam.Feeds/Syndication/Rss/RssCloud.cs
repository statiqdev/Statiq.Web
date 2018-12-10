using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Wyam.Feeds.Syndication.Rss
{
    /// <summary>
    /// RSS 2.0 Cloud
    ///     http://blogs.law.harvard.edu/tech/rss#ltcloudgtSubelementOfLtchannelgt
    ///     http://blogs.law.harvard.edu/tech/soapMeetsRss#rsscloudInterface
    /// </summary>
    [Serializable]
    public class RssCloud : RssBase
    {
        private string _domain = null;
        private int _port = int.MinValue;
        private string _path = null;
        private string _registerProcedure = null;
        private string _protocol = null;

        /// <summary>
        /// Gets and sets the domain.
        /// </summary>
        [DefaultValue(null)]
        [XmlAttribute("domain")]
        public string Domain
        {
            get { return _domain; }
            set { _domain = string.IsNullOrEmpty(value) ? null : value; }
        }

        /// <summary>
        /// Gets and sets the port.
        /// </summary>
        [DefaultValue(int.MinValue)]
        [XmlAttribute("port")]
        public int Port
        {
            get
            {
                return _port;
            }

            set
            {
                if (value <= 0)
                {
                    _port = int.MinValue;
                }
                else
                {
                    _port = value;
                }
            }
        }

        /// <summary>
        /// Gets and sets the path.
        /// </summary>
        [DefaultValue(null)]
        [XmlAttribute("path")]
        public string Path
        {
            get { return _path; }
            set { _path = string.IsNullOrEmpty(value) ? null : value; }
        }

        /// <summary>
        /// Gets and sets the registerProcedure.
        /// </summary>
        [DefaultValue(null)]
        [XmlAttribute("registerProcedure")]
        public string RegisterProcedure
        {
            get { return _registerProcedure; }
            set { _registerProcedure = string.IsNullOrEmpty(value) ? null : value; }
        }

        /// <summary>
        /// Gets and sets the protocol.
        /// </summary>
        [DefaultValue(null)]
        [XmlAttribute("protocol")]
        public string Protocol
        {
            get { return _protocol; }
            set { _protocol = string.IsNullOrEmpty(value) ? null : value; }
        }

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(Domain)
                   && Port <= 0
                   && string.IsNullOrEmpty(Path)
                   && string.IsNullOrEmpty(RegisterProcedure)
                   && string.IsNullOrEmpty(Protocol);
        }
    }
}