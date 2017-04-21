using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.CodeAnalysis;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Util;
using Wyam.Docs.Pipelines;
using Wyam.Feeds;
using Wyam.WebRecipe;
using Wyam.WebRecipe.Pipelines;

namespace Wyam.Docs
{
    /// <summary>
    /// A recipe for creating documentation websites.
    /// </summary>
    /// <metadata cref="DocsKeys.Title" usage="Setting">The title of the site.</metadata>
    /// <metadata cref="DocsKeys.Title" usage="Input">The title of the post or page.</metadata>
    /// <metadata cref="DocsKeys.Logo" usage="Setting" />
    /// <metadata cref="DocsKeys.SourceFiles" usage="Setting" />
    /// <metadata cref="DocsKeys.SolutionFiles" usage="Setting" />
    /// <metadata cref="DocsKeys.ProjectFiles" usage="Setting" />
    /// <metadata cref="DocsKeys.AssemblyFiles" usage="Setting" />
    /// <metadata cref="DocsKeys.BaseEditUrl" usage="Setting" />
    /// <metadata cref="DocsKeys.IncludeGlobalNamespace" usage="Setting" />
    /// <metadata cref="DocsKeys.AutoLinkTypes" usage="Setting" />
    /// <metadata cref="DocsKeys.IncludeDateInPostPath" usage="Setting" />
    /// <metadata cref="DocsKeys.SearchIndex" usage="Setting" />
    /// <metadata cref="DocsKeys.CaseInsensitiveCategories" usage="Setting" />
    /// <metadata cref="DocsKeys.CaseInsensitiveTags" usage="Setting" />
    /// <metadata cref="DocsKeys.CaseInsensitiveAuthors" usage="Setting" />
    /// <metadata cref="DocsKeys.BlogPageSize" usage="Setting" />
    /// <metadata cref="DocsKeys.CategoryPageSize" usage="Setting" />
    /// <metadata cref="DocsKeys.TagPageSize" usage="Setting" />
    /// <metadata cref="DocsKeys.AuthorPageSize" usage="Setting" />
    /// <metadata cref="DocsKeys.MonthPageSize" usage="Setting" />
    /// <metadata cref="DocsKeys.YearPageSize" usage="Setting" />
    /// <metadata cref="DocsKeys.MarkdownConfiguration" usage="Setting" />
    /// <metadata cref="DocsKeys.MarkdownExtensionTypes" usage="Setting" />
    /// <metadata cref="DocsKeys.IgnoreFolders" usage="Setting" />
    /// <metadata cref="DocsKeys.MetaRefreshRedirects" usage="Setting" />
    /// <metadata cref="DocsKeys.NetlifyRedirects" usage="Setting" />
    /// <metadata cref="DocsKeys.BlogRssPath" usage="Setting" />
    /// <metadata cref="DocsKeys.BlogAtomPath" usage="Setting" />
    /// <metadata cref="DocsKeys.BlogRdfPath" usage="Setting" />
    /// <metadata cref="DocsKeys.BlogPath" usage="Setting" />
    /// <metadata cref="DocsKeys.ValidateAbsoluteLinks" usage="Setting" />
    /// <metadata cref="DocsKeys.ValidateRelativeLinks" usage="Setting" />
    /// <metadata cref="DocsKeys.ValidateLinksAsError" usage="Setting" />
    /// <metadata cref="DocsKeys.Description" usage="Input" />
    /// <metadata cref="DocsKeys.Category" usage="Input" />
    /// <metadata cref="DocsKeys.Tags" usage="Input" />
    /// <metadata cref="DocsKeys.Order" usage="Input" />
    /// <metadata cref="DocsKeys.NoSidebar" usage="Input" />
    /// <metadata cref="DocsKeys.NoContainer" usage="Input" />
    /// <metadata cref="DocsKeys.NoTitle" usage="Input" />
    /// <metadata cref="DocsKeys.NoGutter" usage="Input" />
    /// <metadata cref="DocsKeys.Published" usage="Input" />
    /// <metadata cref="DocsKeys.Author" usage="Input" />
    /// <metadata cref="WebKeys.EditFilePath" usage="Output" />
    public class Docs : Recipe
    {
        /// <summary>
        /// Passes type names and paths from the Api pipeline to following ones for auto-linking.
        /// </summary>
        private static readonly ConcurrentDictionary<string, string> TypeNamesToLink = new ConcurrentDictionary<string, string>();

