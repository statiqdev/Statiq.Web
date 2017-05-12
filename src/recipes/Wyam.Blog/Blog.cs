using System;
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
using Wyam.Core.Modules.Contents;
using Wyam.Core.Modules.Control;
using Wyam.Core.Modules.Extensibility;
using Wyam.Core.Modules.IO;
using Wyam.Core.Modules.Metadata;
using Wyam.Feeds;
using Wyam.Web.Pipelines;

namespace Wyam.Blog
{
    /// <summary>
    /// A recipe for creating blogging websites.
    /// </summary>
    /// <metadata cref="BlogKeys.Title" usage="Setting">The title of the blog.</metadata>
    /// <metadata cref="BlogKeys.Title" usage="Input">The title of the post or page.</metadata>
    /// <metadata cref="BlogKeys.Image" usage="Setting">The relative path to an image to display on the home page.</metadata>
    /// <metadata cref="BlogKeys.Image" usage="Input">The relative path to an image for the current post or page (often shown in the header of the page).</metadata>
    /// <metadata cref="BlogKeys.ProcessIncludes" usage="Setting" />
    /// <metadata cref="BlogKeys.ProcessIncludes" usage="Input" />
    /// <metadata cref="BlogKeys.Description" usage="Setting" />
    /// <metadata cref="BlogKeys.Intro" usage="Setting" />
    /// <metadata cref="BlogKeys.PostsPath" usage="Setting" />
    /// <metadata cref="BlogKeys.CaseInsensitiveTags" usage="Setting" />
    /// <metadata cref="BlogKeys.MarkdownConfiguration" usage="Setting" />
    /// <metadata cref="BlogKeys.MarkdownExtensionTypes" usage="Setting" />
    /// <metadata cref="BlogKeys.IncludeDateInPostPath" usage="Setting" />
    /// <metadata cref="BlogKeys.MetaRefreshRedirects" usage="Setting" />
    /// <metadata cref="BlogKeys.NetlifyRedirects" usage="Setting" />
    /// <metadata cref="BlogKeys.RssPath" usage="Setting" />
    /// <metadata cref="BlogKeys.AtomPath" usage="Setting" />
    /// <metadata cref="BlogKeys.RdfPath" usage="Setting" />
    /// <metadata cref="BlogKeys.ValidateAbsoluteLinks" usage="Setting" />
    /// <metadata cref="BlogKeys.ValidateRelativeLinks" usage="Setting" />
    /// <metadata cref="BlogKeys.ValidateLinksAsError" usage="Setting" />
    /// <metadata cref="BlogKeys.TagPageSize" usage="Setting" />
    /// <metadata cref="BlogKeys.ArchivePageSize" usage="Setting" />
    /// <metadata cref="BlogKeys.IgnoreFolders" usage="Setting" />
    /// <metadata cref="BlogKeys.Published" usage="Input" />
    /// <metadata cref="BlogKeys.Tags" usage="Input" />
    /// <metadata cref="BlogKeys.Lead" usage="Input" />
    /// <metadata cref="BlogKeys.Excerpt" usage="Output" />
    /// <metadata cref="BlogKeys.ShowInNavbar" usage="Input" />
    /// <metadata cref="BlogKeys.Posts" usage="Output" />
    /// <metadata cref="BlogKeys.Tag" usage="Output" />
    public class Blog : Recipe
    {
        /// <inheritdoc cref="Web.Pipelines.Pages" />
        [SourceInfo]
        public static Pages Pages { get; } = new Pages(
            nameof(Pages),
            null,
            ctx => new[] { ctx.DirectoryPath(BlogKeys.PostsPath).FullPath }
                .Concat(ctx.List(BlogKeys.IgnoreFolders, Array.Empty<string>())),
            ctx => ctx.String(BlogKeys.MarkdownConfiguration),
            ctx => ctx.List<Type>(BlogKeys.MarkdownExtensionTypes),
            (doc, ctx) => doc.Bool(BlogKeys.ProcessIncludes),
            null,
            false,
            null);

        /// <inheritdoc cref="Web.Pipelines.BlogPosts" />
        [SourceInfo]
        public static BlogPosts BlogPosts { get; } = new BlogPosts(
            nameof(BlogPosts),
            BlogKeys.Published,
            ctx => ctx.String(BlogKeys.MarkdownConfiguration),
            ctx => ctx.List<Type>(BlogKeys.MarkdownExtensionTypes),
            (doc, ctx) => doc.Bool(BlogKeys.ProcessIncludes),
            ctx => ctx.Bool(BlogKeys.IncludeDateInPostPath),
            ctx => ctx.DirectoryPath(BlogKeys.PostsPath).FullPath);

