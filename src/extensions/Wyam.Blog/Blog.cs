using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Blog.Pipelines;
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

namespace Wyam.Blog
{
    /// <summary>
    /// A recipe for creating blogging websites.
    /// </summary>
    public class Blog : Recipe
    {
#pragma warning disable 1591

        [SourceInfo]
        public static Pipeline Pages { get; } = new Pages();

        [SourceInfo]
        public static Pipeline RawPosts { get; } = new RawPosts();

        [SourceInfo]
        public static Pipeline Tags { get; } = new Tags();

        [SourceInfo]
        public static Pipeline Posts { get; } = new Posts();

        [SourceInfo]
        public static Pipeline Feed { get; } = new Feed();

        [SourceInfo]
        public static Pipeline RenderPages { get; } = new RenderPages();

        [SourceInfo]
        public static Pipeline Redirects { get; } = new Redirects();

        [SourceInfo]
        public static Pipeline Resources { get; } = new Resources();

#pragma warning restore 1591

        /// <summary>
        /// The <see cref="ValidateLinks"/> pipeline.
        /// </summary>
        [SourceInfo]
        public static Pipeline ValidateLinks { get; } = new ValidateLinks();

        /// <inheritdoc/>
        public override void Apply(IEngine engine)
        {
            engine.Settings[BlogKeys.Title] = "My Blog";
            engine.Settings[BlogKeys.Description] = "Welcome!";
            engine.Settings[BlogKeys.MarkdownExtensions] = "advanced+bootstrap";
            engine.Settings[BlogKeys.IncludeDateInPostPath] = false;
            engine.Settings[BlogKeys.PostsPath] = new DirectoryPath("posts");
            engine.Settings[BlogKeys.MetaRefreshRedirects] = true;
            engine.Settings[BlogKeys.RssPath] = GenerateFeeds.DefaultRssPath;
            engine.Settings[BlogKeys.AtomPath] = GenerateFeeds.DefaultAtomPath;
            engine.Settings[BlogKeys.RdfPath] = GenerateFeeds.DefaultRdfPath;

            base.Apply(engine);
            // TODO: Revert the BlogPipelines class and obsolete all the members, search and replace the themes to point to Blog.Xyz instead

        }

        /// <inheritdoc />
        public override void Scaffold(IFile configFile, IDirectory inputDirectory)
        {
            // Config file
            configFile?.WriteAllText(@"#recipe Blog");

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
