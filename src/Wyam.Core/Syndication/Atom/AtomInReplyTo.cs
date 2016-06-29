using System;
using System.ComponentModel;
using System.Xml.Serialization;
using Wyam.Core.Syndication.Extensions;

namespace Wyam.Core.Syndication.Atom
{
    /// <summary>
    /// http://tools.ietf.org/html/rfc4685#section-3
    /// </summary>
    [Serializable]
    [XmlType(TypeName="in-reply-to", Namespace=ThreadingNamespace)]
    public class AtomInReplyTo : AtomCommonAttributes, IUriProvider
    {
        private Uri _refId = null;
        private Uri _href = null;
        private Uri _source = null;
        private string _type = null;

        [XmlAttribute("ref")]
        public string Ref
        {
            get { return ConvertToString(_refId); }
            set { _refId = ConvertToUri(value); }
        }

        [XmlAttribute("href")]
        [DefaultValue(null)]
        public string Href
        {
            get { return ConvertToString(_href); }
            set { _href = ConvertToUri(value); }
        }

        [XmlAttribute("source")]
        [DefaultValue(null)]
        public string Source
        {
            get { return ConvertToString(_source); }
            set { _source = ConvertToUri(value); }
        }

        [XmlAttribute("type")]
        [DefaultValue(null)]
        public virtual string Type
        {
            get { return _type; }
            set { _type = string.IsNullOrEmpty(value) ? null : value; }
        }

        Uri IUriProvider.Uri
        {
            get { return _refId; }
        }

        public override void AddNamespaces(XmlSerializerNamespaces namespaces)
        {
            namespaces.Add(ThreadingPrefix, ThreadingNamespace);

            base.AddNamespaces(namespaces);
        }
    }
}