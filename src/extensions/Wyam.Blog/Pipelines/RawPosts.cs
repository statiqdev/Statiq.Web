using System;
using Wyam.Common.Configuration;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Core.Modules.Control;
using Wyam.Core.Modules.Extensibility;
using Wyam.Core.Modules.IO;
using Wyam.Core.Modules.Metadata;

namespace Wyam.Blog.Pipelines
{
    /// <summary>
    /// Loads blog posts from Markdown and/or Razor files.
    /// </summary>
    public class RawPosts : RecipePipeline
    {
        /// <summary>
        /// Reads all markdown posts, processes their front matter, and renders them to HTML.
        /// </summary>
        public string MarkdownPosts { get; } = nameof(MarkdownPosts);

        /// <summary>
        /// Reads all Razor posts and processes their front matter (but does not render them to HTML).
        /// </summary>
        public string RazorPosts { get; } = nameof(RazorPosts);

        /// <summary>
        /// Sets a published date for each post.
        /// </summary>
        public string Published { get; } = nameof(Published);

        /// <summary>
        /// Sets the relative file path for each post in metadata.
        /// </summary>
        public string RelativeFilePath { get; } = nameof(RelativeFilePath);

        /// <summary>
        /// Orders the posts by their published date.
        /// </summary>
        public string OrderByPublished { get; } = nameof(OrderByPublished);

        /// <inheritdoc />
        public override ModuleList GetModules() => new ModuleList
        {
            {
                MarkdownPosts,
                new ModuleCollection
                {
                    new ReadFiles(ctx => $"{ctx.DirectoryPath(BlogKeys.PostsPath).FullPath}/*.md"),
                    new FrontMatter(new Yaml.Yaml()),
                    new Execute(ctx => new Markdown.Markdown()
                        .UseExtensions(ctx.Settings.List<Type>(BlogKeys.MarkdownExternalExtensions))
                        .UseConfiguration(ctx.String(BlogKeys.MarkdownExtensions)))
                }
            },
            {
                RazorPosts,
                new Concat
                {
                    new ReadFiles(ctx => $"{ctx.DirectoryPath(BlogKeys.PostsPath).FullPath}/{{!_,!index,}}*.cshtml"),
                    new FrontMatter(new Yaml.Yaml())
                }
            },
            {
                Published,
                new ModuleCollection
                {
                    new Meta("FrontMatterPublished", (doc, ctx) => doc.ContainsKey(BlogKeys.Published)),  // Record whether the publish date came from front matter
                    new Meta(BlogKeys.Published, (doc, ctx) =>
                    {
                        DateTime published;
                        if (!DateTime.TryParse(doc.String(Keys.SourceFileName).Substring(0, 10), out published))
                        {
                            Common.Tracing.Trace.Warning($"Could not parse published date for {doc.SourceString()}.");
                            return null;
                        }
                        return published;
                    }).OnlyIfNonExisting(),
                    new Where((doc, ctx) =>
                    {
                        if (!doc.ContainsKey(BlogKeys.Published) || doc.Get(BlogKeys.Published) == null)
                        {
                            Common.Tracing.Trace.Warning($"Skipping {doc.SourceString()} due to not having {BlogKeys.Published} metadata");
                            return false;
                        }
                        if (doc.Get<DateTime>(BlogKeys.Published) > DateTime.Now)
                        {
                            Common.Tracing.Trace.Warning($"Skipping {doc.SourceString()} due to having {BlogKeys.Published} metadata of {doc.Get<DateTime>(BlogKeys.Published)} in the future (current date and time is {DateTime.Now})");
                            return false;
                        }
                        return true;
                    })
                }
            },
            {
                RelativeFilePath,
                new Meta(Keys.RelativeFilePath, (doc, ctx) =>
                {
                    DateTime published = doc.Get<DateTime>(BlogKeys.Published);
                    string fileName = doc.Bool("FrontMatterPublished")
                        ? doc.FilePath(Keys.SourceFileName).ChangeExtension("html").FullPath
                        : doc.FilePath(Keys.SourceFileName).ChangeExtension("html").FullPath.Substring(11);
                    return ctx.Bool(BlogKeys.IncludeDateInPostPath)
                        ? $"{ctx.DirectoryPath(BlogKeys.PostsPath).FullPath}/{published:yyyy}/{published:MM}/{fileName}"
                        : $"{ctx.DirectoryPath(BlogKeys.PostsPath).FullPath}/{fileName}";
                })
            },
            {
                OrderByPublished,
                new OrderBy((doc, ctx) => doc.Get<DateTime>(BlogKeys.Published)).Descending()
            }
        };
    }
}
