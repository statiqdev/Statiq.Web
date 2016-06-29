using System.ComponentModel;
using System.Xml.Serialization;

namespace Wyam.Core.Syndication.Rdf
{
    [XmlType("li", Namespace=RdfFeedBase.NamespaceRss10)]
    public class RdfResource
    {
        private RdfBase _target = null;

        /// <summary>
        /// Ctor
        /// </summary>
        public RdfResource()
        {
            _target = null;
        }

        /// <summary>
        /// Ctor
        /// </summary>
        public RdfResource(RdfBase target)
        {
            _target = target;
        }

        /// <summary>
        /// Gets the RDF association for the target
        /// </summary>
        [DefaultValue(null)]
        [XmlAttribute("resource", Namespace=RdfFeedBase.NamespaceRdf)]
        public string Resource
        {
            get { return (_target != null) ? _target.About : null; }
            set { }
        }

        public static implicit operator RdfResource(RdfBase value)
        {
            return new RdfResource(value);
        }

        public static explicit operator RdfBase(RdfResource value)
        {
            return value._target;
        }
    }
}