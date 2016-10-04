using System;
using System.ComponentModel;
using System.Globalization;
using System.Xml.Serialization;
using Wyam.Feeds.Syndication.Extensions;

namespace Wyam.Feeds.Syndication.Atom
{
    /// <summary>
    /// Common shared Atom attributes
    ///		http://tools.ietf.org/html/rfc4287#section-2
    /// </summary>
    /// <remarks>
    /// atomCommonAttributes
    ///		attribute xml:base?
    ///		attribute xml:lang?
    /// </remarks>
    public abstract class AtomCommonAttributes : ExtensibleBase
    {
        public const string XmlPrefix = "xml";
        public const string XmlNamespace = "http://www.w3.org/XML/1998/namespace";
        public const string ThreadingPrefix = "thr";
        public const string ThreadingNamespace = "http://purl.org/syndication/thread/1.0";

        private Uri _xmlBase = null;
        private CultureInfo _xmlLanguage = CultureInfo.InvariantCulture;

        [DefaultValue("")]
        [XmlAttribute("lang", Namespace=XmlNamespace)]
        public string XmlLanguage
        {
            get { return _xmlLanguage.Name; }
            set { _xmlLanguage = CultureInfo.GetCultureInfo(value); }
        }

        [DefaultValue(null)]
        [XmlAttribute("base", Namespace=XmlNamespace)]
        public string XmlBase
        {
            get { return ConvertToString(_xmlBase); }
            set { _xmlBase = ConvertToUri(value); }
        }
    }
}