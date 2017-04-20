using System;
using System.Collections.Concurrent;
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
using Wyam.Common.Util;
using Wyam.Core.Modules.Contents;
using Wyam.Core.Modules.Control;
using Wyam.Core.Modules.Extensibility;
using Wyam.Core.Modules.IO;
using Wyam.Core.Modules.Metadata;
using Wyam.Html;

namespace Wyam.WebRecipe.Pipelines
{
    /// <summary>
    /// Loads documentation content from Markdown and/or Razor files.
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
        /// Writes the file and other metadata to the documents (such as relative output path).
        /// </summary>
        public const string WriteMetadata = nameof(WriteMetadata);

        /// <summary>
        /// Creates a tree structure from the pages.
        /// </summary>
        public const string CreateTree = nameof(CreateTree);

        /// <summary>
        /// Creates the pipeline.
        /// </summary>
        /// <param name="ignoreFolders">
        /// A delegate that should return a <see cref="string"/>
        /// or <c>IEnumerable&lt;string&gt;</c> with ignore paths.
        /// </param>
        /// <param name="treePlaceholderFactory">
        /// A factory to use for creating tree placeholders at points in the tree where no actual pages were found.
        /// If <c>null</c>, the default placeholder factory will be used which outputs empty index files.
        /// </param>
        public Pages(ContextConfig ignoreFolders, Func<object[], MetadataItems, IExecutionContext, IDocument> treePlaceholderFactory)
            : base(GetModules(ignoreFolders, treePlaceholderFactory))
        {
        }

        private static IModuleList GetModules(ContextConfig ignoreFolders, Func<object[], MetadataItems, IExecutionContext, IDocument> treePlaceholderFactory) => new ModuleList
        {
            {
                MarkdownFiles,
                new ModuleCollection
                {
                    new ReadFiles(ctx => $"{{{GetIgnoreFoldersGlob(ctx, ignoreFolders)}}}/*.md"),
                    new Meta(WebRecipeKeys.EditFilePath, (doc, ctx) => doc.FilePath(Keys.RelativeFilePath)),
                    new Include(),
                    new FrontMatter(new Yaml.Yaml()),
                    new Execute(ctx => new Markdown.Markdown()
                        .UseExtensions(ctx.Settings.List<Type>(WebRecipeKeys.MarkdownExternalExtensions))
                        .UseConfiguration(ctx.String(WebRecipeKeys.MarkdownExtensions)))
                }
            },
            {
                RazorFiles,
                new Concat
                {
                    new ReadFiles(ctx => $"{{{GetIgnoreFoldersGlob(ctx, ignoreFolders)}}}/{{!_,}}*.cshtml"), // Add any additional Razor pages
                    new Include(),
                    new FrontMatter(new Yaml.Yaml())
                }
            },
            {
                WriteMetadata,
                new ModuleCollection
                {
                    new Excerpt(),
                    new Title(),
                    new WriteFiles(".html").OnlyMetadata()
                }
            },
            {
                CreateTree,
                treePlaceholderFactory == null
                    ? new Tree().WithNesting(true, true)
                    : new Tree().WithNesting(true, true).WithPlaceholderFactory(treePlaceholderFactory)
            }
        };

        private static string GetIgnoreFoldersGlob(IExecutionContext context, ContextConfig ignoreFolders) =>
            string.Join(",", context
                .List(WebRecipeKeys.IgnoreFolders, Array.Empty<string>())
                .Concat(ignoreFolders.Invoke<IEnumerable<string>>(context))
                .Select(x => "!" + x)
                .Concat(new[] { "**" }));
    }
}
