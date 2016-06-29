using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Wyam.Core.Syndication.Atom
{
	/// <summary>
	/// The Atom Syndication Format
	///		http://tools.ietf.org/html/rfc4287#section-4.1.1
	/// </summary>
	/// <remarks>
	/// atomFeed : atomSource
	///		atomLogo?
	///		atomEntry*
	/// </remarks>
	[Serializable]
	[XmlRoot(RootElement, Namespace=Namespace)]
	public class AtomFeed : AtomSource, IFeed
	{
	    public const string SpecificationUrl = "http://tools.ietf.org/html/rfc4287";
		protected internal const string Prefix = "";
		protected internal const string Namespace = "http://www.w3.org/2005/Atom";
		protected internal const string RootElement = "feed";
		public const string MimeType = "application/atom+xml";

	    [XmlElement("entry")]
		public readonly List<AtomEntry> Entries = new List<AtomEntry>();

		[XmlIgnore]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool EntriesSpecified
		{
			get { return (Entries.Count > 0); }
			set { }
		}

	    string IFeed.MimeType
		{
			get { return MimeType; }
		}

		string IFeed.Copyright
		{
			get
			{
				if (!RightsSpecified)
				{
					return null;
				}
				return Rights.StringValue;
			}
		}

		IList<IFeedItem> IFeed.Items
		{
			get { return Entries.ToArray(); }
		}

	    Uri IFeedMetadata.ID
		{
			get { return ((IUriProvider)this).Uri; }
		}

		string IFeedMetadata.Title
		{
			get
			{
				if (Title == null)
				{
					return null;
				}
				return Title.StringValue;
			}
		}

		string IFeedMetadata.Description
		{
			get
			{
				if (SubTitle == null)
				{
					return null;
				}
				return SubTitle.StringValue;
			}
		}

		string IFeedMetadata.Author
		{
			get
			{
				if (!AuthorsSpecified)
				{
					if (!ContributorsSpecified)
					{
						return null;
					}
					foreach (AtomPerson person in Contributors)
					{
						if (!string.IsNullOrEmpty(person.Name))
						{
							return person.Name;
						}
						if (!string.IsNullOrEmpty(person.Email))
						{
							return person.Name;
						}
					}
				}

				foreach (AtomPerson person in Authors)
				{
					if (!string.IsNullOrEmpty(person.Name))
					{
						return person.Name;
					}
					if (!string.IsNullOrEmpty(person.Email))
					{
						return person.Name;
					}
				}

				return null;
			}
		}

		DateTime? IFeedMetadata.Published
		{
			get { return ((IFeedMetadata)this).Updated; }
		}

		DateTime? IFeedMetadata.Updated
		{
			get
			{
				if (!Updated.HasValue)
				{
					return null;
				}

				return Updated.Value;
			}
		}

		Uri IFeedMetadata.Link
		{
			get
			{
				if (!LinksSpecified)
				{
					return null;
				}

				Uri alternate = null;
				foreach (AtomLink link in Links)
				{
					if ("alternate".Equals(link.Rel))
					{
						return ((IUriProvider)link).Uri;
					}
					else if (alternate == null && !"self".Equals(link.Rel))
					{
						return ((IUriProvider)link).Uri;
					}
				}

				return alternate;
			}
		}

		Uri IFeedMetadata.ImageLink
		{
			get
			{
				if (LogoUri == null)
				{
					return IconUri;
				}
				return LogoUri;
			}
		}

	    public override void AddNamespaces(XmlSerializerNamespaces namespaces)
		{
			namespaces.Add(Prefix, Namespace);
			namespaces.Add(XmlPrefix, XmlNamespace);

			foreach (AtomEntry entry in Entries)
			{
				entry.AddNamespaces(namespaces);
			}

			base.AddNamespaces(namespaces);
		}
	}
}
