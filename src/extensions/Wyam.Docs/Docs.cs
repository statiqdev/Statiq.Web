using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.CodeAnalysis;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Core.Modules.Contents;
using Wyam.Core.Modules.Control;
using Wyam.Core.Modules.Extensibility;
using Wyam.Core.Modules.IO;
using Wyam.Core.Modules.Metadata;
using Wyam.Feeds;
using Wyam.Html;
using Wyam.Razor;
using Wyam.SearchIndex;

namespace Wyam.Docs
{
    public class Docs : IRecipe
    {
        public void Apply(IEngine engine)
        {
            // Global metadata defaults
            engine.GlobalMetadata[DocsKeys.SourceFiles] = "src/**/{!bin,!obj,!packages,!*.Tests,}/**/*.cs";
            engine.GlobalMetadata[DocsKeys.IncludeGlobalNamespace] = true;
            engine.GlobalMetadata[DocsKeys.IncludeDateInPostPath] = false;
            engine.GlobalMetadata[DocsKeys.MarkdownExtensions] = "advanced+bootstrap";
            engine.GlobalMetadata[DocsKeys.SearchIndex] = true;

            engine.Pipelines.Add(DocsPipelines.Code,
                new ReadFiles(ctx => ctx.GlobalMetadata.List<string>(DocsKeys.SourceFiles))
            );

            engine.Pipelines.Add(DocsPipelines.Api,
                new Documents(DocsPipelines.Code),
                // Put analysis module inside execute to have access to global metadata at runtime
                new Execute(ctx => new AnalyzeCSharp()
                    .WhereNamespaces(ctx.GlobalMetadata.Get<bool>(DocsKeys.IncludeGlobalNamespace))
                    .WherePublic()
                    .WithCssClasses("code", "cs")
                    .WithWritePathPrefix("api")
                    .WithAssemblies(ctx.GlobalMetadata.List<string>(DocsKeys.AssemblyFiles))
                    .WithAssemblySymbols())
            );

            engine.Pipelines.Add(DocsPipelines.Pages,
                new ReadFiles(ctx => $"{{{GetIgnoreFoldersGlob(ctx)}}}/*.md"),
                new Meta(DocsKeys.EditFilePath, (doc, ctx) => doc.FilePath(Keys.RelativeFilePath)),
                new Include(),
                new FrontMatter(new Yaml.Yaml()),
                new Execute(ctx => new Markdown.Markdown().UseConfiguration(ctx.String(DocsKeys.MarkdownExtensions))),
                new Concat(
                    // Add any additional Razor pages
                    new ReadFiles(ctx => $"{{{GetIgnoreFoldersGlob(ctx)}}}/{{!_,}}*.cshtml"),
                    new Include(),
                    new FrontMatter(new Yaml.Yaml())),
                new Excerpt(),
                new Title(),
                new Tree()
                    .WithPlaceholderFactory(TreePlaceholderFactory)
                    .WithNesting(true, true)
            );
            
            engine.Pipelines.Add(DocsPipelines.BlogPosts,
                new ReadFiles("blog/*.md"),
                new Meta(DocsKeys.EditFilePath, (doc, ctx) => doc.FilePath(Keys.RelativeFilePath)),
                new FrontMatter(new Yaml.Yaml()),
                new Execute(ctx => new Markdown.Markdown().UseConfiguration(ctx.String(DocsKeys.MarkdownExtensions))),
                new Excerpt(),
                new Meta("FrontMatterPublished", (doc, ctx) => doc.ContainsKey(DocsKeys.Published)),  // Record whether the publish date came from front matter
                new Meta(DocsKeys.Published, (doc, ctx) => DateTime.Parse(doc.String(Keys.SourceFileName).Substring(0, 10))).OnlyIfNonExisting(),
                new Meta(Keys.RelativeFilePath, (doc, ctx) =>
                {
                    DateTime published = doc.Get<DateTime>(DocsKeys.Published);
                    string fileName = doc.Get<bool>("FrontMatterPublished")
                        ? doc.FilePath(Keys.SourceFileName).ChangeExtension("html").FullPath
                        : doc.FilePath(Keys.SourceFileName).ChangeExtension("html").FullPath.Substring(11);
                    return ctx.Get<bool>(DocsKeys.IncludeDateInPostPath) ? $"blog/{published:yyyy}/{published:MM}/{fileName}" : $"blog/{fileName}";
                }),
                new OrderBy((doc, ctx) => doc.Get<DateTime>(DocsKeys.Published)).Descending()
            );

            engine.Pipelines.Add(DocsPipelines.BlogIndexes,
                new If(ctx => ctx.Documents[DocsPipelines.BlogPosts].Any(),
                    new ReadFiles("_BlogIndex.cshtml"),
                    new Paginate(5,
                        new Documents(DocsPipelines.BlogPosts),
                        new OrderBy((doc, ctx) => doc.Get<DateTime>(DocsKeys.Published)).Descending()
                    ),
                    new Meta(Keys.Title, (doc, ctx) =>
                        doc.Get<int>(Keys.CurrentPage) == 1
                            ? "Blog"
                            : $"Blog (Page {doc[Keys.CurrentPage]})"),
                    new Meta(Keys.RelativeFilePath, (doc, ctx) =>
                        doc.Get<int>(Keys.CurrentPage) == 1
                            ? "blog/index.html"
                            : $"blog/page{doc[Keys.CurrentPage]}.html"),
                    new Razor.Razor()
                        .IgnorePrefix(null)
                        .WithLayout("/_BlogLayout.cshtml"),
                    new WriteFiles()
                )
            );

            engine.Pipelines.Add(DocsPipelines.BlogCategories,
                new If(ctx => ctx.Documents[DocsPipelines.BlogPosts].Any(),
                    new ReadFiles("_BlogIndex.cshtml"),
                    new GroupBy((doc, ctx) => doc[DocsKeys.Category],
                        new Documents(DocsPipelines.BlogPosts)
                    ),
                    new ForEach(
                        new Paginate(5,
                            new Documents((doc, ctx) => doc[Keys.GroupDocuments]),
                            new OrderBy((doc, ctx) => doc.Get<DateTime>(DocsKeys.Published)).Descending()
                        ),
                        new Meta(Keys.Title, (doc, ctx) =>
                            doc.Get<int>(Keys.CurrentPage) == 1
                                ? $"{doc[Keys.GroupKey]}"
                                : $"{doc[Keys.GroupKey]} (Page {doc[Keys.CurrentPage]})"),
                        new Meta(Keys.RelativeFilePath, (doc, ctx) =>
                        {
                            string category = doc.String(Keys.GroupKey).ToLower().Replace(" ", "-").Replace("'", string.Empty);
                            return doc.Get<int>(Keys.CurrentPage) == 1
                                ? $"blog/{category}/index.html"
                                : $"blog/{category}/page{doc[Keys.CurrentPage]}.html";
                        }),
                        new Razor.Razor()
                            .IgnorePrefix(null)
                            .WithLayout("/_BlogLayout.cshtml"),
                        new WriteFiles()
                    )
                )
            );

            engine.Pipelines.Add(DocsPipelines.BlogArchives,
                new If(ctx => ctx.Documents[DocsPipelines.BlogPosts].Any(),
                    // Monthly archives
                    new ReadFiles("_BlogIndex.cshtml"),
                    new GroupBy((doc, ctx) => new DateTime(doc.Get<DateTime>(DocsKeys.Published).Year, doc.Get<DateTime>(DocsKeys.Published).Month, 1),
                        new Documents(DocsPipelines.BlogPosts)
                    ),
                    new Meta(Keys.Title, (doc, ctx) => doc.Get<DateTime>(Keys.GroupKey).ToString("MMMM, yyyy")),
                    new Meta("Link", (doc, ctx) => doc.Get<DateTime>(Keys.GroupKey).ToString("yyyy/MM")),
                    new Concat(
                        // Yearly archives
                        new ReadFiles("_BlogIndex.cshtml"),
                        new GroupBy((doc, ctx) => new DateTime(doc.Get<DateTime>(DocsKeys.Published).Year, 1, 1),
                            new Documents(DocsPipelines.BlogPosts)
                        ),
                        new Meta(Keys.Title, (doc, ctx) => doc.Get<DateTime>(Keys.GroupKey).ToString("yyyy")),
                        new Meta("Link", (doc, ctx) => doc.Get<DateTime>(Keys.GroupKey).ToString("yyyy"))
                    ),
                    new ForEach(
                        new Paginate(5,
                            new Documents((doc, ctx) => doc[Keys.GroupDocuments]),
                            new OrderBy((doc, ctx) => doc.Get<DateTime>(DocsKeys.Published)).Descending()
                        ),
                        new Meta(Keys.Title, (doc, ctx) =>
                             doc.Get<int>(Keys.CurrentPage) == 1
                                ? $"{doc[Keys.Title]}"
                                : $"{doc[Keys.Title]} (Page {doc[Keys.CurrentPage]})"),
                        new Meta(Keys.RelativeFilePath, (doc, ctx) =>
                            doc.Get<int>(Keys.CurrentPage) == 1
                                ? $"blog/archive/{doc["Link"]}/index.html"
                                : $"blog/archive/{doc["Link"]}/page{doc[Keys.CurrentPage]}.html"),
                        new Razor.Razor()
                            .IgnorePrefix(null)
                            .WithLayout("/_BlogLayout.cshtml"),
                        new WriteFiles()
                    )
                )
            );

            engine.Pipelines.Add(DocsPipelines.BlogAuthors,
                new If(ctx => ctx.Documents[DocsPipelines.BlogPosts].Any(),
                    new ReadFiles("_BlogIndex.cshtml"),
                    new GroupBy((doc, ctx) => doc[DocsKeys.Author],
                        new Documents(DocsPipelines.BlogPosts)
                    ),
                    new ForEach(
                        new Paginate(5,
                            new Documents((doc, ctx) => doc[Keys.GroupDocuments]),
                            new OrderBy((doc, ctx) => doc.Get<DateTime>(DocsKeys.Published)).Descending()
                        ),
                        new Meta(Keys.Title, (doc, ctx) =>
                            doc.Get<int>(Keys.CurrentPage) == 1
                                ? $"{doc[Keys.GroupKey]}"
                                : $"{doc[Keys.GroupKey]} (Page {doc[Keys.CurrentPage]})"),
                        new Meta(Keys.RelativeFilePath, (doc, ctx) =>
                        {
                            string author = doc.String(Keys.GroupKey).ToLower().Replace(" ", "-").Replace("'", string.Empty);
                            return doc.Get<int>(Keys.CurrentPage) == 1
                                ? $"blog/author/{author}/index.html"
                                : $"blog/author/{author}/page{doc[Keys.CurrentPage]}.html";
                        }),
                        new Razor.Razor()
                            .IgnorePrefix(null)
                            .WithLayout("/_BlogLayout.cshtml"),
                        new WriteFiles()
                    )
                )
            );

            engine.Pipelines.Add(DocsPipelines.BlogFeed,
                new If(ctx => ctx.Documents[DocsPipelines.BlogPosts].Any(),
                    new Documents(DocsPipelines.BlogPosts),
                    new GenerateFeeds(),
                    new WriteFiles()
                )
            );

            engine.Pipelines.Add(DocsPipelines.RenderPages,
                new If(ctx => ctx.Documents[DocsPipelines.Pages].Any(),
                    new Documents(DocsPipelines.Pages),
                    new Flatten(),
                    // Hide the sidebar for root pages if there's no children
                    new Meta(DocsKeys.NoSidebar, (doc, ctx) => doc.Get(DocsKeys.NoSidebar,
                        (doc.DocumentList(Keys.Children)?.Count ?? 0) == 0)
                        && doc.Document(Keys.Parent) == null),
                    new Title(),
                    new Razor.Razor()
                        .WithLayout("/_Layout.cshtml"),
                    new Headings(),
                    new HtmlInsert("div#infobar-headings", (doc, ctx) => ctx.GenerateInfobarHeadings(doc)),
                    new WriteFiles(".html")
                )
            );

            // Render the blog after the indexes and archive so the layout doesn't show up when including whole page (I.e., first post)
            engine.Pipelines.Add(DocsPipelines.RenderBlogPosts,
                new If(ctx => ctx.Documents[DocsPipelines.BlogPosts].Any(),
                    new Documents(DocsPipelines.BlogPosts),
                    new Razor.Razor()
                        .WithLayout("/_BlogPost.cshtml"),
                    new Headings(),
                    new HtmlInsert("div#infobar-headings", (doc, ctx) => ctx.GenerateInfobarHeadings(doc)),
                    new WriteFiles()
                )
            );
            
            engine.Pipelines.Add(DocsPipelines.RenderApi,
                new If(ctx => ctx.Documents[DocsPipelines.Api].Any(),
                    new Documents(DocsPipelines.Api),
                    new Razor.Razor()
                        .WithLayout("/_ApiLayout.cshtml"),
                    new Headings(),
                    new HtmlInsert("div#infobar-headings", (doc, ctx) => ctx.GenerateInfobarHeadings(doc)),
                    new WriteFiles()
                )
            );

            engine.Pipelines.Add(DocsPipelines.ApiIndex,
                new If(ctx => ctx.Documents[DocsPipelines.Api].Any(),
                    new ReadFiles("_ApiIndex.cshtml"),
                    new Meta(Keys.RelativeFilePath, "api/index.html"),
                    new Meta(Keys.SourceFileName, "index.html"),
                    new Title("API"),
                    new Meta(DocsKeys.NoSidebar, true),
                    new Razor.Razor(),
                    new WriteFiles()
                )
            );

            engine.Pipelines.Add(DocsPipelines.ApiSearchIndex,
                new If(ctx => ctx.Documents[DocsPipelines.Api].Any() && ctx.GlobalMetadata.Get<bool>(DocsKeys.SearchIndex),
                    new Documents(DocsPipelines.Api),
                    new Where((doc, ctx) => doc.String(CodeAnalysisKeys.Kind) == "NamedType"),
                    new SearchIndex.SearchIndex((doc, ctx) => 
                        new SearchIndexItem(
                            ctx.GetLink(doc),
                            doc.String(CodeAnalysisKeys.DisplayName),
                            doc.String(CodeAnalysisKeys.DisplayName)
                        )
                    ).IncludeHost(false),
                    new Meta(Keys.WritePath, new FilePath("assets/js/searchIndex.js")),
                    new WriteFiles()
                )
            );
            
            engine.Pipelines.Add(DocsPipelines.Less,
                new ReadFiles("assets/css/*.less"),
                new Concat(
                    new ReadFiles("assets/css/bootstrap/bootstrap.less")
                ),
                new Concat(
                    new ReadFiles("assets/css/adminlte/AdminLTE.less")
                ),
                new Concat(
                    new ReadFiles("assets/css/theme/theme.less")
                ),
                new Less.Less(),
                new WriteFiles(".css")
            );

            engine.Pipelines.Add(DocsPipelines.Resources,
                new CopyFiles("**/*{!.cshtml,!.md,!.less,}")
            );
        }

        public void Scaffold(IDirectory directory)
        {
            throw new NotImplementedException();
        }

        private string GetIgnoreFoldersGlob(IExecutionContext context) => 
            string.Join(",", context.GlobalMetadata
                .List(DocsKeys.IgnoreFolders, Array.Empty<string>())
                .Select(x => "!" + x)
                .Concat(new[] { "!blog", "!api", "**" }));

        private IDocument TreePlaceholderFactory(object[] path, MetadataItems items, IExecutionContext context)
        {
            FilePath indexPath = new FilePath(string.Join("/", path.Concat(new[] {"index.html"})));
            items.Add(Keys.RelativeFilePath, indexPath);
            items.Add(Keys.Title, Title.GetTitle(indexPath));
            return context.GetDocument("@Html.Partial(\"_ChildPages\")", items);
        }
    }
}
