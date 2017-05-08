using System;
using System.Collections;
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

namespace Wyam.Web.Pipelines
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
        /// Creates a tree structure from the pages and/or sorts the pages.
        /// </summary>
        public const string CreateTreeAndSort = nameof(CreateTreeAndSort);

        /// <summary>
        /// Creates the pipeline.
        /// </summary>
        /// <param name="name">The name of this pipeline.</param>
        /// <param name="pagesPattern">
        /// A delegate that should return a <see cref="string"/> with the glob to pages.
        /// If <c>null</c>, a default globbing pattern of "**" is used.
        /// </param>
        /// <param name="ignoreFolders">
        /// A delegate that should return a <see cref="string"/>
        /// or <c>IEnumerable&lt;string&gt;</c> with ignore paths.
        /// If the delegate is <c>null</c>, no paths will be ignored.
        /// </param>
        /// <param name="markdownConfiguration">A delegate that returns the string configuration for the Markdown processor.</param>
        /// <param name="markdownExtensionTypes">A delegate that returns a sequence of <see cref="Type"/> for Markdown extensions.</param>
        /// <param name="processIncludes">A delegate that returns a <see cref="bool"/> indicating if documents should be processed with the <see cref="Include"/> module.</param>
        /// <param name="sort">Sorts the documents based on a comparison. If <c>null</c>, the sorting will be based on the document title.</param>
        /// <param name="createTree"><c>true</c> to create a tree from the pages, <c>false</c> to leave the pages flat.</param>
        /// <param name="treePlaceholderFactory">
        /// A factory to use for creating tree placeholders at points in the tree where no actual pages were found.
        /// If <c>null</c>, the default placeholder factory will be used which outputs empty index files.
        /// </param>
        public Pages(
            string name,
            ContextConfig pagesPattern,
            ContextConfig ignoreFolders,
            ContextConfig markdownConfiguration,
            ContextConfig markdownExtensionTypes,
            DocumentConfig processIncludes,
            Comparison<IDocument> sort,
            bool createTree,
            Func<object[], MetadataItems, IExecutionContext, IDocument> treePlaceholderFactory)
            : base(name, GetModules(pagesPattern, ignoreFolders, markdownConfiguration, markdownExtensionTypes, processIncludes, sort, createTree, treePlaceholderFactory))
        {
        }

        private static IModuleList GetModules(
            ContextConfig pagesPath,
            ContextConfig ignoreFolders,
            ContextConfig markdownConfiguration,
            ContextConfig markdownExtensionTypes,
            DocumentConfig processIncludes,
            Comparison<IDocument> sort,
            bool createTree,
            Func<object[], MetadataItems, IExecutionContext, IDocument> treePlaceholderFactory)
        {
            ModuleList moduleList = new ModuleList
            {
                {
                    MarkdownFiles,
                    new ModuleCollection
                    {
                        new ReadFiles(ctx => $"{GetIgnoreFoldersGlob(ctx, pagesPath, ignoreFolders)}/{{!_,}}*.md"),
                        new Meta(WebKeys.EditFilePath, (doc, ctx) => doc.FilePath(Keys.RelativeFilePath)),
                        new If(processIncludes, new Include()),
                        new FrontMatter(new Yaml.Yaml()),
                        new Execute(ctx => new Markdown.Markdown()
                            .UseConfiguration(markdownConfiguration.Invoke<string>(ctx))
                            .UseExtensions(markdownExtensionTypes.Invoke<IEnumerable<Type>>(ctx)))
                    }
                },
                {
                    RazorFiles,
                    new Concat
                    {
                        new ReadFiles(
                            ctx => $"{GetIgnoreFoldersGlob(ctx, pagesPath, ignoreFolders)}/{{!_,}}*.cshtml"),
                        new Meta(WebKeys.EditFilePath, (doc, ctx) => doc.FilePath(Keys.RelativeFilePath)),
                        new If(processIncludes, new Include()),
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
                }
            };

            // Tree and sort
            if (sort == null)
            {
                sort = (x, y) => Comparer.Default.Compare(x.String(Keys.Title), y.String(Keys.Title));
            }
            if (createTree)
            {
                Tree tree = treePlaceholderFactory == null
                    ? new Tree().WithNesting(true, true)
                    : new Tree().WithNesting(true, true).WithPlaceholderFactory(treePlaceholderFactory);
                tree.WithSort(sort);
                moduleList.Add(CreateTreeAndSort, tree);
            }
            else
            {
                moduleList.Add(CreateTreeAndSort, new Sort(sort));
            }
            return moduleList;
        }

        private static string GetIgnoreFoldersGlob(IExecutionContext context, ContextConfig pagesPattern, ContextConfig ignoreFolders)
        {
            string[] segments =
                (ignoreFolders?.Invoke<IEnumerable<string>>(context).Select(x => "!" + x) ?? Array.Empty<string>())
                .Concat(new[] { pagesPattern == null ? "**" : pagesPattern.Invoke<string>(context) })
                .ToArray();
            return segments.Length == 1 ? segments[0] : $"{{{string.Join(",", segments)}}}";
        }
    }
}
