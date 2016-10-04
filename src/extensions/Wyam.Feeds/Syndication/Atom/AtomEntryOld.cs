using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Wyam.Feeds.Syndication.Atom
{
    /// <summary>
    /// Adapter for Atom 0.3 compatibility
    /// </summary>
    [Serializable]
    public class AtomEntryOld : AtomEntry
    {
        /// <summary>
        /// Ctor
        /// </summary>
        [Obsolete("Atom 0.3 is for backwards compatibility and should only be used for deserialization", true)]
        public AtomEntryOld()
        {
        }

        [DefaultValue(null)]
        [XmlElement("modified")]
        public AtomDate Modified
        {
            get { return base.Updated; }
            set { base.Updated = value; }
        }

        [XmlIgnore]
        public bool ModifiedSpecified
        {
            get { return UpdatedSpecified; }
            set { }
        }

        [DefaultValue(null)]
        [XmlElement("issued")]
        public AtomDate Issued
        {
            get { return base.Published; }
            set { base.Published = value; }
        }

        [XmlIgnore]
        public bool IssuedSpecified
        {
            get { return PublishedSpecified; }
            set { }
        }

        [XmlIgnore]
        public override AtomDate Published
        {
            get { return base.Published; }
            set { base.Published = value; }
        }

        [XmlIgnore]
        public override AtomDate Updated
        {
            get { return base.Updated; }
            set { base.Updated = value; }
        }
    }
}