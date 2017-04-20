using System;
using Wyam.Common.Configuration;
using Wyam.Common.Execution;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Util;
using Wyam.Core.Modules.Control;
using Wyam.Core.Modules.Extensibility;
using Wyam.Core.Modules.IO;
using Wyam.Core.Modules.Metadata;

namespace Wyam.BookSite.Pipelines
{
    /// <summary>
    /// Loads blog posts from Markdown and/or Razor files.
    /// </summary>
    public class RawPosts : Pipeline
    {
        /// <summary>
        /// Reads all markdown posts, processes their front matter, and renders them to HTML.
        /// </summary>
        public const string MarkdownPosts = nameof(MarkdownPosts);

        /// <summary>
        /// Reads all Razor posts and processes their front matter (but does not render them to HTML).
        /// </summary>
        public const string RazorPosts = nameof(RazorPosts);

        /// <summary>
        /// Sets a published date for each post.
        /// </summary>
        public const string Published = nameof(Published);

        /// <summary>
        /// Sets the relative file path for each post in metadata.
        /// </summary>
        public const string RelativeFilePath = nameof(RelativeFilePath);

        /// <summary>
        /// Orders the posts by their published date.
        /// </summary>
        public const string OrderByPublished = nameof(OrderByPublished);

        internal RawPosts()
            : base(GetModules())
        {
        }

        private static ModuleList GetModules() => new ModuleList
        {
            {
                MarkdownPosts,
                new ModuleCollection
                {
                    new ReadFiles(ctx => $"{ctx.DirectoryPath(BookSiteKeys.PostsPath).FullPath}/*.md"),
                    new FrontMatter(new Yaml.Yaml()),
                    new Execute(ctx => new Markdown.Markdown()
                        .UseExtensions(ctx.Settings.List<Type>(BookSiteKeys.MarkdownExternalExtensions))
                        .UseConfiguration(ctx.String(BookSiteKeys.MarkdownExtensions)))
                }
            },
            {
                RazorPosts,
                new Concat
                {
                    new ReadFiles(ctx => $"{ctx.DirectoryPath(BookSiteKeys.PostsPath).FullPath}/{{!_,!index,}}*.cshtml"),
                    new FrontMatter(new Yaml.Yaml())
                }
            },
            {
                Published,
                new ModuleCollection
                {
                    new Meta("FrontMatterPublished", (doc, ctx) => doc.ContainsKey(BookSiteKeys.Published)),  // Record whether the publish date came from front matter
                    new Meta(BookSiteKeys.Published, (doc, ctx) =>
                    {
                        DateTime published;
                        if (!ctx.TryParseInputDateTime(doc.String(Keys.SourceFileName).Substring(0, 10), out published))
                        {
                            Common.Tracing.Trace.Warning($"Could not parse published date for {doc.SourceString()}.");
                            return null;
                        }
                        return published;
                    }).OnlyIfNonExisting(),
                    new Where((doc, ctx) =>
                    {
                        if (!doc.ContainsKey(BookSiteKeys.Published) || doc.Get(BookSiteKeys.Published) == null)
                        {
                            Common.Tracing.Trace.Warning($"Skipping {doc.SourceString()} due to not having {BookSiteKeys.Published} metadata");
                            return false;
                        }
                        if (doc.Get<DateTime>(BookSiteKeys.Published) > DateTime.Now)
                        {
                            Common.Tracing.Trace.Warning($"Skipping {doc.SourceString()} due to having {BookSiteKeys.Published} metadata of {doc.Get<DateTime>(BookSiteKeys.Published)} in the future (current date and time is {DateTime.Now})");
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
                    DateTime published = doc.Get<DateTime>(BookSiteKeys.Published);
                    string fileName = doc.Bool("FrontMatterPublished")
                        ? doc.FilePath(Keys.SourceFileName).ChangeExtension("html").FullPath
                        : doc.FilePath(Keys.SourceFileName).ChangeExtension("html").FullPath.Substring(11);
                    return ctx.Bool(BookSiteKeys.IncludeDateInPostPath)
                        ? $"{ctx.DirectoryPath(BookSiteKeys.PostsPath).FullPath}/{published:yyyy}/{published:MM}/{fileName}"
                        : $"{ctx.DirectoryPath(BookSiteKeys.PostsPath).FullPath}/{fileName}";
                })
            },
            {
                OrderByPublished,
                new OrderBy((doc, ctx) => doc.Get<DateTime>(BookSiteKeys.Published)).Descending()
            }
        };
    }
}
