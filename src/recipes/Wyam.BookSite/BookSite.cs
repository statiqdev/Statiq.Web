using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Util;
using Wyam.Core.Modules.Control;
using Wyam.Core.Modules.Metadata;
using Wyam.Feeds;
using Wyam.Web.Pipelines;

namespace Wyam.BookSite
{
    /// <summary>
    /// A recipe for creating book and ebook marketing sites.
    /// </summary>
    /// <metadata cref="BookSiteKeys.Title" usage="Setting">The title of the book.</metadata>
    /// <metadata cref="BookSiteKeys.Title" usage="Input">The title of the post, chapter, or page.</metadata>
    /// <metadata cref="BookSiteKeys.Subtitle" usage="Setting" />
    /// <metadata cref="BookSiteKeys.Description" usage="Setting" />
    /// <metadata cref="BookSiteKeys.Description" usage="Input">Used with chapters to provide an optional description of the chapter.</metadata>
    /// <metadata cref="BookSiteKeys.Image" usage="Setting" />
    /// <metadata cref="BookSiteKeys.BookImage" usage="Setting" />
    /// <metadata cref="BookSiteKeys.BookLink" usage="Setting" />
    /// <metadata cref="BookSiteKeys.BookLinkText" usage="Setting" />
    /// <metadata cref="BookSiteKeys.BlogPath" usage="Setting" />
    /// <metadata cref="BookSiteKeys.ChaptersPath" usage="Setting" />
    /// <metadata cref="BookSiteKeys.ChaptersIntro" usage="Setting" />
    /// <metadata cref="BookSiteKeys.IgnoreFolders" usage="Setting" />
    /// <metadata cref="BookSiteKeys.SectionsPath" usage="Setting" />
    /// <metadata cref="BookSiteKeys.CaseInsensitiveTags" usage="Setting" />
    /// <metadata cref="BookSiteKeys.MarkdownConfiguration" usage="Setting" />
    /// <metadata cref="BookSiteKeys.MarkdownExtensionTypes" usage="Setting" />
    /// <metadata cref="BookSiteKeys.IncludeDateInPostPath" usage="Setting" />
    /// <metadata cref="BookSiteKeys.MetaRefreshRedirects" usage="Setting" />
    /// <metadata cref="BookSiteKeys.NetlifyRedirects" usage="Setting" />
    /// <metadata cref="BookSiteKeys.BlogRssPath" usage="Setting" />
    /// <metadata cref="BookSiteKeys.BlogAtomPath" usage="Setting" />
    /// <metadata cref="BookSiteKeys.BlogRdfPath" usage="Setting" />
    /// <metadata cref="BookSiteKeys.ValidateAbsoluteLinks" usage="Setting" />
    /// <metadata cref="BookSiteKeys.ValidateRelativeLinks" usage="Setting" />
    /// <metadata cref="BookSiteKeys.ValidateLinksAsError" usage="Setting" />
    /// <metadata cref="BookSiteKeys.BlogPageSize" usage="Setting" />
    /// <metadata cref="BookSiteKeys.TagPageSize" usage="Setting" />
    /// <metadata cref="BookSiteKeys.ChapterNumber" usage="Input" />
    /// <metadata cref="BookSiteKeys.Published" usage="Input" />
    /// <metadata cref="BookSiteKeys.Tags" usage="Input" />
    /// <metadata cref="BookSiteKeys.ShowInNavbar" usage="Input" />
    /// <metadata cref="BookSiteKeys.Order" usage="Input" />
    public class BookSite : Recipe
    {
        /// <summary>
        /// Reads chapter files. These files can be either Markdown or Razor format
        /// and should be placed, one per chapter, in the folder specified by
        /// <see cref="BookSiteKeys.ChaptersPath"/>.
        /// </summary>
        [SourceInfo]
        public static Pages Chapters { get; } = new Pages(
            nameof(Chapters),
            ctx => ctx.DirectoryPath(BookSiteKeys.ChaptersPath).FullPath,
            null,
            ctx => ctx.String(BookSiteKeys.MarkdownConfiguration),
            ctx => ctx.List<Type>(BookSiteKeys.MarkdownExtensionTypes),
            (x, y) => Comparer.Default.Compare(x.Get<int>(BookSiteKeys.ChapterNumber), y.Get<int>(BookSiteKeys.ChapterNumber)),
            false,
            null)
                .InsertBefore(
                    Pages.CreateTreeAndSort,
                    new Where((doc, ctx) => doc.Get<int>(BookSiteKeys.ChapterNumber) > 0));

