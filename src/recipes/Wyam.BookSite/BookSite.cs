using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.BookSite.Pipelines;
using Wyam.Common.Configuration;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Util;
using Wyam.Feeds;

namespace Wyam.BookSite
{
    /// <summary>
    /// A recipe for creating book and ebook marketing sites.
    /// </summary>
    /// <metadata cref="BookSiteKeys.Title" usage="Setting">The title of the blog.</metadata>
    /// <metadata cref="BookSiteKeys.Title" usage="Input">The title of the post or page.</metadata>
    /// <metadata cref="BookSiteKeys.Image" usage="Setting">The relative path to an image to display on the home page.</metadata>
    /// <metadata cref="BookSiteKeys.Image" usage="Input">The relative path to an image for the current post or page (often shown in the header of the page).</metadata>
    /// <metadata cref="BookSiteKeys.Description" usage="Setting" />
    /// <metadata cref="BookSiteKeys.Intro" usage="Setting" />
    /// <metadata cref="BookSiteKeys.PostsPath" usage="Setting" />
    /// <metadata cref="BookSiteKeys.CaseInsensitiveTags" usage="Setting" />
    /// <metadata cref="BookSiteKeys.MarkdownExtensions" usage="Setting" />
    /// <metadata cref="BookSiteKeys.MarkdownExternalExtensions" usage="Setting" />
    /// <metadata cref="BookSiteKeys.IncludeDateInPostPath" usage="Setting" />
    /// <metadata cref="BookSiteKeys.MetaRefreshRedirects" usage="Setting" />
    /// <metadata cref="BookSiteKeys.NetlifyRedirects" usage="Setting" />
    /// <metadata cref="BookSiteKeys.RssPath" usage="Setting" />
    /// <metadata cref="BookSiteKeys.AtomPath" usage="Setting" />
    /// <metadata cref="BookSiteKeys.RdfPath" usage="Setting" />
    /// <metadata cref="BookSiteKeys.ValidateAbsoluteLinks" usage="Setting" />
    /// <metadata cref="BookSiteKeys.ValidateRelativeLinks" usage="Setting" />
    /// <metadata cref="BookSiteKeys.ValidateLinksAsError" usage="Setting" />
    /// <metadata cref="BookSiteKeys.TagPageSize" usage="Setting" />
    /// <metadata cref="BookSiteKeys.Published" usage="Input" />
    /// <metadata cref="BookSiteKeys.Tags" usage="Input" />
    /// <metadata cref="BookSiteKeys.Lead" usage="Input" />
    /// <metadata cref="BookSiteKeys.Excerpt" usage="Output" />
    /// <metadata cref="BookSiteKeys.ShowInNavbar" usage="Input" />
    /// <metadata cref="BookSiteKeys.Content" usage="Output" />
    /// <metadata cref="BookSiteKeys.Posts" usage="Output" />
    /// <metadata cref="BookSiteKeys.Tag" usage="Output" />
    public class BookSite : Recipe
    {
        /// <inheritdoc cref="Pipelines.Pages" />
        [SourceInfo]
        public static Pages Pages { get; } = new Pages();

        /// <inheritdoc cref="Pipelines.RawPosts" />
        [SourceInfo]
        public static RawPosts RawPosts { get; } = new RawPosts();

        /// <inheritdoc cref="Pipelines.Tags" />
        [SourceInfo]
        public static Tags Tags { get; } = new Tags();

        /// <inheritdoc cref="Pipelines.Posts" />
        [SourceInfo]
        public static Posts Posts { get; } = new Posts();

        /// <inheritdoc cref="Pipelines.Feed" />
        [SourceInfo]
        public static Feed Feed { get; } = new Feed();

        /// <inheritdoc cref="Pipelines.RenderPages" />
        [SourceInfo]
        public static RenderPages RenderPages { get; } = new RenderPages();

        /// <inheritdoc cref="Pipelines.Redirects" />
        [SourceInfo]
        public static Redirects Redirects { get; } = new Redirects();

        /// <inheritdoc cref="Pipelines.Resources" />
        [SourceInfo]
        public static Resources Resources { get; } = new Resources();

        /// <inheritdoc cref="Pipelines.ValidateLinks" />
        [SourceInfo]
        public static ValidateLinks ValidateLinks { get; } = new ValidateLinks();

        /// <inheritdoc />
        public override void Apply(IEngine engine)
        {
            // Global metadata defaults
            engine.Settings[BookSiteKeys.Title] = "My Blog";
            engine.Settings[BookSiteKeys.Description] = "Welcome!";
            engine.Settings[BookSiteKeys.MarkdownExtensions] = "advanced+bootstrap";
            engine.Settings[BookSiteKeys.IncludeDateInPostPath] = false;
            engine.Settings[BookSiteKeys.PostsPath] = new DirectoryPath("posts");
            engine.Settings[BookSiteKeys.MetaRefreshRedirects] = true;
            engine.Settings[BookSiteKeys.RssPath] = GenerateFeeds.DefaultRssPath;
            engine.Settings[BookSiteKeys.AtomPath] = GenerateFeeds.DefaultAtomPath;
            engine.Settings[BookSiteKeys.RdfPath] = GenerateFeeds.DefaultRdfPath;

            base.Apply(engine);
        }

        /// <inheritdoc />
        public override void Scaffold(IFile configFile, IDirectory inputDirectory)
        {
            // Config file
            configFile?.WriteAllText(@"#recipe BookSite");

            // TODO
        }
    }
}
