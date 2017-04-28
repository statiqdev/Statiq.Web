using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Core.Modules.Control;
using Wyam.Core.Modules.Extensibility;
using Wyam.Core.Modules.IO;
using Wyam.Core.Modules.Metadata;

namespace Wyam.Web.Pipelines
{
    /// <summary>
    /// Generates an optionally grouped and paged archive for a set of documents from defined pipelines.
    /// </summary>
    public class Archive : Pipeline
    {
        /// <summary>
        /// Reads the common index file and processes it's front matter.
        /// </summary>
        public const string ReadFile = nameof(ReadFile);

        /// <summary>
        /// Populates the documents and performs grouping and pagination (if appropriate).
        /// </summary>
        public const string Populate = nameof(Populate);

        /// <summary>
        /// Renders each index document.
        /// </summary>
        public const string Render = nameof(Render);

        /// <summary>
        /// Writes the documents to the file system.
        /// </summary>
        public const string WriteFiles = nameof(WriteFiles);

        /// <summary>
        /// Creates the pipeline.
        /// </summary>
        /// <param name="name">The name of this pipeline.</param>
        /// <param name="pipelines">The name of the pipeline that contains the posts.</param>
        /// <param name="file">The relative path to the index file template.</param>
        /// <param name="layout">The layout to use for each index file.</param>
        /// <param name="group">A delegate to use for grouping documents or <c>null</c> if no grouping should be performed.</param>
        /// <param name="caseInsensitiveGroupComparer">
        /// A delegate that should return <c>true</c> the use the case-insensitive group key comparer,
        /// <c>false</c> or <c>null</c> to use the default comparer (including for non-string group keys).
        /// </param>
        /// <param name="pageSize">A delegate to get the page size. If <c>null</c>, no paging will be used.</param>
        /// <param name="sort">Sorts the documents before generating the archive pages. If <c>null</c> the documents will maintain the order of their source pipeline(s).</param>
        /// <param name="title">A delegate to get the title of each page.</param>
        /// <param name="relativePath">
        /// A delegate to get the relative output path of each page. If the result contains a ".html" extension, the page
        /// number will be appended to the result file name, otherwise if no ".html" extension is in the result value then
        /// it will be considered a folder path and the first page will be output as "index.html" followed by "page2.html", etc.
        /// </param>
        /// <param name="groupDocumentsMetadataKey">An additional metadata key to store the group documents in, or <c>null</c> not to store them.</param>
        /// <param name="groupKeyMetadataKey">A metadata key to store the group key in, or <c>null</c> not to store it.</param>
        public Archive(
            string name,
            string[] pipelines,
            string file,
            string layout,
            DocumentConfig group,
            ContextConfig caseInsensitiveGroupComparer,
            ContextConfig pageSize,
            Comparison<IDocument> sort,
            DocumentConfig title,
            DocumentConfig relativePath,
            string groupDocumentsMetadataKey,
            string groupKeyMetadataKey)
            : base(
                name,
                GetModules(
                    pipelines,
                    file,
                    layout,
                    group,
                    caseInsensitiveGroupComparer,
                    pageSize,
                    sort,
                    title,
                    relativePath,
                    groupDocumentsMetadataKey,
                    groupKeyMetadataKey))
        {
        }

        private static IModuleList GetModules(
            string[] pipelines,
            string file,
            string layout,
            DocumentConfig group,
            ContextConfig caseInsensitiveGroupComparer,
            ContextConfig pageSize,
            Comparison<IDocument> sort,
            DocumentConfig title,
            DocumentConfig relativePath,
            string groupDocumentsMetadataKey,
            string groupKeyMetadataKey) => new ModuleList
        {
            {
                ReadFile,
                new ModuleCollection
                {
                    new ReadFiles(file),
                    new FrontMatter(new Yaml.Yaml())
                }
            },
            {
                Populate,
                group != null
                    ? new ModuleCollection
                    {
                        new Execute(ctx =>
                            new GroupByMany(group, new Documents().FromPipelines(pipelines))
                                .WithComparer(caseInsensitiveGroupComparer != null && caseInsensitiveGroupComparer.Invoke<bool>(ctx) ? StringComparer.OrdinalIgnoreCase : null)),
                        new Where((doc, ctx) => !string.IsNullOrEmpty(doc.String(Keys.GroupKey))),
                        new ForEach((IModule)GetIndexPageModules(
                            new Documents((doc, _) => doc[Keys.GroupDocuments]),
                            pageSize,
                            sort,
                            title,
                            relativePath,
                            groupDocumentsMetadataKey,
                            groupKeyMetadataKey))
                    }
                    : GetIndexPageModules(
                        new Documents().FromPipelines(pipelines),
                        pageSize,
                        sort,
                        title,
                        relativePath,
                        groupDocumentsMetadataKey,
                        groupKeyMetadataKey)
            },
            {
                Render,
                new Razor.Razor()
                    .IgnorePrefix(null)
                    .WithLayout(layout)
            },
            {
                WriteFiles,
                new WriteFiles()
            }
        };

        private static ModuleCollection GetIndexPageModules(
            Documents indexDocuments,
            ContextConfig pageSize,
            Comparison<IDocument> sort,
            DocumentConfig title,
            DocumentConfig relativePath,
            string groupDocumentsMetadataKey,
            string groupKeyMetadataKey)
        {
            IModule[] paginateModules = sort == null
                ? new IModule[] { indexDocuments }
                : new IModule[] { indexDocuments, new Sort(sort) };
            return new ModuleCollection
            {
                new Execute(ctx => new Paginate(pageSize?.Invoke<int>(ctx) ?? int.MaxValue, paginateModules)),
                new If(
                    (doc, ctx) => doc.ContainsKey(Keys.TotalItems),
                    new Meta(Keys.Title, (doc, ctx) =>
                    {
                        string indexTitle = title.Invoke<string>(doc, ctx);
                        return doc.Get<int>(Keys.CurrentPage) <= 1
                            ? indexTitle
                            : $"{indexTitle} (Page {doc[Keys.CurrentPage]})";
                    }),
                    string.IsNullOrEmpty(groupDocumentsMetadataKey)
                        ? null
                        : new Meta(groupDocumentsMetadataKey, (doc, ctx) => doc[Keys.GroupDocuments]),
                    string.IsNullOrEmpty(groupKeyMetadataKey)
                        ? null
                        : new Meta(groupKeyMetadataKey, (doc, ctx) => doc.String(Keys.GroupKey)),
                    new Meta(Keys.RelativeFilePath, (doc, ctx) =>
                    {
                        string path = relativePath.Invoke<string>(doc, ctx)
                            .ToLowerInvariant()
                            .Replace(' ', '-')
                            .Replace("'", string.Empty)
                            .Replace(".", string.Empty);
                        return doc.Get<int>(Keys.CurrentPage) <= 1
                            ? (path.EndsWith(".html", StringComparison.OrdinalIgnoreCase)
                                ? path
                                : $"{path}/index.html")
                            : (path.EndsWith(".html", StringComparison.OrdinalIgnoreCase)
                                ? path.Insert(path.Length - 5, doc.String(Keys.CurrentPage))
                                : $"{path}/page{doc.String(Keys.CurrentPage)}.html");
                    }))
                    .WithoutUnmatchedDocuments()
            };
        }
    }
}
