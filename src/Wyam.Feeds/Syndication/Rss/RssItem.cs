using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using Wyam.Feeds.Syndication.Extensions;

namespace Wyam.Feeds.Syndication.Rss
{
	/// <summary>
	/// RSS 2.0 Item
	///		http://blogs.law.harvard.edu/tech/rss#hrelementsOfLtitemgt
	/// </summary>
	[Serializable]
	public class RssItem : RssBase, IFeedItem
	{
	    // required
		private string _title = null;
		private Uri _link = null;
		private string _description = null;

		//optional
		private RssPerson _author = null;
		private Uri _comments = null;
		private RssEnclosure _enclosure = null;
		private RssGuid _guid = null;
		private RssDate _pubDate;
		private RssSource _source = null;

		// extensions
		private DublinCore _dublinCore = null;
		private string _contentEncoded = null;
		private Uri _wfwComment = null;
		private Uri _wfwCommentRss = null;
		private int? _slashComments = null;

	    public RssItem()
	    {
	    }

	    public RssItem(IFeedItem source)
	    {

            // ** IFeedMetadata

            // ID
            _guid = new RssGuid
            {
                Value = source.ID.ToString()
            };

            // Title
            string title = source.Title;
            if (!string.IsNullOrWhiteSpace(title))
            {
                Title = title;
            }

            // Description
            string description = source.Description;
            if (!string.IsNullOrEmpty(description))
            {
                Description = description;
            }

            // Author
            string author = source.Author;
            if (!string.IsNullOrEmpty(author))
            {
                Author = new RssPerson
                {
                    Name = author
                };
            }

            // Published
            DateTime? published = source.Published;
            if (published.HasValue)
            {
                PubDate = new RssDate(published.Value);
            }

            // Updated
            DateTime? updated = source.Updated;
            if (updated.HasValue)
            {
                PubDate = new RssDate(updated.Value);
            }

            // Link
            Uri link = source.Link;
            if (link != null)
            {
                Link = link.ToString();
            }

            // ImageLink
	        Uri imageLink = source.ImageLink;
	        if (imageLink != null)
	        {
	            _enclosure = new RssEnclosure
	            {
	                Url = imageLink.ToString(),
	                Type = "image"
	            };
	        }

            // ** IFeedItem

            // ThreadLink
            Uri threadLink = source.ThreadLink;
            if (threadLink != null)
            {
                _wfwCommentRss = threadLink;
            }

            // ThreadCount
            int? threadCount = source.ThreadCount;
            if (threadCount.HasValue)
            {
                SlashComments = threadCount.Value;
            }

            // ThreadUpdated
            // Not in RDF
        }

        /// <summary>
        /// Gets and sets the title of the item.
        /// </summary>
        [DefaultValue(null)]
		[XmlElement("title")]
		public string Title
		{
			get { return _title; }
			set { _title = string.IsNullOrEmpty(value) ? null : value; }
		}

		/// <summary>
		/// Gets and sets the url of the item.
		/// </summary>
		[DefaultValue(null)]
		[XmlElement("link")]
		public string Link
		{
			get { return ConvertToString(_link); }
			set { _link = ConvertToUri(value); }
		}

		/// <summary>
		/// Gets and sets the description of the item.
		/// </summary>
		[DefaultValue(null)]
		[XmlElement("description")]
		public string Description
		{
			get { return _description; }
			set { _description = string.IsNullOrEmpty(value) ? null : value; }
		}

		/// <summary>
		/// Gets and sets the author of the item.
		/// </summary>
		[XmlElement("author")]
		public RssPerson Author
		{
			get
			{
				if (_author == null)
				{
					_author = new RssPerson();
				}

				return _author;
			}
			set { _author = value; }
		}

		[XmlIgnore]
		public bool AuthorSpecified
		{
			get { return (_author != null && !_author.IsEmpty()); }
			set { }
		}

		[XmlElement("category")]
		public readonly List<RssCategory> Categories = new List<RssCategory>();

		[XmlIgnore]
		public bool CategoriesSpecified
		{
			get { return (Categories.Count > 0); }
			set { }
		}

		/// <summary>
		/// Gets and sets a URL to the comments about the item.
		/// </summary>
		[DefaultValue(null)]
		[XmlElement("comments")]
		public string Comments
		{
			get { return ConvertToString(_comments); }
			set { _comments = ConvertToUri(value); }
		}

		[XmlElement("enclosure")]
		public RssEnclosure Enclosure
		{
			get
			{
				if (_enclosure == null)
				{
					_enclosure = new RssEnclosure();
				}
				return _enclosure;
			}
			set { _enclosure = value; }
		}

		[XmlIgnore]
		public bool EnclosureSpecified
		{
			get { return (_enclosure != null) && _enclosure.HasValue; }
			set { }
		}

		[XmlElement("guid")]
		public RssGuid Guid
		{
			get
			{
				if (_guid == null)
				{
					_guid = new RssGuid();
				}
				return _guid;
			}
			set { _guid = value; }
		}

		[XmlIgnore]
		public bool GuidSpecified
		{
			get { return (_guid != null) && _guid.HasValue; }
			set { }
		}

		[DefaultValue(null)]
		[XmlElement("pubDate")]
		public RssDate PubDate
		{
			get { return _pubDate; }
			set { _pubDate = value; }
		}

		[XmlIgnore]
		public bool PubDateSpecified
		{
			get { return _pubDate.HasValue; }
			set { }
		}

		[DefaultValue(null)]
		[XmlElement("source")]
		public RssSource Source
		{
			get { return _source; }
			set { _source = value; }
		}

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
				if (_guid == null)
				{
					return null;
				}

				return ((IUriProvider)_guid).Uri;
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
				if (!AuthorSpecified)
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
				if (string.IsNullOrEmpty(_author.Name))
				{
					return _author.Email;
				}
				return _author.Name;
			}
		}

		DateTime? IFeedMetadata.Published
		{
			get
			{
				if (!_pubDate.HasValue)
				{
					string date = DublinCore[DublinCore.TermName.Date];
					return ConvertToDateTime(date);
				}

				return _pubDate.Value;
			}
		}

		DateTime? IFeedMetadata.Updated => ((IFeedMetadata)this).Published;

	    Uri IFeedMetadata.Link => _link;

	    Uri IFeedMetadata.ImageLink
		{
			get
			{
				if (!EnclosureSpecified)
				{
					return null;
				}

				string type = _enclosure.Type;
				if (string.IsNullOrEmpty(type) ||
					!type.StartsWith("image", StringComparison.InvariantCultureIgnoreCase))
				{
					return null;
				}

				return ((IUriProvider)_enclosure).Uri;
			}
		}

		Uri IFeedItem.ThreadLink
		{
			get
			{
				if (_comments == null)
				{
					return _wfwCommentRss;
				}
				return _comments;
			}
		}

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
