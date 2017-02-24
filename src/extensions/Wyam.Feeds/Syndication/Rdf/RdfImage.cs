using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Wyam.Feeds.Syndication.Rdf
{
    /// <summary>
    /// RDF 1.0 Image
    ///     http://web.resource.org/rss/1.0/spec#s5.4
    /// </summary>
    [Serializable]
    [XmlType("image", Namespace=RdfFeedBase.NamespaceRss10)]
    public class RdfImage : RdfBase, IUriProvider
    {
        private Uri _url = null;

        [DefaultValue(null)]
        [XmlElement("url", Namespace=RdfFeedBase.NamespaceRss10)]
        public string Url
        {
            get { return ConvertToString(_url); }
            set { _url = ConvertToUri(value); }
        }

        /// <summary>
        /// Gets and sets
        /// </summary>
        [DefaultValue(null)]
        [XmlAttribute("about", Namespace=RdfFeedBase.NamespaceRdf)]
        public override string About
        {
            get { return Url; }
            set { Url = value; }
        }

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(Url) &&
                string.IsNullOrEmpty(Title);
        }

        Uri IUriProvider.Uri => _url;
    }
}
