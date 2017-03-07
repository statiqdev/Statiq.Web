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
        /// <summary>
        /// The <see cref="Pages"/> pipeline.
        /// </summary>
        [SourceInfo]
        public static RecipePipeline Pages { get; } = new Pages();

        /// <summary>
        /// The <see cref="RawPosts"/> pipeline.
        /// </summary>
        [SourceInfo]
        public static RecipePipeline RawPosts { get; } = new RawPosts();

        /// <summary>
        /// The <see cref="Tags"/> pipeline.
        /// </summary>
        [SourceInfo]
        public static RecipePipeline Tags { get; } = new Tags();

        /// <summary>
        /// The <see cref="Posts"/> pipeline.
        /// </summary>
        [SourceInfo]
        public static RecipePipeline Posts { get; } = new Posts();

        /// <summary>
        /// The <see cref="Feed"/> pipeline.
        /// </summary>
        [SourceInfo]
        public static RecipePipeline Feed { get; } = new Feed();

        /// <summary>
        /// The <see cref="RenderPages"/> pipeline.
        /// </summary>
        [SourceInfo]
        public static RecipePipeline RenderPages { get; } = new RenderPages();

        /// <summary>
        /// The <see cref="Redirects"/> pipeline.
        /// </summary>
        [SourceInfo]
        public static RecipePipeline Redirects { get; } = new Redirects();

        /// <summary>
        /// The <see cref="Resources"/> pipeline.
        /// </summary>
        [SourceInfo]
        public static RecipePipeline Resources { get; } = new Resources();

        /// <summary>
        /// The <see cref="ValidateLinks"/> pipeline.
        /// </summary>
        [SourceInfo]
        public static RecipePipeline ValidateLinks { get; } = new ValidateLinks();

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
