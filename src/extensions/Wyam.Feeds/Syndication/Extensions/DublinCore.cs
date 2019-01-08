using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace Wyam.Feeds.Syndication.Extensions
{
    /// <summary>
    /// Dublin Core Metadata Element Set, Version 1.1
    ///     http://dublincore.org/documents/dces/
    ///     http://web.resource.org/rss/1.0/modules/dc/
    /// </summary>
    public class DublinCore : IExtensionAdapter
    {
        private const string Prefix = "dc";
        private const string Namespace = "http://purl.org/dc/elements/1.1/";

        private static readonly XmlDocument NodeCreator = new XmlDocument();

        private readonly Dictionary<TermName, XmlElement> _dcTerms = new Dictionary<TermName, XmlElement>();

        /// <summary>
        /// Gets and sets the values for DublinCore extensions
        /// </summary>
        public string this[TermName term]
        {
            get
            {
                if (!_dcTerms.ContainsKey(term))
                {
                    return null;
                }
                return _dcTerms[term].InnerText;
            }

            set
            {
                Add(term, value);
            }
        }

        public ICollection<TermName> Terms => _dcTerms.Keys;

        public void Add(TermName term, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                Remove(term);
                return;
            }

            if (term == TermName.Date)
            {
                try
                {
                    // ensure date is in correct format
                    DateTime? date = ExtensibleBase.ConvertToDateTime(value);
                    value =
                        date.HasValue ?
                        ExtensibleBase.ConvertToString(date.Value) :
                        null;
                }
                catch
                {
                }
            }

            XmlElement element = NodeCreator.CreateElement(
                Prefix,
                term.ToString().ToLowerInvariant(), // TODO: use the XmlEnumAttribute to convert the term name
                Namespace);

            element.InnerText = value;

            _dcTerms[term] = element;
        }

        public bool Remove(TermName term) => _dcTerms.Remove(term);

        IEnumerable<XmlAttribute> IExtensionAdapter.GetAttributeEntensions()
        {
            // Dublin Core does not specify attributes
            return null;
        }

        IEnumerable<XmlElement> IExtensionAdapter.GetElementExtensions()
        {
            if (_dcTerms.Count < 1)
            {
                return null;
            }

            List<XmlElement> extensions = new List<XmlElement>(_dcTerms.Count);

            foreach (TermName term in _dcTerms.Keys)
            {
                extensions.Add(_dcTerms[term]);
            }

            return extensions;
        }

        void IExtensionAdapter.SetAttributeEntensions(IEnumerable<XmlAttribute> attributes)
        {
            // Dublin Core does not specify attributes
        }

        void IExtensionAdapter.SetElementExtensions(IEnumerable<XmlElement> elements)
        {
            foreach (XmlElement element in elements)
            {
                if (!Namespace.Equals(element.NamespaceURI, StringComparison.InvariantCulture))
                {
                    continue;
                }

                try
                {
                    TermName term = (TermName)Enum.Parse(typeof(TermName), element.LocalName, true);
                    _dcTerms[term] = element;
                }
                catch
                {
                    continue;
                }
            }
        }

        public enum TermName
        {
            [XmlEnum("contributor")]
            Contributor,

            [XmlEnum("coverage")]
            Coverage,

            [XmlEnum("creator")]
            Creator,

            [XmlEnum("date")]
            Date,

            [XmlEnum("description")]
            Description,

            [XmlEnum("format")]
            Format,

            [XmlEnum("identifier")]
            Identifier,

            [XmlEnum("language")]
            Language,

            [XmlEnum("publisher")]
            Publisher,

            [XmlEnum("relation")]
            Relation,

            [XmlEnum("rights")]
            Rights,

            [XmlEnum("source")]
            Source,

            [XmlEnum("subject")]
            Subject,

            [XmlEnum("title")]
            Title,

            [XmlEnum("type")]
            Type
        }
    }
}