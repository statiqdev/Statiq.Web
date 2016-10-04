using System.Xml.Serialization;

namespace Wyam.Feeds.Syndication.Atom
{
    public enum AtomTextType
    {
        [XmlEnum("text")]
        Text,

        [XmlEnum("html")]
        Html,

        [XmlEnum("xhtml")]
        Xhtml
    }
}