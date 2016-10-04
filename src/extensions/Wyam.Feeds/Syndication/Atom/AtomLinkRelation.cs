using System.Xml.Serialization;

namespace Wyam.Feeds.Syndication.Atom
{
    /// <summary>
    /// http://www.iana.org/assignments/link-relations.html
    /// </summary>
    public enum AtomLinkRelation
    {
        [XmlEnum(null)]
        None,

        [XmlEnum("alternate")]
        Alternate,

        [XmlEnum("current")]
        Current,

        [XmlEnum("enclosure")]
        Enclosure,

        [XmlEnum("edit")]
        Edit,

        [XmlEnum("edit-media")]
        EditMedia,

        [XmlEnum("first")]
        First,

        [XmlEnum("last")]
        Last,

        [XmlEnum("license")]
        License,

        [XmlEnum("next")]
        Next,

        [XmlEnum("next-archive")]
        NextArchive,

        [XmlEnum("payment")]
        Payment,

        [XmlEnum("previous")]
        Previous,

        [XmlEnum("prev-archive")]
        PrevArchive,

        [XmlEnum("related")]
        Related,

        [XmlEnum("replies")]
        Replies,

        [XmlEnum("self")]
        Self,

        [XmlEnum("via")]
        Via
    }
}