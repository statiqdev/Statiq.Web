using System;
using System.ComponentModel;
using System.Xml.Serialization;
using Wyam.Core.Syndication.Extensions;

namespace Wyam.Core.Syndication.Rdf
{
	/// <summary>
	/// RDF 1.0 Item
	///		http://web.resource.org/rss/1.0/spec#s5.5
	/// </summary>
	public class RdfItem : RdfBase, IFeedItem
	{
	    private string _description = string.Empty;

		// extensions
		private DublinCore _dublinCore = null;
		private string _contentEncoded = null;
		private Uri _wfwComment = null;
		private Uri _wfwCommentRss = null;
		private int? _slashComments = null;

	    /// <summary>
		/// Gets and sets a brief description of the channel's content, function, source, etc.
		/// </summary>
		/// <remarks>
		/// Suggested maximum length is 500 characters.
		/// Required even if empty.
		/// </remarks>
		[XmlElement("description")]
		public string Description
		{
			get { return _description; }
			set { _description = string.IsNullOrEmpty(value) ? string.Empty : value; }
		}

		protected internal string Copyright => DublinCore[DublinCore.TermName.Rights];

	    /// <summary>
		/// Gets and sets the encoded content for this item
		/// </summary>
		[DefaultValue(null)]
		[XmlElement(ContentEncodedElement, Namespace=ContentNamespace)]
		public string ContentEncoded
		{
			get { return _contentEncoded; }
			set { _contentEncoded = value; }
		}

	    /// <summary>
		/// Gets and sets the Uri to which comments can be POSTed
		/// </summary>
		[DefaultValue(null)]
		[XmlElement(WfwCommentElement, Namespace=WfwNamespace)]
		public string WfwComment
		{
			get { return ConvertToString(_wfwComment); }
			set { _wfwComment = ConvertToUri(value); }
		}

		/// <summary>
		/// Gets and sets the Uri at which a feed of comments can be found
		/// </summary>
		[DefaultValue(null)]
		[XmlElement(WfwCommentRssElement, Namespace=WfwNamespace)]
		public string WfwCommentRss
		{
			get { return ConvertToString(_wfwCommentRss); }
			set { _wfwCommentRss = ConvertToUri(value); }
		}

	    /// <summary>
		/// Gets and sets the number of comments for this item
		/// </summary>
		[DefaultValue(null)]
		[XmlElement(SlashCommentsElement, Namespace=SlashNamespace)]
		public int SlashComments
		{
			get
			{
				if (!_slashComments.HasValue)
				{
					return 0;
				}
				return _slashComments.Value;
			}
			set { _slashComments = value; }
		}

		[XmlIgnore]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool SlashCommentsSpecified
		{
			get { return _slashComments.HasValue; }
			set { }
		}

	    /// <summary>
		/// Allows IWebFeedItem to access DublinCore
		/// </summary>
		/// <remarks>
		/// Note this only gets filled on first access
		/// </remarks>
		private DublinCore DublinCore
		{
			get
			{
				if (_dublinCore == null)
				{
					_dublinCore = new DublinCore();
					FillExtensions(_dublinCore);
				}
				return _dublinCore;
			}
		}

		Uri IFeedMetadata.ID
		{
			get
			{
				Uri id = ((IUriProvider)this).Uri;
				if (id == null)
				{
					Uri.TryCreate(About, UriKind.RelativeOrAbsolute, out id);
				}
				return id;
			}
		}

		string IFeedMetadata.Title
		{
			get
			{
				string title = Title;
				if (string.IsNullOrEmpty(title))
				{
					title = DublinCore[DublinCore.TermName.Title];
				}
				return title;
			}
		}

		string IFeedMetadata.Description
		{
			get
			{
				string description = _description;
				if (string.IsNullOrEmpty(description))
				{
					description = ContentEncoded;
					if (string.IsNullOrEmpty(description))
					{
						description = DublinCore[DublinCore.TermName.Description];
						if (string.IsNullOrEmpty(description))
						{
							description = DublinCore[DublinCore.TermName.Subject];
						}
					}
				}
				return description;
			}
		}

		string IFeedMetadata.Author
		{
			get
			{
				string author = DublinCore[DublinCore.TermName.Creator];
				if (string.IsNullOrEmpty(author))
				{
					author = DublinCore[DublinCore.TermName.Contributor];

					if (string.IsNullOrEmpty(author))
					{
						author = DublinCore[DublinCore.TermName.Publisher];
					}
				}
				return author;
			}
		}

		DateTime? IFeedMetadata.Published
		{
			get
			{
				string date = DublinCore[DublinCore.TermName.Date];
				return ConvertToDateTime(date);
			}
		}

		DateTime? IFeedMetadata.Updated
		{
			get
			{
				string date = DublinCore[DublinCore.TermName.Date];
				return ConvertToDateTime(date);
			}
		}

		Uri IFeedMetadata.Link => ((IUriProvider)this).Uri;

	    Uri IFeedMetadata.ImageLink => null;

	    Uri IFeedItem.ThreadLink => _wfwCommentRss;

	    int? IFeedItem.ThreadCount
		{
			get
			{
				if (!SlashCommentsSpecified)
				{
					return null;
				}
				return SlashComments;
			}
		}

		DateTime? IFeedItem.ThreadUpdated => null;

	    public override void AddNamespaces(XmlSerializerNamespaces namespaces)
		{
			if (_contentEncoded != null)
			{
				namespaces.Add(ContentPrefix, ContentNamespace);
			}

			if (_wfwComment != null || _wfwCommentRss != null)
			{
				namespaces.Add(WfwPrefix, WfwNamespace);
			}

			base.AddNamespaces(namespaces);
		}
	}
}
