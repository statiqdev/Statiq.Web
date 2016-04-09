using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Tracing;

namespace Wyam.Core.Modules.Contents
{
    /// <summary>
    /// Creates RSS feed from input documents.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Implements RSS and Atom feeds. All input documents should have <c>RssTitle</c> and <c>RssDescription</c> metadata
    /// because they are required by RSS. You can override these metadata keys via <c>WithTitleMetaKey(string key)</c>
    /// and <c>WithDescriptionMetaKey(string key)</c>.
    /// </para>
    /// <para>
    /// This module outputs input files without changes and one another file: the RSS feed.
    /// The RSS feed contains <c>RelativeFilePath</c> and <c>IsRssFeed: true</c> metadata keys.
    /// Use <c>WriteFiles</c> module inside <c>Branch</c> module without parameters to place file 
    /// in appropriate file system location. See example.
    /// </para>
    /// <para>
    /// Options:
    /// <list type="bullet">
    /// <item><description>Use <c>WithoutGuid()</c> to disable generation of unique identifier for each input document. Guid
    /// is generated using input document's source location. You may want to disable it if your data is generated
    /// on fly.</description></item>
    /// <item><description>Optional <c>RssPubDate</c> metadata will be used to specify document's publication date.
    /// You can override this metadata key with <c>WithPublicationDateMetaKey(string key)</c>.</description></item>
    /// <item><description>Language can be set for feed using <c>WithLanguage(string lang)</c>. Language string must
    /// follow these conventions: http://www.rssboard.org/rss-language-codes</description></item>
    /// <item><description>Use <c>WithLinkCustomizer</c> to create pretty urls.</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Some things to know:
    /// <list type="bullet">
    /// <item><description>If publication date for each document is not set, most RSS reader programs will assume
    /// that most recent publication is the top most in the output RSS file (that is first input document).</description></item>
    /// <item><description>Use <c>OrderBy</c> module to order your documents in right order.</description></item>
    /// <item><description>Use a feed validator when generating RSS for first time: https://validator.w3.org/feed/</description></item>
    /// <item><description>Only basic tags are defined in this module at this moment, eg. you cannot attach author tag to
    /// specify RSS item's author.</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// Input/posts/hello.md:
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
    /// config.wyam:
    /// <code>
    /// Pipelines.Add("Blog posts",
    ///     ReadFiles("posts/*.md"),
    ///     FrontMatter(Yaml()),
    ///     Markdown(),
    ///     WriteFiles(".html"),
    ///     Rss(siteRoot: "http://example.org",
    ///         outputRssFilePath: "posts/feed.rss",
    ///         feedTitle: "My awesome blog",
    ///         feedDescription: "Blog about something"
    ///     ),
    ///     WriteFiles()
    /// );
    /// </code>
    /// 
    /// Output/posts/feed.rss:
    /// <code>
    /// &lt;![CDATA[&lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot;?&gt;
    /// &lt;rss version=&quot;2.0&quot; xmlns:atom=&quot;http://www.w3.org/2005/Atom&quot;&gt;
    ///   &lt;channel&gt;
    ///     &lt;title&gt;My awesome blog&lt;/title&gt;
    ///     &lt;description&gt;Blog about something&lt;/description&gt;
    ///     &lt;link&gt;http://example.org&lt;/link&gt;
    ///     &lt;atom:link href=&quot; http://example.org/posts/feed.rss&quot; rel=&quot;self&quot; /&gt;
    ///     &lt;lastBuildDate&gt;Wed, 30 Dec 2015 13:39:16 +0300&lt;/lastBuildDate&gt;
    ///     &lt;item&gt;
    ///       &lt;title&gt;Hello, world!&lt;/title&gt;
    ///       &lt;link&gt;http://example.org/posts/hello.html&lt;/link&gt;
    ///       &lt;description&gt;My first blog post&lt;/description&gt;
    ///       &lt;pubDate&gt;Sat, 10 Nov 2012 00:00:00 +0400&lt;/pubDate&gt;
    ///       &lt;guid isPermaLink=&quot;false&quot;&gt; 81a26807-355a-dbf9-7729-c6601f1d8a2b&lt;/guid&gt;
    ///     &lt;/item&gt;
    ///   &lt;/channel&gt;
    /// &lt;/rss&gt;
    /// </code>
    /// </example>
    /// <metadata name="RelativeFilePath" type="FilePath">Relative path to the output RSS file.</metadata>
    /// <metadata name="WritePath" type="FilePath">Relative path to the output RSS file (primarily to support the WriteFiles module).</metadata>
    /// <category>Content</category>
    public class Rss : IModule
    {
        /// <summary>
        /// RSS feed title. Required.
        /// </summary>
        private readonly string _feedTitle = string.Empty;

