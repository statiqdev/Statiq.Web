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
    /// <metadata cref="DocsKeys.MarkdownExtensions" usage="Setting" />
    /// <metadata cref="DocsKeys.MarkdownExternalExtensions" usage="Setting" />
    /// <metadata cref="DocsKeys.IgnoreFolders" usage="Setting" />
    /// <metadata cref="DocsKeys.MetaRefreshRedirects" usage="Setting" />
    /// <metadata cref="DocsKeys.NetlifyRedirects" usage="Setting" />
    /// <metadata cref="DocsKeys.BlogRssPath" usage="Setting" />
    /// <metadata cref="DocsKeys.BlogAtomPath" usage="Setting" />
    /// <metadata cref="DocsKeys.BlogRdfPath" usage="Setting" />
    /// <metadata cref="DocsKeys.ValidateAbsoluteLinks" usage="Setting" />
    /// <metadata cref="DocsKeys.ValidateRelativeLinks" usage="Setting" />
    /// <metadata cref="DocsKeys.ValidateLinksAsError" usage="Setting" />
    /// <metadata cref="DocsKeys.EditFilePath" usage="Output" />
    /// <metadata cref="DocsKeys.Description" usage="Input" />
    /// <metadata cref="DocsKeys.Category" usage="Input" />
    /// <metadata cref="DocsKeys.Order" usage="Input" />
    /// <metadata cref="DocsKeys.NoSidebar" usage="Input" />
    /// <metadata cref="DocsKeys.NoContainer" usage="Input" />
    /// <metadata cref="DocsKeys.NoTitle" usage="Input" />
    /// <metadata cref="DocsKeys.NoGutter" usage="Input" />
    /// <metadata cref="DocsKeys.Published" usage="Input" />
    /// <metadata cref="DocsKeys.Author" usage="Input" />
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
        public static Pages Pages { get; } = new Pages(TypeNamesToLink);

        /// <inheritdoc cref="Pipelines.BlogPosts" />
        [SourceInfo]
        public static BlogPosts BlogPosts { get; } = new BlogPosts(TypeNamesToLink);

        /// <inheritdoc cref="Pipelines.BlogIndexes" />
        [SourceInfo]
        public static BlogIndexes BlogIndexes { get; } = new BlogIndexes();

        /// <inheritdoc cref="Pipelines.BlogCategories" />
        [SourceInfo]
        public static BlogCategories BlogCategories { get; } = new BlogCategories();

        /// <inheritdoc cref="Pipelines.BlogArchives" />
        [SourceInfo]
        public static BlogArchives BlogArchives { get; } = new BlogArchives();

        /// <inheritdoc cref="Pipelines.BlogAuthors" />
        [SourceInfo]
        public static BlogAuthors BlogAuthors { get; } = new BlogAuthors();

        /// <inheritdoc cref="Pipelines.BlogFeed" />
        [SourceInfo]
        public static BlogFeed BlogFeed { get; } = new BlogFeed();

        /// <inheritdoc cref="Pipelines.RenderPages" />
        [SourceInfo]
        public static RenderPages RenderPages { get; } = new RenderPages();

        /// <inheritdoc cref="Pipelines.RenderBlogPosts" />
        [SourceInfo]
        public static RenderBlogPosts RenderBlogPosts { get; } = new RenderBlogPosts();

        /// <inheritdoc cref="Pipelines.Redirects" />
        [SourceInfo]
        public static Redirects Redirects { get; } = new Redirects();

        /// <inheritdoc cref="Pipelines.RenderApi" />
        [SourceInfo]
        public static RenderApi RenderApi { get; } = new RenderApi();

        /// <inheritdoc cref="Pipelines.ApiIndex" />
        [SourceInfo]
        public static ApiIndex ApiIndex { get; } = new ApiIndex();

        /// <inheritdoc cref="Pipelines.ApiSearchIndex" />
        [SourceInfo]
        public static ApiSearchIndex ApiSearchIndex { get; } = new ApiSearchIndex();

        /// <inheritdoc cref="Pipelines.Less" />
        [SourceInfo]
        public static Pipelines.Less Less { get; } = new Pipelines.Less();

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
            engine.Settings[DocsKeys.SourceFiles] = new []
            {
                "src/**/{!bin,!obj,!packages,!*.Tests,}/**/*.cs",
                "../src/**/{!bin,!obj,!packages,!*.Tests,}/**/*.cs"
            };
            engine.Settings[DocsKeys.IncludeGlobalNamespace] = true;
            engine.Settings[DocsKeys.IncludeDateInPostPath] = false;
            engine.Settings[DocsKeys.MarkdownExtensions] = "advanced+bootstrap";
            engine.Settings[DocsKeys.SearchIndex] = true;
            engine.Settings[DocsKeys.MetaRefreshRedirects] = true;
            engine.Settings[DocsKeys.AutoLinkTypes] = true;
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
