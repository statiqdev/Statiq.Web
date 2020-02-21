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
                new FilterDocuments(Config.FromDocument(doc => doc.ContainsKey(WebKeys.ArchiveSources) || doc.ContainsKey(WebKeys.ArchiveFilter) || doc.ContainsKey(WebKeys.ArchiveKey))),
                new ForEachDocument
                {
                    new ExecuteConfig(Config.FromDocument(archiveDoc => new ModuleList
                    {
                        // Merge outputs from the pipeline(s) with the archive document
                        // Need to merge so ArchiveFilter is on the actual candidate document
                        new MergeDocuments(archiveDoc.GetList(WebKeys.ArchivePipelines, new[] { nameof(Content) }).ToArray()).Reverse(),
                        new LogMessage(Config.FromContext(ctx => $"{ctx.Inputs.Length} total documents...")),

                        // Filter by document source
                        archiveDoc.ContainsKey(WebKeys.ArchiveSources) ? new FilterSources(archiveDoc.GetList<string>(WebKeys.ArchiveSources)) : null,
                        new LogMessage(Config.FromContext(ctx => $"{ctx.Inputs.Length} after sources...")),

                        // Filter by metadata
                        archiveDoc.ContainsKey(WebKeys.ArchiveFilter) ? new FilterDocuments(Config.FromDocument(doc => doc.GetBool(WebKeys.ArchiveFilter))) : null,
                        new LogMessage(Config.FromContext(ctx => $"{ctx.Inputs.Length} after filter...")),

                        // Group by key (if we have a key)
                        archiveDoc.ContainsKey(WebKeys.ArchiveKey) ? new GroupDocuments(Config.FromDocument(doc => doc.GetList(WebKeys.ArchiveKey, new object[] { }))) : null,
                        new LogMessage(Config.FromContext(ctx => $"{ctx.Inputs.Length} after grouping...")),

                        // At this point we either have group documents or the original documents
                        // Either way, roll them up into a single parent (you can tell it's the top-level because it doesn't have a GroupKey)
                        new ReplaceDocuments(Config.FromContext(ctx => ctx.CreateDocument(new MetadataItems { { Keys.Children, ctx.Inputs } }).Yield())),
                        new LogMessage(Config.FromContext(ctx => $"{ctx.Inputs.Length} after rollup...")),

                        // Seperate branches for top-level archive vs. group pages
                        new ExecuteBranch(new ModuleList
                        {
                            new LogMessage("Top-level branch..."),

                            // Render the top-level view
                            new AddTitle(),
                            new SetDestination(Config.FromValue(archiveDoc.Destination.ChangeExtension(".html")), true),
                            archiveDoc.MediaTypeEquals("text/markdown") ? new RenderMarkdown().UseExtensions() : null,
                            new RenderRazor()
                                .WithLayout(Config.FromDocument((doc, ctx) =>
                                {
                                    // Crawl up the tree looking for a layout
                                    IDocument parent = doc;
                                    while (parent != null)
                                    {
                                        if (parent.ContainsKey(WebKeys.Layout))
                                        {
                                            return parent.GetPath(WebKeys.Layout);
                                        }
                                        parent = parent.GetParent(ctx.Inputs);
                                    }
                                    return null;  // If no layout metadata, revert to default behavior
                                })),
                            new ExecuteIf(Config.FromSetting<bool>(WebKeys.MirrorResources))
                            {
                                new MirrorResources()
                            }
                        }).Branch(new ModuleList
                        {
                            new LogMessage("Groups branch..."),

                            // Explode the top-level back into groups
                            new ReplaceDocuments(Config.FromDocument<IEnumerable<IDocument>>(doc => doc.GetDocumentList(Keys.Children))),

                            // Deal with each group individually
                            new ForEachDocument
                            {
                                // If we need to paginate, explode the groups and then paginate the children
                                // Make sure we retain the group key so it can be reapplied to each page
                                // Otherwise we can just leave the group alone
                                archiveDoc.ContainsKey(WebKeys.ArchivePageSize)
                                    ? new ExecuteConfig(Config.FromDocument(groupDoc => new ModuleList
                                    {
                                        new ReplaceDocuments(Config.FromDocument<IEnumerable<IDocument>>(doc => doc.GetDocumentList(Keys.Children))),
                                        new PaginateDocuments(archiveDoc.GetInt(WebKeys.ArchivePageSize)),
                                        new SetMetadata(Keys.GroupKey, groupDoc.Get(Keys.GroupKey))
                                    }))
                                    : null,

                                // Now we have pages for the group or the entire group depending on if we were paginating

                                // Set the title
                                new AddTitle(Config.FromDocument((doc, ctx) =>
                                {
                                    if (doc.ContainsKey(WebKeys.ArchiveTitle))
                                    {
                                        return doc.GetString(WebKeys.ArchiveTitle);
                                    }

                                    // Default title
                                    string title = $"{doc.GetString(Keys.Title)} - {doc.GetString(Keys.GroupKey)}";
                                    int page = ctx.Inputs.IndexOf(doc);
                                    return page == 0 ? title : (title + $" (Page {page})");
                                })),

                                // Set the destination
                                new SetDestination(
                                    Config.FromDocument((doc, ctx) =>
                                    {
                                        if (doc.ContainsKey(WebKeys.ArchiveDestination))
                                        {
                                            return doc.GetPath(WebKeys.ArchiveDestination);
                                        }

                                        // Default destination
                                        NormalizedPath destination = archiveDoc.Destination
                                            .ChangeExtension(null)
                                            .Combine(doc.GetString(Keys.GroupKey));
                                        int page = ctx.Inputs.IndexOf(doc);
                                        if (page > 0)
                                        {
                                            destination = destination.Combine(page.ToString());
                                        }
                                        return destination.AppendExtension(".html");
                                    }),
                                    true),

                                // Render the pages (these will have a GroupKey while the top-level archive will not)
                                archiveDoc.MediaTypeEquals("text/markdown") ? new RenderMarkdown().UseExtensions() : null,
                                new RenderRazor()
                                    .WithLayout(Config.FromDocument((doc, ctx) =>
                                    {
                                        // Crawl up the tree looking for a layout
                                        IDocument parent = doc;
                                        while (parent != null)
                                        {
                                            if (parent.ContainsKey(WebKeys.Layout))
                                            {
                                                return parent.GetPath(WebKeys.Layout);
                                            }
                                            parent = parent.GetParent(ctx.Inputs);
                                        }
                                        return null;  // If no layout metadata, revert to default behavior
                                    })),
                                new ExecuteIf(Config.FromSetting<bool>(WebKeys.MirrorResources))
                                {
                                    new MirrorResources()
                                }
                            }
                        })
                    }))
                },
            };

            OutputModules = new ModuleList
            {
                new WriteFiles()
            };
        }
    }
}
