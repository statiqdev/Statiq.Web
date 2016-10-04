using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Wyam.Feeds.Syndication.Rss
{
    /// <summary>
	/// RSS 2.0 Channel
	///		http://blogs.law.harvard.edu/tech/rss
	/// </summary>
	/// <remarks>
	/// XmlSerializer serializes public fields before public properties
	/// and serializes base class members before derriving class members.
	/// Since RssChannel uses a readonly field for Items it must be placed
	/// in a derriving class in order to make sure items serialize last.
	/// </remarks>
	public class RssChannel : RssChannelBase
	{
	    [XmlElement("item")]
		public readonly List<RssItem> Items = new List<RssItem>();

		[XmlIgnore]
		public bool ItemsSpecified
		{
			get { return (Items.Count > 0); }
			set { }
		}

	    public override void AddNamespaces(XmlSerializerNamespaces namespaces)
		{
			foreach (RssItem item in Items)
			{
				item.AddNamespaces(namespaces);
			}

			base.AddNamespaces(namespaces);
		}
	}
}