        /// <summary>
        /// RSS feed description. Required.
        /// </summary>
        private readonly string _feedDescription = string.Empty;

        private readonly Uri _siteUri;

        private readonly FilePath _rssPath;

        // Default meta keys
        private string _pubDateMetaKey = "RssPubDate";

        private string _titleMetaKey = "RssTitle";

        private string _guidMetaKey = "RssGuid";

        private string _descriptionMetaKey = "RssDescription";

        // Options
        private bool _appendGuid = true;

        private string _language = null;

        private bool _assumePermalinks = false;

        private Func<FilePath, FilePath> _linkCustomizerDelegate = null;

        private static readonly XNamespace AtomNamespace = XNamespace.Get(@"http://www.w3.org/2005/Atom");

        /// <summary>
        /// Creates RSS feed from input documents.
        /// </summary>
        /// <param name="siteUri">The full URI of the site, including hostname (example: "http://mysite.com/blog/").</param>
        /// <param name="rssPath">Relative output location where generated RSS feed will be placed on server. (example: "feeds/feed.rss").</param>
        /// <param name="feedTitle">Title of the RSS feed.</param>
        /// <param name="feedDescription">Description of the RSS feed.</param>
        public Rss(Uri siteUri, FilePath rssPath, string feedTitle, string feedDescription)
        {
            if (siteUri == null)
            {
                throw new ArgumentNullException(nameof(siteUri));
            }
            if (!siteUri.IsAbsoluteUri)
            {
                throw new ArgumentException("The site URI must be absolute", nameof(siteUri));
            }
            if (rssPath == null)
            {
                throw new ArgumentNullException(nameof(rssPath));
            }
            if (!rssPath.IsRelative)
            {
                throw new ArgumentException("The RSS path must be relative", nameof(rssPath));
            }
            if (string.IsNullOrEmpty(feedTitle))
            {
                throw new ArgumentException(nameof(feedTitle));
            }
            if (string.IsNullOrEmpty(feedDescription))
            {
                throw new ArgumentException(nameof(feedDescription));
            }

            _siteUri = siteUri;
            _rssPath = rssPath;
            _feedDescription = feedDescription;
            _feedTitle = feedTitle;
        }

        /// <summary>
        /// Creates RSS feed from input documents.
        /// </summary>
        /// <param name="siteUri">The full URI of the site, including hostname (example: "http://mysite.com/blog/").</param>
        /// <param name="rssPath">Relative output location where generated RSS feed will be placed on server. (example: "feeds/feed.rss").</param>
        /// <param name="feedTitle">Title of the RSS feed.</param>
        /// <param name="feedDescription">Description of the RSS feed.</param>
        public Rss(string siteUri, FilePath rssPath, string feedTitle, string feedDescription)
            : this(new Uri(siteUri), rssPath, feedTitle, feedDescription)
        {
        }

        /// <summary>
        /// Set metadata key to lookup in documents for RSS item publication date.
        /// </summary>
        public Rss WithPublicationDateMetaKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException(nameof(key));
            }