        /// <summary>
        /// Generates the tag pages for blog posts.
        /// </summary>
        [SourceInfo]
        public static Archive Tags { get; } = new Archive(
            nameof(Tags),
            new string[] { BlogPosts },
            "_Tag.cshtml",
            "/_Layout.cshtml",
            (doc, ctx) => doc.List<string>(BlogKeys.Tags),
            ctx => ctx.Bool(BlogKeys.CaseInsensitiveTags),
            ctx => ctx.Get(BlogKeys.TagPageSize, int.MaxValue),
            null,
            (doc, ctx) => doc.String(Keys.GroupKey),
            (doc, ctx) => $"tags/{doc.String(Keys.GroupKey)}.html",
            BlogKeys.Posts,
            BlogKeys.Tag);

        /// <summary>
        /// Generates the index pages for blog posts.
        /// </summary>
        [SourceInfo]
        public static Archive BlogArchive { get; } = new Archive(
            nameof(BlogArchive),
            new string[] { BlogPosts },
            "_PostIndex.cshtml",
            "/_Layout.cshtml",
            null,
            null,
            ctx => ctx.Get(BlogKeys.ArchivePageSize, int.MaxValue),
            null,
            (doc, ctx) => "Archive",
            (doc, ctx) => $"{ctx.DirectoryPath(BlogKeys.PostsPath).FullPath}",
            null,
            null);

        /// <inheritdoc cref="Web.Pipelines.Feeds" />
        [SourceInfo]
        public static Web.Pipelines.Feeds Feed { get; } = new Web.Pipelines.Feeds(
            nameof(Feed),
            new string[] { BlogPosts },
            ctx => ctx.FilePath(BlogKeys.RssPath),
            ctx => ctx.FilePath(BlogKeys.AtomPath),
            ctx => ctx.FilePath(BlogKeys.RdfPath));

        /// <inheritdoc cref="Web.Pipelines.RenderBlogPosts" />
        [SourceInfo]
        public static RenderBlogPosts RenderBlogPosts { get; } = new RenderBlogPosts(
            nameof(RenderBlogPosts),
            new string[] { BlogPosts },
            BlogKeys.Published,
            (doc, ctx) => "/_PostLayout.cshtml");

        /// <inheritdoc cref="Web.Pipelines.RenderPages" />
        [SourceInfo]
        public static RenderPages RenderPages { get; } = new RenderPages(
            nameof(RenderPages),
            new string[] { Pages },
            (doc, ctx) => "/_Layout.cshtml",
            null);

        /// <inheritdoc cref="Web.Pipelines.Redirects" />
        [SourceInfo]
        public static Redirects Redirects { get; } = new Redirects(
            nameof(Redirects),
            new string[] { RenderPages, RenderBlogPosts },
            ctx => ctx.Bool(BlogKeys.MetaRefreshRedirects),
            ctx => ctx.Bool(BlogKeys.NetlifyRedirects));

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
            new string[] { RenderPages, RenderBlogPosts, Resources },
            ctx => ctx.Bool(BlogKeys.ValidateAbsoluteLinks),
            ctx => ctx.Bool(BlogKeys.ValidateRelativeLinks),
            ctx => ctx.Bool(BlogKeys.ValidateLinksAsError));

        // Obsolete pipeline keys

        [Obsolete("The Blog.RawPosts pipeline key is obsolete, please use Blog.BlogPosts instead.")]
        public const string RawPosts = nameof(BlogPosts);

        [Obsolete("The Blog.Posts pipeline key is obsolete, please use Blog.RenderBlogPosts instead.")]
        public const string Posts = nameof(RenderBlogPosts);

        /// <inheritdoc/>
        public override void Apply(IEngine engine)
        {
            // Global metadata defaults
            engine.Settings[BlogKeys.Title] = "My Blog";
            engine.Settings[BlogKeys.Description] = "Welcome!";
            engine.Settings[BlogKeys.MarkdownConfiguration] = "advanced+bootstrap";
            engine.Settings[BlogKeys.IncludeDateInPostPath] = false;
            engine.Settings[BlogKeys.PostsPath] = new DirectoryPath("posts");
            engine.Settings[BlogKeys.MetaRefreshRedirects] = true;
            engine.Settings[BlogKeys.RssPath] = GenerateFeeds.DefaultRssPath;
            engine.Settings[BlogKeys.AtomPath] = GenerateFeeds.DefaultAtomPath;
            engine.Settings[BlogKeys.RdfPath] = GenerateFeeds.DefaultRdfPath;

            base.Apply(engine);
        }

        /// <inheritdoc />
        public override void Scaffold(IFile configFile, IDirectory inputDirectory)
        {
            // Config file
            configFile?.WriteAllText(@"#recipe Blog

// Customize your settings and add new ones here
Settings[Keys.Host] = ""host.com"";
Settings[BlogKeys.Title] = ""My Blog"";
Settings[BlogKeys.Description] = ""Welcome!"";

// Add any pipeline customizations here");

            // Add info page
            inputDirectory.GetFile("about.md").WriteAllText(
@"Title: About Me
---
I'm awesome!");

            // Add post page
            inputDirectory.GetFile("posts/first-post.md").WriteAllText(
@"Title: First Post
Published: 1/1/2016
Tags: Introduction
---
This is my first post!");
        }
    }
}
