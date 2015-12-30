using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Pipelines;
using Wyam.Common.IO;

namespace Wyam.Modules.Rss
{
    public class Rss : IModule
    {
        /// <summary>
        /// RSS feed title. Required.
        /// </summary>
        private string _feedTitle = string.Empty;

        /// <summary>
        /// RSS feed description. Required.
        /// </summary>
        private string _feedDescription = string.Empty;

        private string _siteRoot = null;

        private string _outputRssFilePath = null;

        // Default meta keys
        private string _pubDateMetaKey = "RssPubDate";

        private string _titleMetaKey = "RssTitle";

        private string _descriptionMetaKey = "RssDescription";

        // Options
        private bool _appendGuid = true;

        private string _language = null;

        private bool _assumePermalinks = false;

        private Func<string, string> _linkCustomizerDelegate = null;

        private readonly static XNamespace atomNamespace = XNamespace.Get(@"http://www.w3.org/2005/Atom");

        /// <summary>
        /// Creates RSS feed from input documents.
        /// </summary>
        /// <param name="siteRoot">Site root URL (example: "http://mysite.com")</param>
        /// <param name="feedTitle">Title of the RSS feed.</param>
        /// <param name="feedDescription">Description of the RSS feed.</param>
        /// <param name="outputRssFilePath">Relative output location where generated RSS feed will be placed on server. (example: "blog/feed.rss")</param>
        /// <remarks>
        /// <para>
        /// Implements RSS and Atom feeds. All input documents should have <c>RssTitle</c> and <c>RssDescription</c> metadata
        /// because they are required by RSS. You can override these metadata keys via <c>WithTitleMetaKey(string key)</c>
        /// and <c>WithDescriptionMetaKey(string key)</c>.
        /// </para>
        /// <para>
        /// This module outputs input files without changes and one another file: the rss feed.
        /// The RSS feed contains <c>RelativeFilePath</c> and <c>IsRssFeed: true</c> metadata keys.
        /// Use <c>WriteFiles</c> module inside <c>Branch</c> module without parameters to place file 
        /// in apporiate file system location. See example.
        /// </para>
        /// <para>
        /// Options:
        /// * Use <c>WithoutGuid()</c> to disable generation of unique identifier for each input document. Guid
        /// is generated using input document's source location. You may want to disable it if your data is generated
        /// on fly.
        /// * Optional <c>RssPubDate</c> metadata will be used to specify document's publication date.
        /// You can override this metadata key with <c>WithPublicationDateMetaKey(string key)</c>.
        /// * Language can be set for feed using <c>WithLanguage(string lang)</c>. Language string must
        /// follow these conventions: http://www.rssboard.org/rss-language-codes
        ///  * Use <c>WithLinkCustomizer</c> to create pretty urls.
        /// </para>
        /// <para>
        /// Some things to know:
        /// * If publication date for each document is not set, most RSS reader programs will assume
        /// that most recent publication is most toppest in the output RSS file (that is first input document).
        /// Use <c>OrderBy</c> module to order your documents in right order.
        /// * Use feed validator when generating RSS for first time: https://validator.w3.org/feed/
        /// * Only basic tags are defined in Rss module at this moment, eg. you cannot attach author tag to
        /// specify RSS item's author.
        /// </para>
        /// </remarks>
        /// <example>
        /// // Input/posts/hello.md
        /// <code>
        /// ---
        /// RssTitle: Hello, world!
        /// RssDescription: My first blog post
        /// RssPubDate: 10/11/12
        /// ---
        /// 
        /// Hello, RSS world!
        /// </code>
        /// 
        /// // config.wyam
        /// <code>
        /// ===
        /// ---
        /// Pipelines.Add("Blog posts",
        ///     ReadFiles("posts/*.md"),
        ///     FrontMatter(Yaml()),
        ///     Markdown(),
        ///     WriteFiles(".html"),
        ///     Rss(siteRoot: "http://example.org",
        ///         outputRssFilePath: "posts/feed.rss"
        ///         feedTitle: "My awesome blog",
        ///         feedDescription: "Blog about something"
        ///     ),
        ///     WriteFiles()
        /// );
        /// </code>
        /// 
        /// // Output/posts/feed.rss
        /// <![CDATA[<?xml version = "1.0" encoding="utf-8"?>
        /// <rss version="2.0" xmlns:atom="http://www.w3.org/2005/Atom">
        ///   <channel>
        ///     <title>My awesome blog</title>
        ///     <description>Blog about something</description>
        ///     <link>http://example.org</link>
        ///     <atom:link href="http://example.org/posts/feed.rss" rel="self" />
        ///     <lastBuildDate>Wed, 30 Dec 2015 13:39:16 +0300</lastBuildDate>
        ///     <item>
        ///       <title>Hello, world!</title>
        ///       <link>http://example.org/posts/hello.html</link>
        ///       <description>My first blog post</description>
        ///       <pubDate>Sat, 10 Nov 2012 00:00:00 +0400</pubDate>
        ///       <guid isPermaLink="false"> 81a26807-355a-dbf9-7729-c6601f1d8a2b</guid>
        ///     </item>
        ///   </channel>
        /// </rss>]]>
        /// </example>
        /// <seealso cref="WithTitleMetaKey(string)"/>
        /// <seealso cref="WithPublicationDateMetaKey(string)"/>
        /// <seealso cref="WithoutGuid()"/>
        /// <seealso cref="WithLanguage(string)"/>
        /// <metadata name="RssPath" type="string">Absolute path to output RSS file on server.</metadata>
        /// <metadata name="WritePath" type="string">Path where RSS file will be written via WriteFiles module.</metadata>
        /// <caterogy>Metadata</caterogy>
        public Rss(string siteRoot, string outputRssFilePath, string feedTitle, string feedDescription)
        {
            if (string.IsNullOrEmpty(siteRoot))
                throw new ArgumentException("siteRoot");
            if (string.IsNullOrEmpty(outputRssFilePath))
                throw new ArgumentException("outputRssFilePath");
            if (string.IsNullOrEmpty(feedTitle))
                throw new ArgumentException("feedTitle");
            if (string.IsNullOrEmpty(feedDescription))
                throw new ArgumentException("feedDescription");

            _siteRoot = PathHelper.ToLink(siteRoot.TrimEnd(new char[] { '/', '\\' }));
            _outputRssFilePath = outputRssFilePath;
            _feedDescription = feedDescription;
            _feedTitle = feedTitle;
        }

