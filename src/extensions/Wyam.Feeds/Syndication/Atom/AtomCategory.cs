using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Wyam.Feeds.Syndication.Atom
{
    /// <summary>
	/// http://tools.ietf.org/html/rfc4287#section-4.2.2
	/// </summary>
	[Serializable]
    public class AtomCategory : AtomCommonAttributes
	{
        private Uri _scheme = null;
		private string _term = null;
		private string _label = null;
		private string _value = null;

        /// <summary>
		/// Ctor
		/// </summary>
		public AtomCategory()
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="text"></param>
		public AtomCategory(string term)
		{
			_term = term;
		}

        [DefaultValue(null)]
		[XmlAttribute("scheme")]
		public string Scheme
		{
			get { return ConvertToString(_scheme); }
			set { _scheme = ConvertToUri(value); }
		}

		[DefaultValue(null)]
		[XmlAttribute("term")]
		public string Term
		{
			get { return _term; }
			set { _term = string.IsNullOrEmpty(value) ? null : value; }
		}

		[DefaultValue(null)]
		[XmlAttribute("label")]
		public string Label
		{
			get { return _label; }
			set { _label = string.IsNullOrEmpty(value) ? null : value; }
		}

		[XmlText]
		[DefaultValue(null)]
		public string Value
		{
			get { return _value; }
			set { _value = string.IsNullOrEmpty(value) ? null : value; }
		}

        public static implicit operator AtomCategory(string value)
		{
			return new AtomCategory(value);
		}

		public static explicit operator string(AtomCategory value)
		{
			return value.Value;
		}
	}
}
