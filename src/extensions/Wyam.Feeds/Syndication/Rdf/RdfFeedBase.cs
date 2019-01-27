using System.ComponentModel;
using System.Xml.Serialization;
using Wyam.Feeds.Syndication.Extensions;

namespace Wyam.Feeds.Syndication.Rdf
{
    /// <summary>
    /// RDF 1.0 Root
    ///     http://web.resource.org/rss/1.0/spec#s5.2
    /// </summary>
    public abstract class RdfFeedBase : ExtensibleBase
    {
        public const string SpecificationUrl = "http://web.resource.org/rss/1.0/spec";
        protected internal const string RootElement = "RDF";
        protected internal const string NamespaceRdf = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
        protected internal const string NamespaceRss10 = "http://purl.org/rss/1.0/";
        protected internal const string NamespaceDefault = "http://purl.org/rss/1.0/";
        public const string MimeType = "application/rss+xml";

        private RdfChannel _channel = null;
        private RdfImage _image = null;
        private RdfTextInput _textInput = null;

        [DefaultValue(null)]
        [XmlElement("channel", Namespace=NamespaceRss10)]
        public RdfChannel Channel
        {
            get
            {
                if (_channel == null)
                {
                    _channel = new RdfChannel();
                    _channel.SetParent((RdfFeed)this);
                }

                return _channel;
            }
            set
            {
                _channel = value;
                _channel.SetParent((RdfFeed)this);
            }
        }

        /// <summary>
        /// Gets and sets an RDF association between the optional image element and this particular RSS channel.
        /// </summary>
        [DefaultValue(null)]
        [XmlElement("image", Namespace=NamespaceRss10)]
        public RdfImage Image
        {
            get
            {
                return _image ?? (_image = new RdfImage());
            }

            set
            {
                _image = value;
            }
        }

        [XmlIgnore]
        public bool ImageSpecified
        {
            get { return _image?.IsEmpty() == false; }
            set { }
        }

        /// <summary>
        /// Gets and sets an RDF association between the optional textinput element and this particular RSS channel.
        /// </summary>
        [DefaultValue(null)]
        [XmlElement("textinput", Namespace=NamespaceRss10)]
        public RdfTextInput TextInput
        {
            get
            {
                return _textInput ?? (_textInput = new RdfTextInput());
            }

            set
            {
                _textInput = value;
            }
        }

        [XmlIgnore]
        public bool TextInputSpecified
        {
            get { return _textInput?.IsEmpty() == false; }
            set { }
        }
    }
}