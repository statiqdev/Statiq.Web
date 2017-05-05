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
        /// <param name="publishedKey">The metadata key where <see cref="DateTime"/> published dates are stored for each post.</param>
        /// <param name="markdownConfiguration">A delegate that returns the string configuration for the Markdown processor.</param>
        /// <param name="markdownExtensionTypes">A delegate that returns a sequence of <see cref="Type"/> for Markdown extensions.</param>
        /// <param name="includeDateInPostPath">A delegate that returns a <see cref="bool"/> indicating if post dates should be included in the output path.</param>
        /// <param name="postsPath">A delegate that should return a <see cref="string"/> with the path to blog post files.</param>
        public BlogPosts(
            string name,
            string publishedKey,
            ContextConfig markdownConfiguration,
            ContextConfig markdownExtensionTypes,
            ContextConfig includeDateInPostPath,
            ContextConfig postsPath)
            : base(name, GetModules(publishedKey, markdownConfiguration, markdownExtensionTypes, includeDateInPostPath, postsPath))
        {
        }

        private static IModuleList GetModules(
            string publishedKey,
            ContextConfig markdownConfiguration,
            ContextConfig markdownExtensionTypes,
            ContextConfig includeDateInPostPath,
            ContextConfig postsPath) => new ModuleList
        {
            {
                MarkdownPosts,
                new ModuleCollection
                {
                    new ReadFiles(ctx => $"{postsPath.Invoke<string>(ctx)}/{{!_,}}*.md"),
                    new Meta(WebKeys.EditFilePath, (doc, ctx) => doc.FilePath(Keys.RelativeFilePath)),
                    new Include(),
                    new FrontMatter(new Yaml.Yaml()),
                    new Execute(ctx => new Markdown.Markdown()
                        .UseConfiguration(markdownConfiguration.Invoke<string>(ctx))
                        .UseExtensions(markdownExtensionTypes.Invoke<IEnumerable<Type>>(ctx)))
                }
            },
            {
                RazorPosts,
                new Concat
                {
                    new ReadFiles(ctx => $"{postsPath.Invoke<string>(ctx)}/{{!_,!index,}}*.cshtml"),
                    new Meta(WebKeys.EditFilePath, (doc, ctx) => doc.FilePath(Keys.RelativeFilePath)),
                    new Include(),
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
                    new Meta("FrontMatterPublished", (doc, ctx) => doc.ContainsKey(publishedKey)),  // Record whether the publish date came from front matter
                    new Meta(publishedKey, (doc, ctx) =>
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
                        if (!doc.ContainsKey(publishedKey) || doc.Get(publishedKey) == null)
                        {
                            Common.Tracing.Trace.Warning($"Skipping {doc.SourceString()} due to not having {publishedKey} metadata");
                            return false;
                        }
                        if (doc.Get<DateTime>(publishedKey) > DateTime.Now)
                        {
                            Common.Tracing.Trace.Warning($"Skipping {doc.SourceString()} due to having {publishedKey} metadata of {doc.Get<DateTime>(publishedKey)} in the future (current date and time is {DateTime.Now})");
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
                    DateTime published = doc.Get<DateTime>(publishedKey);
                    string fileName = doc.Bool("FrontMatterPublished")
                        ? doc.FilePath(Keys.SourceFileName).ChangeExtension("html").FullPath
                        : doc.FilePath(Keys.SourceFileName).ChangeExtension("html").FullPath.Substring(11);
                    return includeDateInPostPath.Invoke<bool>(ctx)
                        ? $"{postsPath.Invoke<string>(ctx)}/{published:yyyy}/{published:MM}/{fileName}"
                        : $"{postsPath.Invoke<string>(ctx)}/{fileName}";
                })
            },
            {
                OrderByPublished,
                new OrderBy((doc, ctx) => doc.Get<DateTime>(publishedKey)).Descending()
            }
        };
    }
}
