using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Configuration;
using Wyam.Common.Execution;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Util;
using Wyam.Core.Modules.Contents;
using Wyam.Core.Modules.Control;
using Wyam.Core.Modules.Extensibility;
using Wyam.Core.Modules.IO;
using Wyam.Core.Modules.Metadata;
using Wyam.Html;

namespace Wyam.Web.Pipelines
{
    /// <summary>
    /// Loads blog posts from Markdown and/or Razor files.
    /// </summary>
    public class BlogPosts : Pipeline
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
        /// Writes metadata such as the excerpt.
        /// </summary>
        public const string WriteMetadata = nameof(WriteMetadata);

        /// <summary>
        /// Sets the relative file path for each post in metadata.
        /// </summary>
        public const string RelativeFilePath = nameof(RelativeFilePath);

        /// <summary>
        /// Orders the posts by their published date.
        /// </summary>
        public const string OrderByPublished = nameof(OrderByPublished);

        /// <summary>
        /// Creates the pipeline.
        /// </summary>
        /// <param name="name">The name of this pipeline.</param>
        /// <param name="settings">The settings for the pipeline.</param>
        public BlogPosts(string name, BlogPostsSettings settings)
            : base(name, GetModules(settings))
        {
        }

        private static IModuleList GetModules(BlogPostsSettings settings) => new ModuleList
        {
            {
                MarkdownPosts,
                new ModuleCollection
                {
                    new ReadFiles(ctx => $"{settings.PostsPath.Invoke<string>(ctx)}/**/{{!_,}}*.md"),
                    new Meta(WebKeys.EditFilePath, (doc, ctx) => doc.FilePath(Keys.RelativeFilePath)),
                    new If(settings.ProcessIncludes, new Include()),
                    new FrontMatter(new Yaml.Yaml()),
                    new Execute(ctx => new Markdown.Markdown()
                        .UseConfiguration(settings.MarkdownConfiguration.Invoke<string>(ctx))
                        .UseExtensions(settings.MarkdownExtensionTypes.Invoke<IEnumerable<Type>>(ctx))
                        .PrependLinkRoot(settings.PrependLinkRoot.Invoke<bool>(ctx)))
                }
            },
            {
                RazorPosts,
                new Concat
                {
                    new ReadFiles(ctx => $"{settings.PostsPath.Invoke<string>(ctx)}/{{!_,!index,}}*.cshtml"),
                    new Meta(WebKeys.EditFilePath, (doc, ctx) => doc.FilePath(Keys.RelativeFilePath)),
                    new If(settings.ProcessIncludes, new Include()),
                    new FrontMatter(new Yaml.Yaml())
                }
            },
            {
                WriteMetadata,
                new Excerpt()
            },
            {
                Published,
                new ModuleCollection
                {
                    new If(
                        (doc, ctx) => doc.ContainsKey(settings.PublishedKey) && ctx.TryParseInputDateTime(doc.String(settings.PublishedKey), out _),
                        new Meta("FrontMatterPublished", (doc, ctx) => true)) // Record whether the publish date came from front matter
                        .Else(
                            new Meta(settings.PublishedKey, (doc, ctx) =>
                            {
                                DateTime published;
                                if (doc.String(Keys.SourceFileName).Length >= 10 && ctx.TryParseInputDateTime(doc.String(Keys.SourceFileName).Substring(0, 10), out published))
                                {
                                    return published;
                                }
                                Common.Tracing.Trace.Warning($"Could not parse published date for {doc.SourceString()}.");
                                return null;
                            })),
                    new Where((doc, ctx) =>
                    {
                        if (!doc.ContainsKey(settings.PublishedKey) || doc.Get(settings.PublishedKey) == null)
                        {
                            Common.Tracing.Trace.Warning($"Skipping {doc.SourceString()} due to not having {settings.PublishedKey} metadata");
                            return false;
                        }
                        if (doc.Get<DateTime>(settings.PublishedKey) > DateTime.Now)
                        {
                            Common.Tracing.Trace.Warning($"Skipping {doc.SourceString()} due to having {settings.PublishedKey} metadata of {doc.Get<DateTime>(settings.PublishedKey)} in the future (current date and time is {DateTime.Now})");
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
                    DateTime published = doc.Get<DateTime>(settings.PublishedKey);
                    string fileName = doc.Bool("FrontMatterPublished")
                        ? doc.FilePath(Keys.SourceFileName).ChangeExtension("html").FullPath
                        : doc.FilePath(Keys.SourceFileName).ChangeExtension("html").FullPath.Substring(11);
                    return settings.IncludeDateInPostPath.Invoke<bool>(ctx)
                        ? $"{settings.PostsPath.Invoke<string>(ctx)}/{published:yyyy}/{published:MM}/{fileName}"
                        : $"{settings.PostsPath.Invoke<string>(ctx)}/{fileName}";
                })
            },
            {
                OrderByPublished,
                new OrderBy((doc, ctx) => doc.Get<DateTime>(settings.PublishedKey)).Descending()
            }
        };
    }
}
