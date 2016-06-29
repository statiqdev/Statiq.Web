using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Wyam.Core.Syndication.Rdf
{
    /// <summary>
	/// RDF 1.0 Root
	///		http://web.resource.org/rss/1.0/spec#s5.2
	/// </summary>
	/// <remarks>
	/// XmlSerializer serializes public fields before public properties
	/// and serializes base class members before derriving class members.
	/// Since RssChannel uses a readonly field for Items it must be placed
	/// in a derriving class in order to make sure items serialize last.
	/// </remarks>
	[XmlRoot(RootElement, Namespace=NamespaceRdf)]
	public class RdfFeed : RdfFeedBase, IFeed
	{
	    [XmlElement("item", Namespace=NamespaceRss10)]
		public readonly List<RdfItem> Items = new List<RdfItem>();

	    [XmlIgnore]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ItemsSpecified
		{
			get { return (Items.Count > 0); }
			set { }
		}

		[XmlIgnore]
		public RdfItem this[int index]
		{
			get { return Items[index]; }
			set { Items[index] = value; }
		}

	    string IFeed.MimeType => MimeType;

        string IFeed.Copyright
        {
            get { return Channel.Copyright; }
            set { Channel.Copyright = value; }
        }

        IList<IFeedItem> IFeed.Items => Items.ToArray();

        Uri IFeedMetadata.ID => ((IFeedMetadata)Channel).ID;

        string IFeedMetadata.Title => ((IFeedMetadata)Channel).Title;

        string IFeedMetadata.Description => ((IFeedMetadata)Channel).Description;

        string IFeedMetadata.Author => ((IFeedMetadata)Channel).Author;

        DateTime? IFeedMetadata.Published => ((IFeedMetadata)Channel).Published;

        DateTime? IFeedMetadata.Updated => ((IFeedMetadata)Channel).Updated;

        Uri IFeedMetadata.Link => ((IFeedMetadata)Channel).Link;

        Uri IFeedMetadata.ImageLink => ((IUriProvider)Image).Uri;

        public override void AddNamespaces(XmlSerializerNamespaces namespaces)
		{
			namespaces.Add("", NamespaceRss10);
			namespaces.Add("rdf", NamespaceRdf);

			Channel.AddNamespaces(namespaces);
			if (ImageSpecified)
			{
				Image.AddNamespaces(namespaces);
			}
			if (TextInputSpecified)
			{
				TextInput.AddNamespaces(namespaces);
			}

			foreach (RdfItem item in Items)
			{
				item.AddNamespaces(namespaces);
			}

			base.AddNamespaces(namespaces);
		}
	}
}
