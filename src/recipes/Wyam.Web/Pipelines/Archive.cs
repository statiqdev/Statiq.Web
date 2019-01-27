using System;
using System.Linq;
using Wyam.Common.Configuration;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Core.Modules.Control;
using Wyam.Core.Modules.Extensibility;
using Wyam.Core.Modules.IO;

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
        /// <param name="settings">The settings for the pipeline.</param>
        public Archive(string name, ArchiveSettings settings)
            : base(name, GetModules(settings))
        {
        }

        private static IModuleList GetModules(ArchiveSettings settings) => new ModuleList
        {
            {
                ReadFile,
                new ModuleCollection
                {
                    new ReadFiles(settings.TemplateFile),
                    new FrontMatter(new Yaml.Yaml())
                }
            },
            {
                Populate,
                settings.Group != null
                    ? new ModuleCollection
                    {
                        new Execute(ctx =>
                            new GroupByMany(settings.Group, new Documents().FromPipelines(settings.Pipelines))
                                .WithComparer(settings.CaseInsensitiveGroupComparer?.Invoke<bool>(ctx) == true ? StringComparer.OrdinalIgnoreCase : null)
                                .WithEmptyOutputIfNoGroups()),
                        new Where((doc, ctx) => !string.IsNullOrEmpty(doc.String(Keys.GroupKey))),
                        new ForEach((IModule)GetIndexPageModules(
                            new Documents((doc, _) => doc[Keys.GroupDocuments]),
                            settings))
                    }
                    : GetIndexPageModules(
                        new Documents().FromPipelines(settings.Pipelines),
                        settings)
            },
            {
                Render,
                new If(
                    ctx => settings.WriteIfEmpty || settings.Pipelines.Any(x => ctx.Documents[x].Any()),
                    new Razor.Razor()
                        .IgnorePrefix(null)
                        .WithLayout(settings.Layout))
                        .WithoutUnmatchedDocuments()
            },
            {
                WriteFiles,
                new WriteFiles()
            }
        };

        private static ModuleCollection GetIndexPageModules(Documents documents, ArchiveSettings settings)
        {
            IModule[] paginateModules = settings.Sort == null
                ? new IModule[] { documents }
                : new IModule[] { documents, new Sort(settings.Sort) };
            return new ModuleCollection
            {
                new Execute(c =>
                {
                    Paginate paginate = new Paginate(settings.PageSize?.Invoke<int>(c) ?? int.MaxValue, paginateModules);
                    paginate = paginate.WithPageMetadata(Keys.Title, (doc, ctx) =>
                    {
                        string indexTitle = settings.Title?.Invoke<string>(doc, ctx) ?? string.Empty;
                        return doc.Get<int>(Keys.CurrentPage) <= 1
                            ? indexTitle
                            : (string.IsNullOrEmpty(indexTitle) ? $"Page {doc[Keys.CurrentPage]}" : $"{indexTitle} (Page {doc[Keys.CurrentPage]})");
                    });
                    if (settings.TakePages != null)
                    {
                        paginate = paginate.TakePages(settings.TakePages.Invoke<int>(c));
                    }
                    if (settings.SkipPages != null)
                    {
                        paginate = paginate.SkipPages(settings.SkipPages.Invoke<int>(c));
                    }
                    if (!string.IsNullOrEmpty(settings.GroupDocumentsMetadataKey))
                    {
                        paginate = paginate.WithPageMetadata(settings.GroupDocumentsMetadataKey, (doc, ctx) => doc[Keys.GroupDocuments]);
                    }
                    if (!string.IsNullOrEmpty(settings.GroupKeyMetadataKey))
                    {
                        paginate = paginate.WithPageMetadata(settings.GroupKeyMetadataKey, (doc, ctx) => doc.String(Keys.GroupKey));
                    }
                    paginate = paginate.WithPageMetadata(Keys.RelativeFilePath, (doc, ctx) =>
                    {
                        FilePath filePath = settings.RelativePath?.Invoke<FilePath>(doc, ctx) ?? settings.TemplateFile.Invoke<FilePath>(ctx);
                        bool hasExtension = filePath.HasExtension;
                        if (hasExtension)
                        {
                            filePath = filePath.Directory.CombineFile(filePath.FileNameWithoutExtension);
                        }
                        string path = filePath
                            .FullPath
                            .Replace(' ', '-')
                            .Replace("'", string.Empty)
                            .Replace(".", string.Empty);
                        return doc.Get<int>(Keys.CurrentPage) <= 1
                            ? (hasExtension ? $"{path}.html" : $"{path}/index.html")
                            : (hasExtension ? $"{path}{doc.String(Keys.CurrentPage)}.html" : $"{path}/page{doc.String(Keys.CurrentPage)}.html");
                    });
                    return paginate;
                })
            };
        }
    }
}
