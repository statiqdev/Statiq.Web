using System.Collections.Generic;
using System.Xml;

namespace Wyam.Core.Syndication.Extensions
{
    /// <summary>
    /// Interface that adapters implement to apply / extract additional elements and attributes
    /// </summary>
    public interface IExtensionAdapter
    {
        IEnumerable<XmlAttribute> GetAttributeEntensions();
        IEnumerable<XmlElement> GetElementExtensions();

        void SetAttributeEntensions(IEnumerable<XmlAttribute> attributes);
        void SetElementExtensions(IEnumerable<XmlElement> elements);
    }
}