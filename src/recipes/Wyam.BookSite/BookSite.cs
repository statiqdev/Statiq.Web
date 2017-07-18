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
    /// <metadata cref="BookSiteKeys.ProcessIncludes" usage="Setting" />
    /// <metadata cref="BookSiteKeys.ProcessIncludes" usage="Input" />
    /// <metadata cref="BookSiteKeys.Subtitle" usage="Setting" />
    /// <metadata cref="BookSiteKeys.Description" usage="Setting" />
    /// <metadata cref="BookSiteKeys.Description" usage="Input">Used with chapters to provide an optional description of the chapter.</metadata>
    /// <metadata cref="BookSiteKeys.Image" usage="Setting" />
    /// <metadata cref="BookSiteKeys.Image" usage="Input">An image to display on the blog or chapter page.</metadata>
    /// <metadata cref="BookSiteKeys.BookImage" usage="Setting" />
    /// <metadata cref="BookSiteKeys.BookLink" usage="Setting" />
    /// <metadata cref="BookSiteKeys.BookLinkText" usage="Setting" />
    /// <metadata cref="BookSiteKeys.BlogPath" usage="Setting" />
    /// <metadata cref="BookSiteKeys.ChaptersPath" usage="Setting" />
    /// <metadata cref="BookSiteKeys.ChaptersIntro" usage="Setting" />
    /// <metadata cref="BookSiteKeys.IgnoreFolders" usage="Setting" />
    /// <metadata cref="BookSiteKeys.SectionsPath" usage="Setting" />
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
    /// <metadata cref="BookSiteKeys.ChapterNumber" usage="Input" />
    /// <metadata cref="BookSiteKeys.Published" usage="Input" />
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
            new PagesSettings
            {
                PagesPattern = ctx => ctx.DirectoryPath(BookSiteKeys.ChaptersPath).FullPath,
                MarkdownConfiguration = ctx => ctx.String(BookSiteKeys.MarkdownConfiguration),
                MarkdownExtensionTypes = ctx => ctx.List<Type>(BookSiteKeys.MarkdownExtensionTypes),
                ProcessIncludes = (doc, ctx) => doc.Bool(BookSiteKeys.ProcessIncludes),
                Sort = (x, y) => Comparer.Default.Compare(x.Get<int>(BookSiteKeys.ChapterNumber), y.Get<int>(BookSiteKeys.ChapterNumber))
            })
                .InsertBefore(
                    Pages.CreateTreeAndSort,
                    new Where((doc, ctx) => doc.Get<int>(BookSiteKeys.ChapterNumber) > 0));

        /// <inheritdoc cref="Web.Pipelines.BlogPosts" />
        [SourceInfo]
        public static BlogPosts BlogPosts { get; } = new BlogPosts(
            nameof(BlogPosts),
            new BlogPostsSettings
            {
                PublishedKey = BookSiteKeys.Published,
                MarkdownConfiguration = ctx => ctx.String(BookSiteKeys.MarkdownConfiguration),
                MarkdownExtensionTypes = ctx => ctx.List<Type>(BookSiteKeys.MarkdownExtensionTypes),
                ProcessIncludes = (doc, ctx) => doc.Bool(BookSiteKeys.ProcessIncludes),
                IncludeDateInPostPath = ctx => ctx.Bool(BookSiteKeys.IncludeDateInPostPath),
                PostsPath = ctx => ctx.DirectoryPath(BookSiteKeys.BlogPath).FullPath
            });

        /// <inheritdoc cref="Web.Pipelines.Pages" />
        [SourceInfo]
        public static Pages Pages { get; } = new Pages(
            nameof(Pages),
            new PagesSettings
            {
                IgnorePaths = ctx => new[]
                {
                    ctx.DirectoryPath(BookSiteKeys.BlogPath).FullPath,
                    ctx.DirectoryPath(BookSiteKeys.ChaptersPath).FullPath,
                    ctx.DirectoryPath(BookSiteKeys.SectionsPath).FullPath
                }
                    .Concat(ctx.List(BookSiteKeys.IgnoreFolders, Array.Empty<string>())),
                MarkdownConfiguration = ctx => ctx.String(BookSiteKeys.MarkdownConfiguration),
                MarkdownExtensionTypes = ctx => ctx.List<Type>(BookSiteKeys.MarkdownExtensionTypes),
                ProcessIncludes = (doc, ctx) => doc.Bool(BookSiteKeys.ProcessIncludes),
                Sort = (x, y) =>
                {
                    int order = Comparer.Default.Compare(x.String(BookSiteKeys.Order), y.String(BookSiteKeys.Order));
                    return order == 0 ? Comparer.Default.Compare(x.String(Keys.Title), y.String(Keys.Title)) : order;
                },
                CreateTree = true,
                TreePlaceholderFactory = TreePlaceholderFactory
            });

        /// <summary>
        /// Reads sections for the homepage. These files can be either Markdown or Razor format
        /// and should be placed, one per section, in the folder specified by
        /// <see cref="BookSiteKeys.SectionsPath"/>.
        /// </summary>
        [SourceInfo]
        public static Pages Sections { get; } = new Pages(
            nameof(Sections),
            new PagesSettings
            {
                PagesPattern = ctx => ctx.DirectoryPath(BookSiteKeys.SectionsPath).FullPath,
                MarkdownConfiguration = ctx => ctx.String(BookSiteKeys.MarkdownConfiguration),
                MarkdownExtensionTypes = ctx => ctx.List<Type>(BookSiteKeys.MarkdownExtensionTypes),
                ProcessIncludes = (doc, ctx) => doc.Bool(BookSiteKeys.ProcessIncludes),
                Sort = (x, y) =>
                {
                    int order = Comparer.Default.Compare(x.String(BookSiteKeys.Order), y.String(BookSiteKeys.Order));
                    return order == 0 ? Comparer.Default.Compare(x.String(Keys.Title), y.String(Keys.Title)) : order;
                }
            });

        /// <summary>
        /// Generates the index pages for blog posts.
        /// </summary>
        [SourceInfo]
        public static Archive BlogIndexes { get; } = new Archive(
            nameof(BlogIndexes),
            new ArchiveSettings
            {
                Pipelines = new string[] { BlogPosts },
                TemplateFile = ctx => "_BlogIndex.cshtml",
                Layout = "/_Layout.cshtml",
                PageSize = ctx => ctx.Get(BookSiteKeys.BlogPageSize, int.MaxValue),
                Title = (doc, ctx) => "Blog",
                RelativePath = (doc, ctx) => $"{ctx.DirectoryPath(BookSiteKeys.BlogPath).FullPath}"
            });

        /// <summary>
        /// Generates the chapter index.
        /// </summary>
        [SourceInfo]
        public static Archive ChapterIndex { get; } = new Archive(
            nameof(ChapterIndex),
            new ArchiveSettings
            {
                Pipelines = new string[] { Chapters },
                TemplateFile = ctx => "_ChapterIndex.cshtml",
                Layout = "/_Layout.cshtml",
                Title = (doc, ctx) => "Chapters",
                RelativePath = (doc, ctx) => $"{ctx.DirectoryPath(BookSiteKeys.ChaptersPath).FullPath}"
            });

        /// <inheritdoc cref="Web.Pipelines.Feeds" />
        [SourceInfo]
        public static Web.Pipelines.Feeds BlogFeed { get; } = new Web.Pipelines.Feeds(
            nameof(BlogFeed),
            new FeedsSettings
            {
                Pipelines = new string[] { BlogPosts },
                RssPath = ctx => ctx.FilePath(BookSiteKeys.BlogRssPath),
                AtomPath = ctx => ctx.FilePath(BookSiteKeys.BlogAtomPath),
                RdfPath = ctx => ctx.FilePath(BookSiteKeys.BlogRdfPath)
            });

        /// <inheritdoc cref="Web.Pipelines.RenderPages" />
        [SourceInfo]
        public static RenderPages RenderPages { get; } = new RenderPages(
            nameof(RenderPages),
            new RenderPagesSettings
            {
                Pipelines = new string[] { Pages },
                Layout = (doc, ctx) => "/_Layout.cshtml"
            });

        /// <summary>
        /// Renders the chapter pages if they have content.
        /// </summary>
        [SourceInfo]
        public static RenderPages RenderChapters { get; } = new RenderPages(
            nameof(RenderChapters),
            new RenderPagesSettings
            {
                Pipelines = new string[] { Chapters },
                Layout = (doc, ctx) => "/_Chapter.cshtml",
                Sort = (x, y) => Comparer.Default.Compare(x.Get<int>(BookSiteKeys.ChapterNumber), y.Get<int>(BookSiteKeys.ChapterNumber))
            });

        /// <inheritdoc cref="Web.Pipelines.RenderBlogPosts" />
        [SourceInfo]
        public static RenderBlogPosts RenderBlogPosts { get; } = new RenderBlogPosts(
            nameof(RenderBlogPosts),
            new RenderBlogPostsSettings
            {
                Pipelines = new string[] { BlogPosts },
                PublishedKey = BookSiteKeys.Published,
                Layout = (doc, ctx) => "/_BlogPost.cshtml"
            });

        /// <inheritdoc cref="Web.Pipelines.Redirects" />
        [SourceInfo]
        public static Redirects Redirects { get; } = new Redirects(
            nameof(Redirects),
            new RedirectsSettings
            {
                Pipelines = new string[] { RenderPages, RenderBlogPosts },
                MetaRefreshRedirects = ctx => ctx.Bool(BookSiteKeys.MetaRefreshRedirects),
                NetlifyRedirects = ctx => ctx.Bool(BookSiteKeys.NetlifyRedirects)
            });

        /// <inheritdoc cref="Web.Pipelines.Less" />
        [SourceInfo]
        public static Web.Pipelines.Less Less { get; } = new Web.Pipelines.Less(nameof(Less));

        /// <inheritdoc cref="Web.Pipelines.Sass" />
        [SourceInfo]
        public static Web.Pipelines.Sass Sass { get; } = new Web.Pipelines.Sass(nameof(Sass));

        /// <inheritdoc cref="Web.Pipelines.Resources" />
        [SourceInfo]
        public static Resources Resources { get; } = new Resources(nameof(Resources));

        /// <inheritdoc cref="Web.Pipelines.ValidateLinks" />
        [SourceInfo]
        public static ValidateLinks ValidateLinks { get; } = new ValidateLinks(
            nameof(ValidateLinks),
            new ValidateLinksSettings
            {
                Pipelines = new string[] { RenderPages, RenderBlogPosts, Resources },
                ValidateAbsoluteLinks = ctx => ctx.Bool(BookSiteKeys.ValidateAbsoluteLinks),
                ValidateRelativeLinks = ctx => ctx.Bool(BookSiteKeys.ValidateRelativeLinks),
                ValidateLinksAsError = ctx => ctx.Bool(BookSiteKeys.ValidateLinksAsError)
            });

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
            configFile?.WriteAllText(@"#recipe BookSite

// Customize your settings and add new ones here
Settings[Keys.Host] = ""host.com"";
Settings[BookSiteKeys.Title] = ""Your Book Title"";
Settings[BookSiteKeys.Subtitle] = ""The Subtitle Of Your Book."";
Settings[BookSiteKeys.Description] = ""A short description of your book."";
Settings[BookSiteKeys.BookLink] = ""https://a/link/to/your/book"";
Settings[BookSiteKeys.BookImage] = ""/images/book.jpg"";  // Add an image at this path to your input folder
Settings[BookSiteKeys.Image] = ""/images/banner.jpg"";  // Add an image at this path to your input folder
Settings[BookSiteKeys.ChaptersIntro] = ""A short introduction to the content of your book."";

// Add any pipeline customizations here");

            // Chapter
            inputDirectory.GetFile("chapters/first-chapter.md").WriteAllText(
                @"Title: First Chapter
Description: An optional description of this chapter.
ChapterNumber: 1
---
An optional longer description of your chapter.");

            // Blog Post
            inputDirectory.GetFile("blog/first-post.md").WriteAllText(
                @"Title: First Post
Published: 1/1/2016
---
This is a new blog post.");

            // Home sections
            inputDirectory.GetFile("sections/endorsements.cshtml").WriteAllText(@"Title: What Readers Are Saying
Order: 1
---
<div class=""row 150%"">
    <div class=""4u 12u(mobile)"">
        <section class=""highlight"">
            <p>A newspaper quote.</p>
            <h3><a href=""#"">Daily Newspaper</a></h3>
        </section>
    </div>
    <div class=""4u 12u(mobile)"">
        <section class=""highlight"">
            <p>Quote from a reviewer.</p>
            <h3><a href=""#"">Reviewer on GoodReads</a></h3>
        </section>
    </div>
    <div class=""4u 12u(mobile)"">
        <section class=""highlight"">
            <p>I loved this book. It was incredible.</p>
            <h3><a href=""#"">Internet Person</a></h3>
        </section>
    </div>
</div>");
            inputDirectory.GetFile("sections/author.cshtml").WriteAllText(@"Title: About The Author
Order: 2
---
<section id=""features"">
    <header class=""style1"">
        <h2>Your Name</h2>
        <p>Say a little something about yourself.</p>
    </header>
</section>");

            // Page
            inputDirectory.GetFile("custom-page.md").WriteAllText(@"Title: Custom Page
---
You can add as many custom pages as you want.");
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
