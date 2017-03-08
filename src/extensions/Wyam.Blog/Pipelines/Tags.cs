using System;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Core.Modules.Control;
using Wyam.Core.Modules.Extensibility;
using Wyam.Core.Modules.IO;
using Wyam.Core.Modules.Metadata;

namespace Wyam.Blog.Pipelines
{
    /// <summary>
    /// Generates tag groups from the tags on blog posts.
    /// </summary>
    public class Tags : Pipeline
    {
        /// <summary>
        /// Reads the common tag file and processes it's front matter.
        /// </summary>
        public const string TagFile = nameof(TagFile);

        /// <summary>
        /// Gets a document for each tag.
        /// </summary>
        public const string TagDocuments = nameof(TagDocuments);

        /// <summary>
        /// Sets the relative file path for each tag document in metadata.
        /// </summary>
        public const string RelativeFilePath = nameof(RelativeFilePath);

        /// <summary>
        /// Renders each tag document.
        /// </summary>
        public const string Render = nameof(Render);

        /// <summary>
        /// Writes the documents to the file system.
        /// </summary>
        public const string WriteFiles = nameof(WriteFiles);

        internal Tags()
            : base(GetModules())
        {
        }

        private static ModuleList GetModules() => new ModuleList
        {
            {
                TagFile,
                new ModuleCollection
                {
                    new ReadFiles("tags/tag.cshtml"),
                    new FrontMatter(new Yaml.Yaml())
                }
            },
            {
                TagDocuments,
                new ModuleCollection
                {
                    new Execute(ctx =>
                        new GroupByMany(BlogKeys.Tags, new Documents(Blog.RawPosts))
                            .WithComparer(ctx.Bool(BlogKeys.CaseInsensitiveTags) ? StringComparer.OrdinalIgnoreCase : null)),
                    new Where((doc, ctx) => !string.IsNullOrEmpty(doc.String(Keys.GroupKey))),
                    new Meta(BlogKeys.Tag, (doc, ctx) => doc.String(Keys.GroupKey)),
                    new Meta(BlogKeys.Title, (doc, ctx) => doc.String(Keys.GroupKey)),
                    new Meta(BlogKeys.Posts, (doc, ctx) => doc.List<IDocument>(Keys.GroupDocuments))
                }
            },
            {
                RelativeFilePath,
                new Meta(Keys.RelativeFilePath, (doc, ctx) =>
                {
                    string tag = doc.String(Keys.GroupKey);
                    return $"tags/{(tag.StartsWith(".") ? tag.Substring(1) : tag).ToLowerInvariant().Replace(' ', '-')}.html";
                })
            },
            {
                Render,
                new Razor.Razor()
                    .WithLayout("/_Layout.cshtml")
            },
            {
                WriteFiles,
                new WriteFiles()
            }
        };
    }
}