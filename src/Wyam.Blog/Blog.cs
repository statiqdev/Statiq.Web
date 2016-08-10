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
using Wyam.Core.Modules.Contents;
using Wyam.Core.Modules.Control;
using Wyam.Core.Modules.Extensibility;
using Wyam.Core.Modules.IO;
using Wyam.Core.Modules.Metadata;
using Wyam.Feeds;
using Wyam.Html;

namespace Wyam.Blog
{
    public class Blog : IRecipe
    {
        public void Apply(IEngine engine)
        {
            // Global metadata defaults
            engine.GlobalMetadata[BlogKeys.Title] = "My Blog";
            engine.GlobalMetadata[BlogKeys.Description] = "Welcome!";
            
            // Get the pages first so they're available in the navbar, but don't render until last
            engine.Pipelines.Add(BlogPipelines.Pages,
                new ReadFiles("{!posts,**}/*.md"),
                new FrontMatter(new Yaml.Yaml()),
                new Markdown.Markdown(),
                new Replace("<pre><code>", "<pre class=\"prettyprint\"><code>"),
                new Concat(
                    // Add any additional Razor pages
                    new ReadFiles("{!posts,!tags,**}/*.cshtml"),
                    new FrontMatter(new Yaml.Yaml())),
                new Concat(
                    // Add the posts index page
                    new ReadFiles("posts/index.cshtml"),
                    new FrontMatter(new Yaml.Yaml())),
                new Concat(
                    // Add the tags index page
                    new ReadFiles("tags/index.cshtml"),
                    new FrontMatter(new Yaml.Yaml())),
                new WriteFiles(".html")
                    .OnlyMetadata()
            );

            engine.Pipelines.Add(BlogPipelines.Posts,
                new ReadFiles("posts/*.md"),
                new FrontMatter(new Yaml.Yaml()),
                new Where((doc, ctx) => doc.ContainsKey(BlogKeys.Published) && doc.Get<DateTime>(BlogKeys.Published) <= DateTime.Today),
                new Markdown.Markdown(),
                new Replace("<pre><code>", "<pre class=\"prettyprint\"><code>"),
                new Concat(
                    // Add any posts written in Razor
                    new ReadFiles("posts/{!_,!index,}*.cshtml"),
                    new FrontMatter(new Yaml.Yaml())),
                new OrderBy((doc, ctx) => doc.Get<DateTime>(BlogKeys.Published)).Descending()
            );

            engine.Pipelines.Add(BlogPipelines.Tags,
                new ReadFiles("tags/tag.cshtml"),
                new FrontMatter(new Yaml.Yaml()),
                new GroupByMany(BlogKeys.Tags, new Documents(BlogPipelines.Posts)),
                new Where((doc, ctx) => !string.IsNullOrEmpty(doc.String(Keys.GroupKey))),
                new Meta(BlogKeys.Tag, (doc, ctx) => doc.String(Keys.GroupKey)),
                new Meta(BlogKeys.Title, (doc, ctx) => doc.String(Keys.GroupKey)),
                new Meta(BlogKeys.Posts, (doc, ctx) => doc.List<IDocument>(Keys.GroupDocuments)),
                new Meta(Keys.RelativeFilePath, (doc, ctx) =>
                {
                    string tag = doc.String(Keys.GroupKey);
                    return $"tags/{(tag.StartsWith(".") ? tag.Substring(1) : tag).ToLowerInvariant().Replace(' ', '-')}.html";
                }),
                new Razor.Razor(),
                new WriteFiles()
            );

            engine.Pipelines.Add(BlogPipelines.RenderPosts,
                new Documents(BlogPipelines.Posts),
                new Razor.Razor(),
                new Excerpt()
                    .SetMetadataKey(BlogKeys.Excerpt),
                new Excerpt("div#content")
                    .SetMetadataKey(BlogKeys.Content)
                    .GetOuterHtml(false),
                new WriteFiles(".html"));

            engine.Pipelines.Add(BlogPipelines.Feed,
                new Documents(BlogPipelines.RenderPosts),
                new GenerateFeeds(),
                new WriteFiles());

            engine.Pipelines.Add(BlogPipelines.RenderPages,
                new Documents(BlogPipelines.Pages),
                new Razor.Razor(),
                new WriteFiles()
            );

            engine.Pipelines.Add(BlogPipelines.Resources,
                new CopyFiles("**/*{!.cshtml,!.md,}"));
        }

        public void Scaffold(IDirectory directory)
        {
            // Add info page
            IFile aboutFile = directory.GetFile("about.md");
            aboutFile.WriteAllText(@"Title: About Me
---
I'm awesome!");

            // Add post page
            IDirectory postsDirectory = directory.GetDirectory("posts");
            IFile postFile = postsDirectory.GetFile("first-post.md");
            postFile.WriteAllText(@"Title: First Post
Published: 1/1/2016
Tags: Introduction
---
This is my first post!");
        }
    }
}