        /// <inheritdoc cref="Pipelines.Code" />
        [SourceInfo]
        public static Code Code { get; } = new Code();

        /// <inheritdoc cref="Pipelines.Api" />
        [SourceInfo]
        public static Api Api { get; } = new Api(TypeNamesToLink);

        /// <inheritdoc cref="Pipelines.Pages" />
        [SourceInfo]
        public static Pipelines.Pages Pages { get; } = new Pipelines.Pages(TypeNamesToLink);

        /// <inheritdoc cref="Pipelines.BlogPosts" />
        [SourceInfo]
        public static Pipelines.BlogPosts BlogPosts { get; } = new Pipelines.BlogPosts(TypeNamesToLink);

        /// <summary>
        /// Generates the index pages for blog posts.
        /// </summary>
        [SourceInfo]
        public static Archive BlogIndexes { get; } = new Archive(
            nameof(BlogIndexes),
            new string[] { BlogPosts },
            "_BlogIndex.cshtml",
            "/_BlogLayout.cshtml",
            null,
            null,
            ctx => ctx.Get(DocsKeys.BlogPageSize, int.MaxValue),
            (doc, _) => doc.Get<DateTime>(DocsKeys.Published),
            true,
            (doc, ctx) => "Blog",
            (doc, ctx) => $"{ctx.DirectoryPath(DocsKeys.BlogPath).FullPath}",
            null,
            null);

        /// <summary>
        /// Generates the category pages for blog posts.
        /// </summary>
        [SourceInfo]
        public static Archive BlogCategories { get; } = new Archive(
            nameof(BlogCategories),
            new string[] { BlogPosts },
            "_BlogIndex.cshtml",
            "/_BlogLayout.cshtml",
            (doc, ctx) => doc.List<string>(DocsKeys.Category),
            ctx => ctx.Bool(DocsKeys.CaseInsensitiveCategories),
            ctx => ctx.Get(DocsKeys.CategoryPageSize, int.MaxValue),
            (doc, _) => doc.Get<DateTime>(DocsKeys.Published),
            true,
            (doc, ctx) => doc.String(Keys.GroupKey),
            (doc, ctx) => $"{ctx.DirectoryPath(DocsKeys.BlogPath).FullPath}/{doc.String(Keys.GroupKey)}",
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
            "/_BlogLayout.cshtml",
            (doc, ctx) => doc.List<string>(DocsKeys.Tags),
            ctx => ctx.Bool(DocsKeys.CaseInsensitiveTags),
            ctx => ctx.Get(DocsKeys.TagPageSize, int.MaxValue),
            (doc, _) => doc.Get<DateTime>(DocsKeys.Published),
            true,
            (doc, ctx) => doc.String(Keys.GroupKey),
            (doc, ctx) => $"{ctx.DirectoryPath(DocsKeys.BlogPath).FullPath}/tag/{doc.String(Keys.GroupKey)}",
            null,
            null);

        /// <summary>
        /// Generates the author pages for blog posts.
        /// </summary>
        [SourceInfo]
        public static Archive BlogAuthors { get; } = new Archive(
            nameof(BlogAuthors),
            new string[] { BlogPosts },
            "_BlogIndex.cshtml",
            "/_BlogLayout.cshtml",
            (doc, ctx) => doc.List<string>(DocsKeys.Author),
            ctx => ctx.Bool(DocsKeys.CaseInsensitiveAuthors),
            ctx => ctx.Get(DocsKeys.AuthorPageSize, int.MaxValue),
            (doc, _) => doc.Get<DateTime>(DocsKeys.Published),
            true,
            (doc, ctx) => doc.String(Keys.GroupKey),
            (doc, ctx) => $"{ctx.DirectoryPath(DocsKeys.BlogPath).FullPath}/author/{doc.String(Keys.GroupKey)}",
            null,
            null);