        /// <inheritdoc cref="BlogPosts" />
        [SourceInfo]
        public static BlogPosts BlogPosts { get; } = new BlogPosts(
            nameof(BlogPosts),
            BookSiteKeys.Published,
            ctx => ctx.String(BookSiteKeys.MarkdownConfiguration),
            ctx => ctx.List<Type>(BookSiteKeys.MarkdownExtensionTypes),
            ctx => ctx.Bool(BookSiteKeys.IncludeDateInPostPath),
            ctx => ctx.DirectoryPath(BookSiteKeys.BlogPath).FullPath);

        /// <inheritdoc cref="Pages" />
        [SourceInfo]
        public static Pages Pages { get; } = new Pages(
            nameof(Pages),
            null,
            ctx => new[]
                {
                    ctx.DirectoryPath(BookSiteKeys.BlogPath).FullPath,
                    ctx.DirectoryPath(BookSiteKeys.ChaptersPath).FullPath,
                    ctx.DirectoryPath(BookSiteKeys.SectionsPath).FullPath
                }
                .Concat(ctx.List(BookSiteKeys.IgnoreFolders, Array.Empty<string>())),
            ctx => ctx.String(BookSiteKeys.MarkdownConfiguration),
            ctx => ctx.List<Type>(BookSiteKeys.MarkdownExtensionTypes),
            (x, y) =>
            {
                int order = Comparer.Default.Compare(x.String(BookSiteKeys.Order), y.String(BookSiteKeys.Order));
                return order == 0 ? Comparer.Default.Compare(x.String(Keys.Title), y.String(Keys.Title)) : order;
            },
            true,
            TreePlaceholderFactory);

        /// <summary>
        /// Reads sections for the homepage. These files can be either Markdown or Razor format
        /// and should be placed, one per section, in the folder specified by
        /// <see cref="BookSiteKeys.SectionsPath"/>.
        /// </summary>
        [SourceInfo]
        public static Pages Sections { get; } = new Pages(
            nameof(Sections),
            ctx => ctx.DirectoryPath(BookSiteKeys.SectionsPath).FullPath,
            null,
            ctx => ctx.String(BookSiteKeys.MarkdownConfiguration),
            ctx => ctx.List<Type>(BookSiteKeys.MarkdownExtensionTypes),
            (x, y) =>
            {
                int order = Comparer.Default.Compare(x.String(BookSiteKeys.Order), y.String(BookSiteKeys.Order));
                return order == 0 ? Comparer.Default.Compare(x.String(Keys.Title), y.String(Keys.Title)) : order;
            },
            false,
            null);

        /// <summary>
        /// Generates the index pages for blog posts.
        /// </summary>
        [SourceInfo]
        public static Archive BlogIndexes { get; } = new Archive(
            nameof(BlogIndexes),
            new string[] { BlogPosts },
            "_BlogIndex.cshtml",
            "/_Layout.cshtml",
            null,
            null,
            ctx => ctx.Get(BookSiteKeys.BlogPageSize, int.MaxValue),
            null,
            (doc, ctx) => "Blog",
            (doc, ctx) => $"{ctx.DirectoryPath(BookSiteKeys.BlogPath).FullPath}",
            null,
            null);

        /// <summary>
        /// Generates the tag pages for blog posts.
        /// </summary>
        [SourceInfo]
        public static Archive BlogTags { get; } = new Archive(
            nameof(BlogTags),
            new string[] { BlogPosts },
            "_BlogIndex.cshtml",
            "/_Layout.cshtml",
            (doc, ctx) => doc.List<string>(BookSiteKeys.Tags),
            ctx => ctx.Bool(BookSiteKeys.CaseInsensitiveTags),
            ctx => ctx.Get(BookSiteKeys.TagPageSize, int.MaxValue),
            null,
            (doc, ctx) => doc.String(Keys.GroupKey),
            (doc, ctx) => $"{ctx.DirectoryPath(BookSiteKeys.BlogPath).FullPath}/tag/{doc.String(Keys.GroupKey)}",
            null,
            null);

