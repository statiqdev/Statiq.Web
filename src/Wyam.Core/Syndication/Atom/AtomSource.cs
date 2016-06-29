using System;
using System.ComponentModel;
using System.Xml.Serialization;
using Wyam.Core.Syndication.Extensions;

namespace Wyam.Core.Syndication.Atom
{
    /// <summary>
    /// http://tools.ietf.org/html/rfc4287#section-4.2.11
    /// </summary>
    /// <remarks>
    /// atomSource : atomBase
    ///		atomGenerator?
    ///		atomIcon?
    ///		atomLogo?
    ///		atomSubtitle?
    /// </remarks>
    public class AtomSource : AtomBase
    {
        private AtomGenerator _generator = null;
        private Uri _icon = null;
        private Uri _logo = null;
        private AtomText _subtitle = null;

        [DefaultValue(null)]
        [XmlElement("generator")]
        public AtomGenerator Generator
        {
            get { return _generator; }
            set { _generator = value; }
        }

        [DefaultValue(null)]
        [XmlElement("icon")]
        public string Icon
        {
            get { return ConvertToString(_icon); }
            set { _icon = ConvertToUri(value); }
        }

        [XmlIgnore]
        protected Uri IconUri
        {
            get { return _icon; }
        }

        [DefaultValue(null)]
        [XmlElement("logo")]
        public string Logo
        {
            get { return ConvertToString(_logo); }
            set { _logo = ConvertToUri(value); }
        }

        [XmlIgnore]
        protected Uri LogoUri
        {
            get { return _logo; }
        }

        [DefaultValue(null)]
        [XmlElement("subtitle")]
        public AtomText SubTitle
        {
            get { return _subtitle; }
            set { _subtitle = value; }
        }

        [XmlIgnore]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual bool SubTitleSpecified
        {
            get { return true; }
            set { }
        }
    }
}