        /// <summary>
        /// Set metadata key to lookup in documents for RSS item publication date.
        /// </summary>
        public Rss WithPublicationDateMetaKey(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("key");

            _pubDateMetaKey = key;
            return this;
        }

        /// <summary>
        /// Set metadata key to lookup in documents for RSS item title.
        /// </summary>
        public Rss WithTitleMetaKey(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("key");

            _titleMetaKey = key;
            return this;
        }

        public Rss WithDescriptionMetaKey(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("key");

            _descriptionMetaKey = key;
            return this;
        }

        /// <summary>
        /// Set feed language.
        /// </summary>
        /// <param name="lang">RSS language code</param>
        /// <returns></returns>
        public Rss WithLanguage(string lang)
        {
            if (string.IsNullOrEmpty(lang))
                throw new ArgumentException("lang");

            _language = lang;
            return this;
        }

        /// <summary>
        /// Assume that RSS item link is unique and permanent.
        /// </summary>
        /// <returns></returns>
        public Rss AssumePermalinks()
        {
            _assumePermalinks = true;
            return this;
        }

       
        
        /// <summary>
        /// Allows to customize output links.
        /// Does not execute when document explicitly
        /// defines output URL via RssItemUrl metadata.
        /// </summary>
        /// <param name="customizer"></param>
        /// <returns></returns>
        public Rss WithLinkCustomizer(Func<string, string> customizer) {
            if (customizer == null)
                throw new ArgumentNullException("customizer");

            _linkCustomizerDelegate = customizer;
            return this;
        }

