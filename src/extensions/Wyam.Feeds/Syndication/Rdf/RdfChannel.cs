using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Wyam.Feeds.Syndication.Rdf
{
    /// <summary>
    /// RDF 1.0 Channel
    ///     http://web.resource.org/rss/1.0/spec#s5.3
    /// </summary>
    [Serializable]
    [XmlType("channel", Namespace=RdfFeedBase.NamespaceRss10)]
    public class RdfChannel : RdfItem
    {
        private RdfFeed _parent = null;

        /// <summary>
        /// Gets and sets an RDF association between the optional image element
        /// and this particular RSS channel.
        /// </summary>
        [DefaultValue(null)]
        [XmlElement("image", Namespace=RdfFeedBase.NamespaceRss10)]
        public RdfResource Image
        {
            get
            {
                if (_parent == null ||
                    !_parent.ImageSpecified)
                {
                    return null;
                }

                return new RdfResource(_parent.Image);
            }
            set {  }
        }

        /// <summary>
        /// Gets and sets an RDF table of contents, associating the document's items
        /// with this particular RSS channel.
        /// </summary>
        /// <remarks>
        /// Required even if empty.
        /// </remarks>
        [XmlElement("items", Namespace=RdfFeedBase.NamespaceRss10)]
        public RdfSequence Items
        {
            get { return new RdfSequence(_parent); }
            set {  }
        }

        /// <summary>
        /// Gets and sets an RDF association between the optional image element and this particular RSS channel.
        /// </summary>
        [DefaultValue(null)]
        [XmlElement("textinput", Namespace=RdfFeedBase.NamespaceRss10)]
        public RdfResource TextInput
        {
            get
            {
                if (_parent == null ||
                    !_parent.TextInputSpecified)
                {
                    return null;
                }

                return new RdfResource(_parent.TextInput);
            }
            set { }
        }

        internal void SetParent(RdfFeed feed)
        {
            _parent = feed;
        }
    }
}