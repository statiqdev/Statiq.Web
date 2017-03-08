using System;
using Wyam.Common.Configuration;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Core.Modules.Control;
using Wyam.Core.Modules.Extensibility;
using Wyam.Core.Modules.IO;
using Wyam.Core.Modules.Metadata;

namespace Wyam.Blog.Pipelines
{
    /// <summary>
    /// Loads page content from Markdown and/or Razor files so that it's available
    /// to following pipelines like the RSS feed generator. This does not render the
    /// pages or write them to disk (that is performed in following pipelines).
    /// </summary>
    public class Pages : Pipeline
    {
        /// <summary>
        /// Reads all markdown files, processes their front matter, and renders them to HTML.
        /// </summary>
        public const string MarkdownFiles = nameof(MarkdownFiles);

        /// <summary>
        /// Reads all Razor files and processes their front matter (but does not render them to HTML).
        /// </summary>
        public const string RazorFiles = nameof(RazorFiles);

        /// <summary>
        /// Reads the posts index file and processes it's front matter.
        /// </summary>
        public const string PostsIndex = nameof(PostsIndex);

        /// <summary>
        /// Reads the tags index file and processes it's front matter.
        /// </summary>
        public const string TagsIndex = nameof(TagsIndex);

        /// <summary>
        /// Copy the index page image and header text color from settings metadata (if set) to the document metadata.
        /// </summary>
        public const string HomePageStyles = nameof(HomePageStyles);

        /// <summary>
        /// Writes the file metadata to the documents (such as relative output path).
        /// </summary>
        public const string WriteFileMetadata = nameof(WriteFileMetadata);

        internal Pages()
            : base(GetModules())
        {
        }

        private static ModuleList GetModules() => new ModuleList
        {
            {
                MarkdownFiles,
                new ModuleCollection
                {
                    new ReadFiles(ctx => $"{{!{ctx.DirectoryPath(BlogKeys.PostsPath).FullPath},**}}/*.md"),
                    new FrontMatter(new Yaml.Yaml()),
                    new Execute(ctx => new Markdown.Markdown()
                        .UseExtensions(ctx.Settings.List<Type>(BlogKeys.MarkdownExternalExtensions))
                        .UseConfiguration(ctx.String(BlogKeys.MarkdownExtensions)))
                }
            },
            {
                RazorFiles,
                new Concat
                {
                    new ReadFiles(
                        ctx => $"{{!{ctx.DirectoryPath(BlogKeys.PostsPath).FullPath},!tags,**}}/*.cshtml"),
                    new FrontMatter(new Yaml.Yaml())
                }
            },
            {
                PostsIndex,
                new Concat
                {
                    new ReadFiles("posts/index.cshtml"),
                    new FrontMatter(new Yaml.Yaml()),
                    new Meta(
                        Keys.RelativeFilePath,
                        ctx => ctx.DirectoryPath(BlogKeys.PostsPath).CombineFile("index.cshtml"))
                }
            },
            {
                TagsIndex,
                new Concat
                {
                    new ReadFiles("tags/index.cshtml"),
                    new FrontMatter(new Yaml.Yaml())
                }
            },
            {
                HomePageStyles,
                new ModuleCollection
                {
                    new If(
                        (doc, ctx) => doc.FilePath(Keys.RelativeFilePath).Equals(new FilePath("index.cshtml"))
                                      && ctx.ContainsKey(BlogKeys.Image),
                        new Meta(BlogKeys.Image, ctx => ctx[BlogKeys.Image])),
                    new If(
                        (doc, ctx) => doc.FilePath(Keys.RelativeFilePath).Equals(new FilePath("index.cshtml"))
                                      && ctx.ContainsKey(BlogKeys.HeaderTextColor),
                        new Meta(BlogKeys.HeaderTextColor, ctx => ctx[BlogKeys.HeaderTextColor]))
                }
            },
            {
                WriteFileMetadata,
                new WriteFiles(".html")
                    .OnlyMetadata()
            }
        };
    }
}