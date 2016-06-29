using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using Wyam.Core.Syndication.Extensions;

namespace Wyam.Core.Syndication.Atom
{
    /// <summary>
    /// Commonly shared Atom base
    /// </summary>
    /// <remarks>
    /// atomBase
    ///		atomAuthor*
    ///		atomCategory*
    ///		atomContributor*
    ///		atomId
    ///		atomLink*
    ///		atomRights?
    ///		atomTitle
    ///		atomUpdated
    /// </remarks>
    public abstract class AtomBase : AtomCommonAttributes, IUriProvider
	{
	    private Uri _id = null;
		private AtomText _rights = null;
		private AtomText _title = new AtomText();
		private AtomDate _updated = new AtomDate();

	    /// <remarks>
		/// Required even if empty.
		/// </remarks>
		[XmlElement("id")]
		public string Id
		{
			get
			{
				string value = ConvertToString(_id);
				return string.IsNullOrEmpty(value) ? string.Empty : value;
			}
			set { _id = ConvertToUri(value); }
		}

		/// <remarks>
		/// Required even if empty.
		/// </remarks>
		[XmlElement("title")]
		public AtomText Title
		{
			get { return _title; }
			set { _title = value ?? new AtomText(); }
		}

		[XmlElement("author")]
		public readonly List<AtomPerson> Authors = new List<AtomPerson>();

		[XmlIgnore]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool AuthorsSpecified
		{
			get { return (Authors.Count > 0); }
			set { }
		}

		[XmlElement("category")]
		public readonly List<AtomCategory> Categories = new List<AtomCategory>();

		[XmlIgnore]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool CategoriesSpecified
		{
			get { return (Categories.Count > 0); }
			set { }
		}

		[XmlElement("contributor")]
		public readonly List<AtomPerson> Contributors = new List<AtomPerson>();

		[XmlIgnore]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ContributorsSpecified
		{
			get { return (Contributors.Count > 0); }
			set { }
		}

		[XmlElement("link")]
		public readonly List<AtomLink> Links = new List<AtomLink>();

		[XmlIgnore]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool LinksSpecified
		{
			get { return (Links.Count > 0); }
			set { }
		}

		[DefaultValue(null)]
		[XmlElement("rights")]
		public AtomText Rights
		{
			get { return _rights; }
			set { _rights = value; }
		}

		/// <remarks>
		/// Required even if empty.
		/// </remarks>
		[XmlElement("updated")]
		public virtual AtomDate Updated
		{
			get { return _updated; }
			set { _updated = value; }
		}

		[XmlIgnore]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public virtual bool UpdatedSpecified
		{
			get { return true; }
			set { }
		}

	    public override void AddNamespaces(XmlSerializerNamespaces namespaces)
		{
			foreach (AtomLink link in Links)
			{
				link.AddNamespaces(namespaces);
			}

			foreach (AtomCategory category in Categories)
			{
				category.AddNamespaces(namespaces);
			}

			foreach (AtomPerson person in Authors)
			{
				person.AddNamespaces(namespaces);
			}

			base.AddNamespaces(namespaces);
		}

	    Uri IUriProvider.Uri => _id;
	}
}