        /// <inheritdoc cref="Web.Pipelines.Feeds" />
        [SourceInfo]
        public static Web.Pipelines.Feeds BlogFeed { get; } = new Web.Pipelines.Feeds(
            nameof(BlogFeed),
            new string[] { BlogPosts },
            ctx => ctx.FilePath(BookSiteKeys.BlogRssPath),
            ctx => ctx.FilePath(BookSiteKeys.BlogAtomPath),
            ctx => ctx.FilePath(BookSiteKeys.BlogRdfPath));

        /// <inheritdoc cref="RenderPages" />
        [SourceInfo]
        public static RenderPages RenderPages { get; } = new RenderPages(
            nameof(RenderPages),
            new string[] { Pages },
            (doc, ctx) => "/_Layout.cshtml",
            null);

        /// <inheritdoc cref="RenderBlogPosts" />
        [SourceInfo]
        public static RenderBlogPosts RenderBlogPosts { get; } = new RenderBlogPosts(
            nameof(RenderBlogPosts),
            new string[] { BlogPosts },
            BookSiteKeys.Published,
            (doc, ctx) => "/_BlogPost.cshtml");

        /// <inheritdoc cref="Web.Pipelines.Redirects" />
        [SourceInfo]
        public static Redirects Redirects { get; } = new Redirects(
            nameof(Redirects),
            new string[] { RenderPages, RenderBlogPosts },
            ctx => ctx.Bool(BookSiteKeys.MetaRefreshRedirects),
            ctx => ctx.Bool(BookSiteKeys.NetlifyRedirects));

        /// <inheritdoc cref="Web.Pipelines.Resources" />
        [SourceInfo]
        public static Resources Resources { get; } = new Resources(nameof(Resources));

        /// <inheritdoc cref="Web.Pipelines.ValidateLinks" />
        [SourceInfo]
        public static ValidateLinks ValidateLinks { get; } = new ValidateLinks(
            nameof(ValidateLinks),
            new string[] { RenderPages, RenderBlogPosts, Resources },
            ctx => ctx.Bool(BookSiteKeys.ValidateAbsoluteLinks),
            ctx => ctx.Bool(BookSiteKeys.ValidateRelativeLinks),
            ctx => ctx.Bool(BookSiteKeys.ValidateLinksAsError));

        /// <inheritdoc />
        public override void Apply(IEngine engine)
        {
            // Global metadata defaults
            engine.Settings[BookSiteKeys.Title] = "My Book";
            engine.Settings[BookSiteKeys.Description] = "The best book you'll ever read.";
            engine.Settings[BookSiteKeys.BookImage] = "/images/book.png";
            engine.Settings[BookSiteKeys.BookLinkText] = "Order Now";
            engine.Settings[BookSiteKeys.MarkdownConfiguration] = "advanced+bootstrap";
            engine.Settings[BookSiteKeys.IncludeDateInPostPath] = false;
            engine.Settings[BookSiteKeys.BlogPath] = "blog";
            engine.Settings[BookSiteKeys.ChaptersPath] = "chapters";
            engine.Settings[BookSiteKeys.SectionsPath] = "sections";
            engine.Settings[BookSiteKeys.BlogPageSize] = 5;
            engine.Settings[BookSiteKeys.TagPageSize] = 5;
            engine.Settings[BookSiteKeys.MetaRefreshRedirects] = true;
            engine.Settings[BookSiteKeys.BlogRssPath] = GenerateFeeds.DefaultRssPath;
            engine.Settings[BookSiteKeys.BlogAtomPath] = GenerateFeeds.DefaultAtomPath;
            engine.Settings[BookSiteKeys.BlogRdfPath] = GenerateFeeds.DefaultRdfPath;

            base.Apply(engine);
        }

        /// <inheritdoc />
        public override void Scaffold(IFile configFile, IDirectory inputDirectory)
        {
            // Config file
            configFile?.WriteAllText(@"#recipe BookSite");

            // TODO
        }

        private static IDocument TreePlaceholderFactory(object[] path, MetadataItems items, IExecutionContext context)
        {
            FilePath indexPath = new FilePath(string.Join("/", path.Concat(new[] { "index.html" })));
            items.Add(Keys.RelativeFilePath, indexPath);
            items.Add(Keys.Title, Title.GetTitle(indexPath));
            return context.GetDocument(context.GetContentStream("@Html.Partial(\"_ChildPages\")"), items);
        }
    }
}