        /// <summary>
        /// Generates the monthly archive pages for blog posts.
        /// </summary>
        [SourceInfo]
        public static Archive BlogArchives { get; } = new Archive(
            nameof(BlogArchives),
            new string[] { BlogPosts },
            "_BlogIndex.cshtml",
            "/_BlogLayout.cshtml",
            (doc, ctx) => new DateTime(doc.Get<DateTime>(DocsKeys.Published).Year, doc.Get<DateTime>(DocsKeys.Published).Month, 1),
            null,
            ctx => ctx.Get(DocsKeys.MonthPageSize, int.MaxValue),
            (doc, _) => doc.Get<DateTime>(DocsKeys.Published),
            true,
            (doc, ctx) => doc.Get<DateTime>(Keys.GroupKey).ToString("MMMM, yyyy"),
            (doc, ctx) => $"blog/archive/{doc.Get<DateTime>(Keys.GroupKey):yyyy/MM}",
            null,
            null);

        /// <summary>
        /// Generates the yearly archive pages for blog posts.
        /// </summary>
        [SourceInfo]
        public static Archive BlogYearlyArchives { get; } = new Archive(
            nameof(BlogYearlyArchives),
            new string[] { BlogPosts },
            "_BlogIndex.cshtml",
            "/_BlogLayout.cshtml",
            (doc, ctx) => new DateTime(doc.Get<DateTime>(DocsKeys.Published).Year, 1, 1),
            null,
            ctx => ctx.Get(DocsKeys.MonthPageSize, int.MaxValue),
            (doc, _) => doc.Get<DateTime>(DocsKeys.Published),
            true,
            (doc, ctx) => doc.Get<DateTime>(Keys.GroupKey).ToString("yyyy"),
            (doc, ctx) => $"blog/archive/{doc.Get<DateTime>(Keys.GroupKey):yyyy}",
            null,
            null);

        /// <summary>
        /// Generates the blog RSS, Atom, and/or RDF feeds.
        /// </summary>
        [SourceInfo]
        public static WebRecipe.Pipelines.Feeds BlogFeed { get; } = new WebRecipe.Pipelines.Feeds(
            nameof(BlogFeed),
            new string[] { BlogPosts },
            ctx => ctx.FilePath(DocsKeys.BlogRssPath),
            ctx => ctx.FilePath(DocsKeys.BlogAtomPath),
            ctx => ctx.FilePath(DocsKeys.BlogRdfPath));

        /// <inheritdoc cref="Pipelines.RenderPages" />
        [SourceInfo]
        public static RenderPages RenderPages { get; } = new RenderPages();

        /// <inheritdoc cref="Pipelines.RenderBlogPosts" />
        [SourceInfo]
        public static RenderBlogPosts RenderBlogPosts { get; } = new RenderBlogPosts();

        /// <inheritdoc cref="WebRecipe.Pipelines.Redirects" />
        [SourceInfo]
        public static Redirects Redirects { get; } = new Redirects(
            nameof(Redirects),
            new string[] { RenderPages, RenderBlogPosts },
            ctx => ctx.Bool(DocsKeys.MetaRefreshRedirects),
            ctx => ctx.Bool(DocsKeys.NetlifyRedirects));

        /// <inheritdoc cref="Pipelines.RenderApi" />
        [SourceInfo]
        public static RenderApi RenderApi { get; } = new RenderApi();

        /// <inheritdoc cref="Pipelines.ApiIndex" />
        [SourceInfo]
        public static ApiIndex ApiIndex { get; } = new ApiIndex();

