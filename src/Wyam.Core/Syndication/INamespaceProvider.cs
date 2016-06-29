using System.Xml.Serialization;

namespace Wyam.Core.Syndication
{
    public interface INamespaceProvider
    {
        /// <summary>
        /// Adds additional namespace URIs for the feed
        /// </summary>
        void AddNamespaces(XmlSerializerNamespaces namespaces);
    }
}