using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Statiq.App;
using Statiq.Common;
using Statiq.Core;
using Statiq.Html;
using Statiq.Markdown;
using Statiq.Razor;
using Statiq.Yaml;

namespace Statiq.Web.Pipelines
{
    public class Archives : Pipeline
    {
        public Archives()
        {
            Dependencies.Add(nameof(Content));

            InputModules = new ModuleList
            {
                new ReadFiles("**/{!_,}*.{html,cshtml,md}")
            };

            ProcessModules = new ModuleList
            {
                new ProcessIncludes(),
                new ExtractFrontMatter(new ParseYaml()),
                new FilterDocuments(Config.FromDocument(IsArchive)),
                new ForEachDocument
                {
                    new ExecuteConfig(Config.FromDocument(archiveDoc =>
                    {
                        ModuleList modules = new ModuleList();

                        // Merge outputs from the pipeline(s) with the archive document so ArchiveFilter is on the actual candidate document
                        modules.Add(new MergeDocuments(archiveDoc.GetList(WebKeys.ArchivePipelines, new[] { nameof(Content) }).ToArray()).Reverse());

                        // Filter by document source
                        if (archiveDoc.ContainsKey(WebKeys.ArchiveSources))
                        {
                            modules.Add(new FilterSources(archiveDoc.GetList<string>(WebKeys.ArchiveSources)));
                        }

                        // Filter by metadata
                        if (archiveDoc.ContainsKey(WebKeys.ArchiveFilter))
                        {
                            modules.Add(new FilterDocuments(Config.FromDocument(doc => doc.GetBool(WebKeys.ArchiveFilter))));
                        }

                        // Order the documents
                        if (archiveDoc.ContainsKey(WebKeys.ArchiveOrderKey))
                        {
                            modules.Add(
                                new OrderDocuments(archiveDoc.GetString(WebKeys.ArchiveOrderKey))
                                    .Descending(archiveDoc.GetBool(WebKeys.ArchiveOrderDescending)));
                        }

                        // Are we making groups?
                        if (archiveDoc.ContainsKey(WebKeys.ArchiveKey))
                        {
                            // Group by the archive key
                            modules.Add(
                                new GroupDocuments(Config.FromDocument(doc => doc.GetList(WebKeys.ArchiveKey, new object[] { }))),
                                new MergeDocuments(Config.FromValue<IEnumerable<IDocument>>(new[] { archiveDoc })).Reverse());
                            modules.Add(GetTitleAndDestinationModules(archiveDoc)); // This will get rerun after pagination, but run it now so the top-level index has titles/destinations

                            // Process the groups
                            ModuleList groups = new ModuleList();
                            if (archiveDoc.ContainsKey(WebKeys.ArchivePageSize))
                            {
                                groups.Add(new ForEachDocument
                                {
                                    // Paginate the group
                                    new ExecuteConfig(Config.FromDocument(groupDoc => new ModuleList
                                    {
                                        new ReplaceDocuments(Config.FromDocument<IEnumerable<IDocument>>(doc => doc.GetDocumentList(Keys.Children))),
                                        new PaginateDocuments(archiveDoc.GetInt(WebKeys.ArchivePageSize)),
                                        new SetMetadata(Keys.GroupKey, groupDoc.Get(Keys.GroupKey)),
                                        new MergeDocuments(Config.FromValue<IEnumerable<IDocument>>(new[] { archiveDoc })).Reverse()
                                    }))
                                });
                            }
                            groups.Add(GetTitleAndDestinationModules(archiveDoc));

                            // Add the group modules and create a top-level index document in a branch
                            modules.Add(new ExecuteBranch(groups).Branch(GetTopLevelIndexModules(archiveDoc)));
                        }
                        else
                        {
                            // We weren't grouping so now we've got a sequence of documents

                            // Paginate the documents
                            if (archiveDoc.ContainsKey(WebKeys.ArchivePageSize))
                            {
                                modules.Add(
                                    new PaginateDocuments(archiveDoc.GetInt(WebKeys.ArchivePageSize)),
                                    new MergeDocuments(Config.FromValue<IEnumerable<IDocument>>(new[] { archiveDoc })).Reverse());
                                modules.Add(GetTitleAndDestinationModules(archiveDoc));
                            }
                            else
                            {
                                // We didn't make pages so create a top-level document
                                modules.Add(GetTopLevelIndexModules(archiveDoc));
                            }
                        }

                        // Render any markdown content
                        if (archiveDoc.MediaTypeEquals("text/markdown"))
                        {
                            modules.Add(new RenderMarkdown().UseExtensions());
                        }

                        return modules;
                    }))
                },
            };
        }

        private static IModule[] GetTopLevelIndexModules(IDocument archiveDoc) => new IModule[]
        {
            new ReplaceDocuments(Config.FromContext(ctx => archiveDoc.Clone(new MetadataItems { { Keys.Children, ctx.Inputs } }).Yield())),
            new AddTitle(),
            new SetDestination(Config.FromValue(archiveDoc.Destination.ChangeExtension(".html")), true)
        };

        private static IModule[] GetTitleAndDestinationModules(IDocument archiveDoc) => new IModule[]
        {
            new AddTitle(
                Config.FromDocument(doc =>
                {
                    if (doc.ContainsKey(WebKeys.ArchiveTitle))
                    {
                        return doc.GetString(WebKeys.ArchiveTitle);
                    }

                    // Default title
                    string title = doc.GetString(Keys.Title);
                    if (doc.ContainsKey(Keys.GroupKey))
                    {
                        title += " - " + doc.GetString(Keys.GroupKey);
                    }
                    int index = doc.GetInt(Keys.Index);
                    return index <= 1 ? title : (title + $" (Page {index})");
                })),
            new SetDestination(
                Config.FromDocument(doc =>
                {
                    if (doc.ContainsKey(WebKeys.ArchiveDestination))
                    {
                        return doc.GetPath(WebKeys.ArchiveDestination);
                    }

                    // Default destination
                    NormalizedPath destination = archiveDoc.Destination
                        .ChangeExtension(null)
                        .Combine(NormalizedPath.ReplaceInvalidFileNameChars(doc.GetString(Keys.GroupKey)));
                    int index = doc.GetInt(Keys.Index);
                    if (index > 1)
                    {
                        destination = destination.Combine(index.ToString());
                    }
                    return destination.AppendExtension(".html");
                }),
                true)
        };

        public static bool IsArchive(IDocument document) =>
            document.ContainsKey(WebKeys.ArchiveSources) || document.ContainsKey(WebKeys.ArchiveFilter) || document.ContainsKey(WebKeys.ArchiveKey);
    }
}
