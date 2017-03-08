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
    public class Docs : Recipe
    {
        private static readonly ConcurrentDictionary<string, string> TypeNamesToLink = new ConcurrentDictionary<string, string>();

#pragma warning disable 1591

        [SourceInfo]
        public static Pipeline Code { get; } = new Code();

        [SourceInfo]
        public static Pipeline Api { get; } = new Api(TypeNamesToLink);

        [SourceInfo]
        public static Pipeline Pages { get; } = new Pages(TypeNamesToLink);

        [SourceInfo]
        public static Pipeline BlogPosts { get; } = new BlogPosts(TypeNamesToLink);

        [SourceInfo]
        public static Pipeline BlogIndexes { get; } = new BlogIndexes();

        [SourceInfo]
        public static Pipeline BlogCategories { get; } = new BlogCategories();

        [SourceInfo]
        public static Pipeline BlogArchives { get; } = new BlogArchives();

        [SourceInfo]
        public static Pipeline BlogAuthors { get; } = new BlogAuthors();

        [SourceInfo]
        public static Pipeline BlogFeed { get; } = new BlogFeed();

        [SourceInfo]
        public static Pipeline RenderPages { get; } = new RenderPages();

        [SourceInfo]
        public static Pipeline RenderBlogPosts { get; } = new RenderBlogPosts();

        [SourceInfo]
        public static Pipeline Redirects { get; } = new Redirects();

        [SourceInfo]
        public static Pipeline RenderApi { get; } = new RenderApi();

        [SourceInfo]
        public static Pipeline ApiIndex { get; } = new ApiIndex();

        [SourceInfo]
        public static Pipeline ApiSearchIndex { get; } = new ApiSearchIndex();

        [SourceInfo]
        public static Pipeline Less { get; } = new Pipelines.Less();

        [SourceInfo]
        public static Pipeline Resources { get; } = new Resources();

        [SourceInfo]
        public static Pipeline ValidateLinks { get; } = new ValidateLinks();

#pragma warning restore 1591

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
