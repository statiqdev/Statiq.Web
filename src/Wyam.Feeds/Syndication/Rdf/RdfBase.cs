using System;
using System.ComponentModel;
using System.Xml.Serialization;
using Wyam.Feeds.Syndication.Extensions;

namespace Wyam.Feeds.Syndication.Rdf
{
	/// <summary>
	/// RDF 1.0 Base
	///		http://web.resource.org/rss/1.0/spec#s5.3
	/// </summary>
	[Serializable]
    public abstract class RdfBase : ExtensibleBase, IUriProvider
	{
	    private string _title = string.Empty;
		private Uri _link = null;
		private string _about = null;

	    /// <summary>
		/// Gets and sets a descriptive title for the channel.
		/// </summary>
		/// <remarks>
		/// Suggested maximum length is 40 characters.
		/// Required even if empty.
		/// </remarks>
		[XmlElement("title")]
		public string Title
		{
			get { return _title; }
			set { _title = string.IsNullOrEmpty(value) ? string.Empty : value; }
		}

		/// <summary>
		/// Gets and sets the URL to which an HTML rendering of the channel title will link,
		/// commonly the parent site's home or news page.
		/// </summary>
		/// <remarks>
		/// Suggested maximum length is 500 characters.
		/// </remarks>
		[DefaultValue(null)]
		[XmlElement("link")]
		public string Link
		{
			get
			{
				string value = ConvertToString(_link);
				return string.IsNullOrEmpty(value) ? string.Empty : value;
			}
			set { _link = ConvertToUri(value); }
		}

		/// <summary>
		/// Gets and sets a URL link to the described resource
		/// </summary>
		[XmlAttribute("about", Namespace=RdfFeedBase.NamespaceRdf)]
		public virtual string About
		{
			get
			{
				if (string.IsNullOrEmpty(_about))
				{
					return Link;
				}
				return _about;
			}
			set { _about = string.IsNullOrEmpty(value) ? string.Empty : value; }
		}

	    Uri IUriProvider.Uri => _link;
	}
}