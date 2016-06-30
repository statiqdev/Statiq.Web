using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Wyam.Feeds.Syndication.Atom;
using Wyam.Feeds.Syndication.Rdf;
using Wyam.Feeds.Syndication.Rss;

namespace Wyam.Feeds.Syndication
{
    /// <summary>
    /// Utility for serialization
    /// </summary>
    public static class FeedSerializer
	{
	    /*
		 * From MSDN:
		 * 
		 * To increase performance, the XML serialization infrastructure dynamically generates
		 * assemblies to serialize and deserialize specified types. The infrastructure finds and
		 * reuses those assemblies. This behavior occurs only when using the following constructors:
		 * 
		 *		System.Xml.Serialization.XmlSerializer(Type) 
		 *		System.Xml.Serialization.XmlSerializer(Type,String) 
		 * 
		 * If you use any of the other constructors, multiple versions of the same assembly are generated
		 * and never unloaded, resulting in a memory leak and poor performance. The simplest solution is
		 * to use one of the two constructors above. Otherwise, you must cache the assemblies in a Hashtable,
		 * as shown in the following example.
		 */
         
		public static IFeed DeserializeXml(Stream input)
		{
			XmlReaderSettings settings = new XmlReaderSettings();
			settings.IgnoreComments = true;
			settings.IgnoreWhitespace = true;
			settings.IgnoreProcessingInstructions = true;

			using (XmlReader reader = XmlReader.Create(input, settings))
			{
				reader.MoveToContent();

				Type type = GetFeedType(reader.NamespaceURI, reader.LocalName);

				XmlSerializer serializer = new XmlSerializer(type);
				return serializer.Deserialize(reader) as IFeed;
			}
		}

        public static void SerializeXml(IFeed feed, Stream output) =>
            SerializeXml(feed, output, null);

        public static void SerializeXml(IFeed feed, Stream output, string xsltUrl) => 
            SerializeXml(feed, output, xsltUrl, true);

        public static void SerializeXml(IFeed feed, Stream output, string xsltUrl, bool prettyPrint) =>
            SerializeXml(feed.FeedType, feed, output, xsltUrl, prettyPrint);

        public static void SerializeXml(FeedType feedType, IFeed feed, Stream output, string xsltUrl, bool prettyPrint)
		{
            if (feedType == null)
            {
                throw new ArgumentNullException(nameof(feedType));
            }

            // Do we need to transform the feed?
            if (feedType != feed.FeedType)
            {
                feed = feedType.GetFeed(feed);
            }

            // Setup document formatting, make human readable
            XmlWriterSettings settings = new XmlWriterSettings
            {
                CheckCharacters = true,
                CloseOutput = true,
                ConformanceLevel = ConformanceLevel.Document,
                Encoding = System.Text.Encoding.UTF8
            };
            if (prettyPrint)
			{
				settings.Indent = true;
				settings.IndentChars = "\t";
			}
			else
			{
				settings.Indent = false;
				settings.NewLineChars = string.Empty;
			}
			settings.NewLineHandling = NewLineHandling.Replace;

			XmlWriter writer = XmlWriter.Create(output, settings);

			// Add a stylesheet for browser viewing
			if (!string.IsNullOrEmpty(xsltUrl))
			{
				// Render the XSLT processor instruction
				writer.WriteProcessingInstruction("xml-stylesheet", $"type=\"text/xsl\" href=\"{xsltUrl}\" version=\"1.0\"");
			}

			// Get all namespaces
			XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
			feed.AddNamespaces(namespaces);

			// Serialize feed
			XmlSerializer serializer = new XmlSerializer(feed.GetType());
			serializer.Serialize(writer, feed, namespaces);
		}

	    private static Type GetFeedType(string namespaceUri, string rootElement)
		{
			switch (rootElement)
			{
				case AtomFeed.RootElement:
				{
					switch (namespaceUri)
					{
						case AtomFeed.Namespace:
						{
							return typeof(AtomFeed);
						}
						case AtomFeedOld.Namespace:
						{
							return typeof(AtomFeedOld);
						}
					}
					break;
				}
				case RssFeed.RootElement:
				{
					return typeof(RssFeed);
				}
				case RdfFeedBase.RootElement:
				{
					return typeof(RdfFeed);
				}
			}

			return typeof(IFeed);
		}
	}
}
