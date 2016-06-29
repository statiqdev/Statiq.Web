using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;

namespace Wyam.Core.Syndication.Extensions
{
    /// <summary>
    /// Allows any generic extensions expressed as XmlElements and XmlAttributes
    /// </summary>
    public abstract class ExtensibleBase : INamespaceProvider
	{
	    protected const string ContentPrefix = "content";
		protected const string ContentNamespace = "http://purl.org/rss/1.0/modules/content/";
		protected const string ContentEncodedElement = "encoded";

		protected const string WfwPrefix = "wfw";
		protected const string WfwNamespace = "http://wellformedweb.org/CommentAPI/";
		protected const string WfwCommentElement = "comment";
		protected const string WfwCommentRssElement = "commentRss";

		protected const string SlashPrefix = "slash";
		protected const string SlashNamespace = "http://purl.org/rss/1.0/modules/slash/";
		//protected const string SlashSectionElement = "section";
		//protected const string SlashDepartmentElement = "department";
		protected const string SlashCommentsElement = "comments";
		//protected const string SlashHitParadeElement = "hit_parade";

	    [XmlAnyElement]
		public readonly List<XmlElement> ElementExtensions = new List<XmlElement>();

		[XmlIgnore]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ElementExtensionsSpecified
		{
			get { return ElementExtensions.Count > 0; }
			set { }
		}

		[XmlAnyAttribute]
		public readonly List<XmlAttribute> AttributeExtensions = new List<XmlAttribute>();

		[XmlIgnore]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool AttributeExtensionsSpecified
		{
			get { return AttributeExtensions.Count > 0; }
			set { }
		}

	    /// <summary>
		/// Applies the extensions in adapter to ExtensibleBase
		/// </summary>
		/// <param name="adapter"></param>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public void AddExtensions(IExtensionAdapter adapter)
		{
			if (adapter == null)
			{
				return;
			}

			IEnumerable<XmlAttribute> attributes = adapter.GetAttributeEntensions();
			if (attributes != null)
			{
				AttributeExtensions.AddRange(attributes);
			}

			IEnumerable<XmlElement> elements = adapter.GetElementExtensions();
			if (elements != null)
			{
				ElementExtensions.AddRange(elements);
			}
		}

		/// <summary>
		/// Extracts the extensions in this ExtensibleBase into adapter
		/// </summary>
		/// <param name="adapter"></param>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected void FillExtensions(IExtensionAdapter adapter)
		{
			if (adapter == null)
			{
				return;
			}

			adapter.SetAttributeEntensions(AttributeExtensions);
			adapter.SetElementExtensions(ElementExtensions);
		}

	    [EditorBrowsable(EditorBrowsableState.Advanced)]
		public virtual void AddNamespaces(XmlSerializerNamespaces namespaces)
		{
			foreach (XmlAttribute node in AttributeExtensions)
			{
				if (String.IsNullOrEmpty(node.Prefix))
				{
					// do not let extensions overwrite the default namespace
					continue;
				}
				namespaces.Add(node.Prefix, node.NamespaceURI);
			}
			foreach (XmlElement node in ElementExtensions)
			{
				if (String.IsNullOrEmpty(node.Prefix))
				{
					// do not let extensions overwrite the default namespace
					continue;
				}
				namespaces.Add(node.Prefix, node.NamespaceURI);
			}
		}

	    public static string ConvertToString(DateTime dateTime)
		{
			return XmlConvert.ToString(dateTime, XmlDateTimeSerializationMode.Utc);
		}

		public static DateTime? ConvertToDateTime(string value)
		{
			DateTime dateTime;
			if (!DateTime.TryParse(value, out dateTime))
			{
				return null;
			}
			return dateTime;
		}

	    protected static string ConvertToString(Uri uri) => 
            uri == null ? null : Uri.EscapeUriString(uri.ToString());

	    protected static Uri ConvertToUri(string value)
		{
			Uri uri;
			if (String.IsNullOrEmpty(value) ||
				!Uri.TryCreate(value, UriKind.RelativeOrAbsolute, out uri))
			{
				return null;
			}

			return uri;
		}
	}
}