            _pubDateMetaKey = key;
            return this;
        }

        /// <summary>
        /// Set metadata key to lookup in documents for RSS item title.
        /// </summary>
        public Rss WithTitleMetaKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException(nameof(key));
            }

            _titleMetaKey = key;
            return this;
        }

        /// <summary>
        /// Set metadata key to lookup in documents for RSS item Guid.
        /// </summary>
        public Rss WithGuidMetaKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException(nameof(key));
            }

            _guidMetaKey = key;
            return this;
        }

        /// <summary>
        /// Set metadata key to lookup in documents for RSS item description.
        /// </summary>
        public Rss WithDescriptionMetaKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException(nameof(key));
            }

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
            {
                throw new ArgumentException(nameof(lang));
            }

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
        /// <param name="customizer">A delegate that takes and returns a <see cref="FilePath"/>.</param>
        /// <returns></returns>
        public Rss WithLinkCustomizer(Func<FilePath, FilePath> customizer)
        {
            if (customizer == null)
            {
                throw new ArgumentNullException(nameof(customizer));
            }

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
            FilePath rssPath = new DirectoryPath(_siteUri.ToString()).CombineFile(_rssPath);

            XElement rssRoot = new XElement("rss",
                new XAttribute("version", "2.0"),
                new XAttribute(XNamespace.Xmlns + "atom", AtomNamespace)
            );

            XElement channel = new XElement("channel",
                new XElement("title", _feedTitle),
                new XElement("description", _feedDescription),
                new XElement("link", _siteUri.ToString()),
                new XElement(AtomNamespace + "link",
                    new XAttribute("href", rssPath.FullPath),
                    new XAttribute("rel", "self")
                ),
                new XElement("lastBuildDate", DateTimeHelper.ToRssDate(DateTime.Now))
            );

            if (_language != null)
            {
                channel.Add(new XElement("language", _language));
            }

            foreach (IDocument input in inputs)
            {
                XElement item = new XElement("item");

                object title;
                bool hasTitle = input.TryGetValue(_titleMetaKey, out title);
                item.Add(new XElement("title", hasTitle ? title : "Untitled"));
                
                // Get the item link
                FilePath link = input.FilePath(Keys.RssItemUrl) 
                    ?? input.FilePath(Keys.RelativeFilePath);
                if(link != null)
                {
                    if (link.IsAbsolute)
                    {
                        link = new DirectoryPath("/").GetRelativePath(link);
                    }
                    if (_linkCustomizerDelegate != null)
                    {
                        link = _linkCustomizerDelegate(link);

                        // Check for absolute one more time in case the customizer returned an absolute path
                        if (link.IsAbsolute)
                        {
                            link = new DirectoryPath("/").GetRelativePath(link);
                        }
                    }
                }
                if (link == null)
                {
                    throw new Exception($"Required metadata keys {Keys.RssItemUrl} or {Keys.RelativeFilePath} were not found in the document {input.SourceString()}");
                }
            
                FilePath itemLink = new DirectoryPath(_siteUri.ToString()).CombineFile(link);
                item.Add(new XElement("link", itemLink.FullPath));

                object description = null;
                bool hasDescription = input.TryGetValue(_descriptionMetaKey, out description);
                item.Add(new XElement("description", hasDescription ? description : "No description"));

                object pubDate = null;
                if (input.TryGetValue(_pubDateMetaKey, out pubDate))
                {
                    item.Add(new XElement("pubDate", DateTimeHelper.ToRssDate(DateTime.Parse((string)pubDate))));
                }

                if (_appendGuid)
                {
                    object guid;
                    bool hasGuid = input.TryGetValue(_guidMetaKey, out guid);
                       
                    if (hasGuid)
                    {
                        item.Add(new XElement("guid", guid, new XAttribute("isPermaLink", false)));
                    }
                    else
                    {
                        if (_assumePermalinks)
                        {
                            item.Add(new XElement("guid", link));
                        }
                        else if (input.Source == null)
                        {
                            Trace.Warning($"Cannot generate RSS item guid for document {input.SourceString()} because document source is not valid");
                        }
                        else
                        {
                            item.Add(new XElement("guid", GuidHelper.ToGuid(input.Source.FullPath).ToString(), new XAttribute("isPermaLink", false)));
                        }
                    }
                }

                channel.Add(item);   
            }

            rss.Add(rssRoot);
            rssRoot.Add(channel);

            // rss.ToString() doesn't return XML declaration
            StringBuilder outText = new StringBuilder();
            using (UTF8StringWriter stream = new UTF8StringWriter(outText))
            {
                rss.Save(stream);
            }

            return new IDocument[] { context.GetDocument(outText.ToString(), new KeyValuePair<string, object>[] {
                new KeyValuePair<string, object>(Keys.IsRssFeed, true),
                new KeyValuePair<string, object>(Keys.RelativeFilePath, _rssPath),
                new KeyValuePair<string, object>(Keys.WritePath, _rssPath)
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

    internal class DateTimeHelper
    {
        private static CultureInfo rssDateCulture = new CultureInfo("en");

        public static string ToRssDate(DateTime date)
        {
            var value = date.ToString("ddd',' d MMM yyyy HH':'mm':'ss", rssDateCulture)
                + " " + date.ToString("zzzz", rssDateCulture).Replace(":", "");
            return value;
        }
    }
}
