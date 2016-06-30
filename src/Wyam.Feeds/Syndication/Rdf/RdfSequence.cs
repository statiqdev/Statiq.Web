using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Wyam.Feeds.Syndication.Rdf
{
    public class RdfSequence
    {
        private RdfFeed _target = null;

        /// <summary>
        /// Ctor
        /// </summary>
        public RdfSequence()
        {
            _target = null;
        }

        /// <summary>
        /// Ctor
        /// </summary>
        public RdfSequence(RdfFeed target)
        {
            _target = target;
        }

        [DefaultValue(null)]
        [XmlArray("Seq", Namespace=RdfFeedBase.NamespaceRdf)]
        public List<RdfResource> Items
        {
            get
            {
                if (_target == null ||
                    _target.Items == null ||
                    _target.Items.Count == 0)
                {
                    return null;
                }

                List<RdfResource> items = new List<RdfResource>(_target.Items.Count);
                foreach (RdfBase item in _target.Items)
                {
                    items.Add(new RdfResource(item));
                }
                return items;
            }
            set { }
        }

        [XmlIgnore]
        public bool ItemsSpecified
        {
            get
            {
                List<RdfResource> items = Items;

                return (items != null) && (items.Count > 0);
            }
            set { }
        }
    }
}