        /// <summary>
        /// Do not append GUIDs to RSS items.
        /// </summary>
        public Rss WithoutGuid()
        {
            _appendGuid = false;
            return this;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            XDocument rss = new XDocument(new XDeclaration("1.0", "UTF-8", null));
            var rssAbsolutePath = PathHelper.ToLink(Path.Combine(_siteRoot, _outputRssFilePath));

            var rssRoot = new XElement("rss",
                new XAttribute("version", "2.0"),
                new XAttribute(XNamespace.Xmlns + "atom", atomNamespace)
            );

            var channel = new XElement("channel",
                new XElement("title", _feedTitle),
                new XElement("description", _feedDescription),
                new XElement("link", _siteRoot),
                new XElement(atomNamespace + "link",
                    new XAttribute("href", rssAbsolutePath),
                    new XAttribute("rel", "self")
                ),
                new XElement("lastBuildDate", DateTime.Now.ToRssDate())
            );

            if (_language != null)
            {
                channel.Add(new XElement("language", _language));
            }

            foreach (var input in inputs)
            {
                var item = new XElement("item");

                object title;
                bool hasTitle = input.TryGetValue("RssTitle", out title);
                item.Add(new XElement("title", hasTitle ? title : "Untitled"));

                string link = null;
                {
                    object linkTemp = null;
                    if (input.TryGetValue("RssItemUrl", out linkTemp))
                    {
                        link = PathHelper.ToRootLink((string)linkTemp);
                    }
                    else
                    {
                        linkTemp = null;
                        if (input.TryGetValue("RelativeFilePath", out linkTemp))
                        {
                            link = PathHelper.ToRootLink((string)linkTemp);
                            if (_linkCustomizerDelegate != null)
                                link = _linkCustomizerDelegate(link);
                        }
                    }
                }

                //foreach (var m in input.Metadata)
                //{
                //    Console.WriteLine("{0}: {1}", m.Key, m.Value);
                //}

                if (string.IsNullOrWhiteSpace(link))
                {
                    throw new ArgumentException("Required RssItemUrl or RelativeFilePath was not found in metadata in document " + input.Source);
                }

                item.Add(new XElement("link", PathHelper.ToLink(_siteRoot + link)));

                object description = null;
                bool hasDescription = input.TryGetValue(_descriptionMetaKey, out description);
                item.Add(new XElement("description", hasDescription ? description : "No description"));

                object pubDate = null;
                if (input.TryGetValue(_pubDateMetaKey, out pubDate))
                {
                    item.Add(new XElement("pubDate", DateTime.Parse((string)pubDate).ToRssDate()));
                }

                if (_appendGuid)
                {
                    if (_assumePermalinks)
                    {
                        item.Add(new XElement("guid", link));
                    }
                    else if (string.IsNullOrWhiteSpace(input.Source))
                    {
                        context.Trace.Warning("Cannot generate RSS item guid for document " + input.Source + " because document Source is not valid.");
                    }
                    else
                    {
                        item.Add(new XElement("guid", GuidHelper.ToGuid(input.Source).ToString(), new XAttribute("isPermaLink", false)));
                    }
                }

                channel.Add(item);   
            }

            rss.Add(rssRoot);
            rssRoot.Add(channel);

            // rss.ToString() doesn't return XML declaration
            var outText = new StringBuilder();
            using (var stream = new UTF8StringWriter(outText))
            {
                rss.Save(stream);
            }

            return new IDocument[] { context.GetNewDocument(outText.ToString(), new KeyValuePair<string, object>[] {
                new KeyValuePair<string, object>("IsRssFeed", true),
                new KeyValuePair<string, object>("RelativeFilePath", _outputRssFilePath),
                new KeyValuePair<string, object>("WritePath", _outputRssFilePath)
            })};
        }
    }

    internal class GuidHelper
    {
        public static Guid ToGuid(string input)
        {
            byte[] stringBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashed = new System.Security.Cryptography
                .SHA1CryptoServiceProvider()
                .ComputeHash(stringBytes);

            Array.Resize(ref hashed, 16);
            return new Guid(hashed);
        }
    }

    internal class UTF8StringWriter : StringWriter
    {
        public override Encoding Encoding { get { return Encoding.UTF8; } }

        public UTF8StringWriter(StringBuilder builder)
            : base(builder)
        {

        }
    }
}
