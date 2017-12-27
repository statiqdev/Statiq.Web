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
        /// <param name="settings">The settings for the pipeline.</param>
        public Pages(string name, PagesSettings settings)
            : base(name, GetModules(settings))
        {
        }

        private static IModuleList GetModules(PagesSettings settings)
        {
            ModuleList moduleList = new ModuleList
            {
                {
                    MarkdownFiles,
                    new ModuleCollection
                    {
                        new ReadFiles(ctx => GetGlobbingPattern(ctx, settings.PagesPattern, settings.IgnorePaths, "md")),
                        new Meta(WebKeys.EditFilePath, (doc, ctx) => doc.FilePath(Keys.RelativeFilePath)),
                        new If(settings.ProcessIncludes, new Include()),
                        new FrontMatter(new Yaml.Yaml()),
                        new Execute(ctx => new Markdown.Markdown()
                            .UseConfiguration(settings.MarkdownConfiguration.Invoke<string>(ctx))
                            .UseExtensions(settings.MarkdownExtensionTypes.Invoke<IEnumerable<Type>>(ctx)))
                    }
                },
                {
                    RazorFiles,
                    new Concat
                    {
                        new ReadFiles(
                            ctx => GetGlobbingPattern(ctx, settings.PagesPattern, settings.IgnorePaths, "cshtml")),
                        new Meta(WebKeys.EditFilePath, (doc, ctx) => doc.FilePath(Keys.RelativeFilePath)),
                        new If(settings.ProcessIncludes, new Include()),
                        new FrontMatter(new Yaml.Yaml())
                    }.Where((ctx, doc, inputs) => !inputs.Any(x =>
                        string.Equals(
                            x.FilePath(Keys.RelativeFilePath).ChangeExtension(null).FullPath,
                            doc.FilePath(Keys.RelativeFilePath).ChangeExtension(null).FullPath,
                            StringComparison.OrdinalIgnoreCase))) // Don't overwrite existing documents
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
            Comparison<IDocument> sort = settings.Sort
                ?? ((x, y) => Comparer.Default.Compare(x.String(Keys.Title), y.String(Keys.Title)));
            if (settings.CreateTree)
            {
                Tree tree = settings.TreePlaceholderFactory == null
                    ? new Tree().WithNesting(true, true)
                    : new Tree().WithNesting(true, true).WithPlaceholderFactory(settings.TreePlaceholderFactory);
                tree.WithSort(sort);
                moduleList.Add(CreateTreeAndSort, tree);
            }
            else
            {
                moduleList.Add(CreateTreeAndSort, new Sort(sort));
            }
            return moduleList;
        }

        private static string GetGlobbingPattern(IExecutionContext context, ContextConfig pagesPattern, ContextConfig ignorePaths, string extension)
        {
            List<string> segments = new List<string>();
            IEnumerable<string> ignorePatterns = ignorePaths?.Invoke<IEnumerable<string>>(context).Select(x => "!" + x);
            if (ignorePatterns != null)
            {
                segments.AddRange(ignorePatterns);
            }
            segments.Add($"{(pagesPattern == null ? string.Empty : pagesPattern.Invoke(context) + "/")}**/{{!.git,}}/**/{{!_,}}*.{extension}");
            return segments.Count == 1 ? segments[0] : $"{{{string.Join(",", segments)}}}";
        }
    }
}
