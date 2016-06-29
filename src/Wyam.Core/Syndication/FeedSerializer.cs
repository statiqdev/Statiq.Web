using System;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Serialization;
using Wyam.Core.Syndication.Atom;
using Wyam.Core.Syndication.Rdf;
using Wyam.Core.Syndication.Rss;

namespace Wyam.Core.Syndication
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

				Type type = FeedSerializer.GetFeedType(reader.NamespaceURI, reader.LocalName);

				XmlSerializer serializer = new XmlSerializer(type);
				return serializer.Deserialize(reader) as IFeed;
			}
		}
        
		public static void SerializeXml(IFeed feed, Stream output, string xsltUrl) => 
            FeedSerializer.SerializeXml(feed, output, xsltUrl, true);

	    public static void SerializeXml(IFeed feed, Stream output, string xsltUrl, bool prettyPrint)
		{
			// Setup document formatting, make human readable
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.CheckCharacters = true;
			settings.CloseOutput = true;
			settings.ConformanceLevel = ConformanceLevel.Document;
			settings.Encoding = System.Text.Encoding.UTF8;
			if (prettyPrint)
			{
				settings.Indent = true;
				settings.IndentChars = "\t";
			}
			else
			{
				settings.Indent = false;
				settings.NewLineChars = String.Empty;
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
				case RdfFeed.RootElement:
				{
					return typeof(RdfFeed);
				}
			}

			return typeof(IFeed);
		}
	}
}