        /// <inheritdoc cref="Pipelines.ApiSearchIndex" />
        [SourceInfo]
        public static ApiSearchIndex ApiSearchIndex { get; } = new ApiSearchIndex();

        /// <inheritdoc cref="WebRecipe.Pipelines.Less" />
        [SourceInfo]
        public static WebRecipe.Pipelines.Less Less { get; } = new WebRecipe.Pipelines.Less(
            nameof(Less),
            new string[]
            {
                "assets/css/*.less",
                "assets/css/bootstrap/bootstrap.less",
                "assets/css/adminlte/AdminLTE.less",
                "assets/css/theme/theme.less"
            });

        /// <inheritdoc cref="WebRecipe.Pipelines.Resources" />
        [SourceInfo]
        public static Resources Resources { get; } = new Resources(nameof(Resources));

        /// <inheritdoc cref="WebRecipe.Pipelines.ValidateLinks" />
        [SourceInfo]
        public static ValidateLinks ValidateLinks { get; } = new ValidateLinks(
            nameof(ValidateLinks),
            new string[] { RenderPages, RenderBlogPosts, RenderApi, Resources },
            ctx => ctx.Bool(DocsKeys.ValidateAbsoluteLinks),
            ctx => ctx.Bool(DocsKeys.ValidateRelativeLinks),
            ctx => ctx.Bool(DocsKeys.ValidateLinksAsError));

        /// <inheritdoc />
        public override void Apply(IEngine engine)
        {
            // Global metadata defaults
            engine.Settings[DocsKeys.SourceFiles] = new []
            {
                "src/**/{!bin,!obj,!packages,!*.Tests,}/**/*.cs",
                "../src/**/{!bin,!obj,!packages,!*.Tests,}/**/*.cs"
            };
            engine.Settings[DocsKeys.IncludeGlobalNamespace] = true;
            engine.Settings[DocsKeys.IncludeDateInPostPath] = false;
            engine.Settings[DocsKeys.MarkdownConfiguration] = "advanced+bootstrap";
            engine.Settings[DocsKeys.SearchIndex] = true;
            engine.Settings[DocsKeys.MetaRefreshRedirects] = true;
            engine.Settings[DocsKeys.AutoLinkTypes] = true;
            engine.Settings[DocsKeys.BlogPath] = "blog";
            engine.Settings[DocsKeys.BlogPageSize] = 5;
            engine.Settings[DocsKeys.CategoryPageSize] = 5;
            engine.Settings[DocsKeys.TagPageSize] = 5;
            engine.Settings[DocsKeys.AuthorPageSize] = 5;
            engine.Settings[DocsKeys.MonthPageSize] = 5;
            engine.Settings[DocsKeys.YearPageSize] = 5;
            engine.Settings[DocsKeys.BlogRssPath] = GenerateFeeds.DefaultRssPath;
            engine.Settings[DocsKeys.BlogAtomPath] = GenerateFeeds.DefaultAtomPath;
            engine.Settings[DocsKeys.BlogRdfPath] = GenerateFeeds.DefaultRdfPath;

            base.Apply(engine);
        }

        /// <inheritdoc />
        public override void Scaffold(IFile configFile, IDirectory inputDirectory)
        {
            // Config file
            configFile?.WriteAllText(@"#recipe Docs");

            // Add info page
            inputDirectory.GetFile("about.md").WriteAllText(
@"Title: About This Project
---
This project is awesome!");

            // Add docs pages
            inputDirectory.GetFile("docs/command-line.md").WriteAllText(
@"Description: How to use the command line.
---
Here are some instructions on how to use the command line.");
            inputDirectory.GetFile("docs/usage.md").WriteAllText(
@"Description: Library usage instructions.
---
To use this library, take these steps...");

            // Add post page
            inputDirectory.GetFile("blog/new-release.md").WriteAllText(
@"Title: New Release
Published: 1/1/2016
Category: Release
Author: me
---
There is a new release out, go get it now.");
        }
    }
}
