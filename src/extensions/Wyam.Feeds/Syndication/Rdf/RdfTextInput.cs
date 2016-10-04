using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Wyam.Feeds.Syndication.Rdf
{
    /// <summary>
    /// RDF 1.0 TextInput
    ///		http://web.resource.org/rss/1.0/spec#s5.6
    /// </summary>
    [Serializable]
    [XmlType("input", Namespace=RdfFeedBase.NamespaceRss10)]
    public class RdfTextInput : RdfItem
    {
        private string _name = null;

        [DefaultValue(null)]
        [XmlElement("name", Namespace=RdfFeedBase.NamespaceRss10)]
        public string Name
        {
            get { return _name; }
            set { _name = string.IsNullOrEmpty(value) ? null : value; }
        }

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(Title) &&
                   string.IsNullOrEmpty(Description) &&
                   string.IsNullOrEmpty(Link) &&
                   string.IsNullOrEmpty(Name);
        }
    }
}