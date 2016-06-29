using System.Collections.Generic;

namespace Wyam.Core.Syndication
{
    /// <summary>
    /// Feed interface
    /// </summary>
    public interface IFeed : IFeedMetadata, INamespaceProvider
	{
		/// <summary>
		/// Gets the MIME Type designation for the feed
		/// </summary>
		string MimeType { get; }

		/// <summary>
		/// Gets the copyright
		/// </summary>
		string Copyright { get; }

		/// <summary>
		/// Gets a list of feed items
		/// </summary>
		IList<IFeedItem> Items { get; }
	}
}
