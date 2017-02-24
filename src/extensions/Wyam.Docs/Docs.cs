using System;
using System.Collections;
using System.Collections.Concurrent;
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
        private readonly ConcurrentDictionary<string, string> _typeNamesToLink = new ConcurrentDictionary<string, string>();

        public void Apply(IEngine engine)
        {
            // Global metadata defaults
            engine.Settings[DocsKeys.SourceFiles] = new []
            {
                "src/**/{!bin,!obj,!packages,!*.Tests,}/**/*.cs",
                "../src/**/{!bin,!obj,!packages,!*.Tests,}/**/*.cs"
            };
            engine.Settings[DocsKeys.IncludeGlobalNamespace] = true;
            engine.Settings[DocsKeys.IncludeDateInPostPath] = false;
            engine.Settings[DocsKeys.MarkdownExtensions] = "advanced+bootstrap";
            engine.Settings[DocsKeys.SearchIndex] = true;
            engine.Settings[DocsKeys.MetaRefreshRedirects] = true;
            engine.Settings[DocsKeys.AutoLinkTypes] = true;
            engine.Settings[DocsKeys.BlogRssPath] = GenerateFeeds.DefaultRssPath;
            engine.Settings[DocsKeys.BlogAtomPath] = GenerateFeeds.DefaultAtomPath;
            engine.Settings[DocsKeys.BlogRdfPath] = GenerateFeeds.DefaultRdfPath;

            engine.Pipelines.Add(DocsPipelines.Code,
                new ReadFiles(ctx => ctx.List<string>(DocsKeys.SourceFiles))
            );

            engine.Pipelines.Add(DocsPipelines.Api,
                new If(ctx => ctx.Documents[DocsPipelines.Code].Any() || ctx.List<string>(DocsKeys.AssemblyFiles)?.Count > 0,
                    new Documents(DocsPipelines.Code),
                    // Put analysis module inside execute to have access to global metadata at runtime
                    new Execute(ctx => new AnalyzeCSharp()
                        .WhereNamespaces(ctx.Bool(DocsKeys.IncludeGlobalNamespace))
                        .WherePublic()
                        .WithCssClasses("code", "cs")
                        .WithWritePathPrefix("api")
                        .WithAssemblies(ctx.List<string>(DocsKeys.AssemblyFiles))
                        .WithAssemblySymbols()),
                    // Calculate a type name to link lookup for auto linking
                    new Execute((doc, ctx) =>
                    {
                        string name = null;
                        string kind = doc.String(CodeAnalysisKeys.Kind);
                        if (kind == "NamedType")
                        {
                            name = doc.String(CodeAnalysisKeys.DisplayName);
                        }
                        else if (kind == "Property" || kind == "Method")
                        {
                            IDocument containingType = doc.Document(CodeAnalysisKeys.ContainingType);
                            if (containingType != null)
                            {
                                name = $"{containingType.String(CodeAnalysisKeys.DisplayName)}.{doc.String(CodeAnalysisKeys.DisplayName)}";
                            }
                        }
                        if (name != null)
                        {
                            _typeNamesToLink.AddOrUpdate(name, ctx.GetLink(doc), (x, y) => string.Empty);
                        }
                    })
                )
            );

            engine.Pipelines.Add(DocsPipelines.Pages,
                new ReadFiles(ctx => $"{{{GetIgnoreFoldersGlob(ctx)}}}/*.md"),
                new Meta(DocsKeys.EditFilePath, (doc, ctx) => doc.FilePath(Keys.RelativeFilePath)),
                new Include(),
                new FrontMatter(new Yaml.Yaml()),
                new Execute(ctx => new Markdown.Markdown().UseExtensions(ctx.Settings.List<Type>(DocsKeys.MarkdownExternalExtensions)).UseConfiguration(ctx.String(DocsKeys.MarkdownExtensions))),
                new Concat(
                    // Add any additional Razor pages
                    new ReadFiles(ctx => $"{{{GetIgnoreFoldersGlob(ctx)}}}/{{!_,}}*.cshtml"),
                    new Include(),
                    new FrontMatter(new Yaml.Yaml())),
                new If(ctx => ctx.Bool(DocsKeys.AutoLinkTypes),
                    new AutoLink(_typeNamesToLink)
                        .WithQuerySelector("code")
                        .WithMatchOnlyWholeWord()
                ),
                // This is an ugly hack to re-escape @ symbols in Markdown since AngleSharp unescapes them if it
                // changes text content to add an auto link, can be removed if AngleSharp #494 is addressed
                new If((doc, ctx) => doc.String(Keys.SourceFileExt) == ".md",
                    new Replace("@", "&#64;")
                ),
                new Excerpt(),
                new Title(),
                new WriteFiles(".html").OnlyMetadata(),
                new Tree()
                    .WithPlaceholderFactory(TreePlaceholderFactory)
                    .WithNesting(true, true)
            );

            engine.Pipelines.Add(DocsPipelines.BlogPosts,
                new ReadFiles("blog/*.md"),
                new Meta(DocsKeys.EditFilePath, (doc, ctx) => doc.FilePath(Keys.RelativeFilePath)),
                new FrontMatter(new Yaml.Yaml()),
                new Execute(ctx => new Markdown.Markdown().UseExtensions(ctx.Settings.List<Type>(DocsKeys.MarkdownExternalExtensions)).UseConfiguration(ctx.String(DocsKeys.MarkdownExtensions))),
                new If(ctx => ctx.Bool(DocsKeys.AutoLinkTypes),
                    new AutoLink(_typeNamesToLink)
                        .WithQuerySelector("code")
                        .WithMatchOnlyWholeWord()
                ),
                // This is an ugly hack to re-escape @ symbols in Markdown since AngleSharp unescapes them if it
                // changes text content to add an auto link, can be removed if AngleSharp #494 is addressed
                new If((doc, ctx) => doc.String(Keys.SourceFileExt) == ".md",
                    new Replace("@", "&#64;")
                ),
                new Excerpt(),
                new Meta("FrontMatterPublished", (doc, ctx) => doc.ContainsKey(DocsKeys.Published)),  // Record whether the publish date came from front matter
                new Meta(DocsKeys.Published, (doc, ctx) =>
                {
                    DateTime published;
                    if (!DateTime.TryParse(doc.String(Keys.SourceFileName).Substring(0, 10), out published))
                    {
                        Wyam.Common.Tracing.Trace.Warning($"Could not parse published date for {doc.SourceString()}.");
                        return null;
                    }
                    return published;
                }).OnlyIfNonExisting(),
                new Where((doc, ctx) =>
                {
                    if (!doc.ContainsKey(DocsKeys.Published) || doc.Get(DocsKeys.Published) == null)
                    {
                        Common.Tracing.Trace.Warning($"Skipping {doc.SourceString()} due to not having {DocsKeys.Published} metadata");
                        return false;
                    }
                    if (doc.Get<DateTime>(DocsKeys.Published) > DateTime.Now)
                    {
                        Common.Tracing.Trace.Warning($"Skipping {doc.SourceString()} due to having {DocsKeys.Published} metadata in the future of {doc.Get<DateTime>(DocsKeys.Published)} (current date and time is {DateTime.Now})");
                        return false;
                    }
                    return true;
                }),
                new Meta(Keys.RelativeFilePath, (doc, ctx) =>
                {
                    DateTime published = doc.Get<DateTime>(DocsKeys.Published);
                    string fileName = doc.Bool("FrontMatterPublished")
                        ? doc.FilePath(Keys.SourceFileName).ChangeExtension("html").FullPath
                        : doc.FilePath(Keys.SourceFileName).ChangeExtension("html").FullPath.Substring(11);
                    return ctx.Bool(DocsKeys.IncludeDateInPostPath) ? $"blog/{published:yyyy}/{published:MM}/{fileName}" : $"blog/{fileName}";
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
                    new GenerateFeeds()
                        .WithRssPath(ctx => ctx.FilePath(DocsKeys.BlogRssPath))
                        .WithAtomPath(ctx => ctx.FilePath(DocsKeys.BlogAtomPath))
                        .WithRdfPath(ctx => ctx.FilePath(DocsKeys.BlogRdfPath)),
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
                    new WriteFiles()
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

            engine.Pipelines.Add(DocsPipelines.Redirects,
                new Documents(DocsPipelines.RenderPages),
                new Concat(
                    new Documents(DocsPipelines.RenderBlogPosts)
                ),
                new Execute(ctx =>
                {
                    Redirect redirect = new Redirect()
                        .WithMetaRefreshPages(ctx.Bool(DocsKeys.MetaRefreshRedirects));
                    if (ctx.Bool(DocsKeys.NetlifyRedirects))
                    {
                        redirect.WithAdditionalOutput("_redirects", redirects =>
                            string.Join(Environment.NewLine, redirects.Select(r => $"/{r.Key} {r.Value}")));
                    }
                    return redirect;
                }),
                new WriteFiles()
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
                    new Razor.Razor(),
                    new WriteFiles()
                )
            );

            engine.Pipelines.Add(DocsPipelines.ApiSearchIndex,
                new If(ctx => ctx.Documents[DocsPipelines.Api].Any() && ctx.Bool(DocsKeys.SearchIndex),
                    new Documents(DocsPipelines.Api),
                    new Where((doc, ctx) => doc.String(CodeAnalysisKeys.Kind) == "NamedType"),
                    new SearchIndex.SearchIndex((doc, ctx) =>
                        new SearchIndexItem(
                            ctx.GetLink(doc),
                            doc.String(CodeAnalysisKeys.DisplayName),
                            doc.String(CodeAnalysisKeys.DisplayName)
                        ))
                        .WithScript((scriptBuilder, context) =>
                        {
                            // Use a custom tokenizer that splits on camel case characters
                            // https://github.com/olivernn/lunr.js/issues/230#issuecomment-244790648
                            scriptBuilder.Insert(0, @"
var camelCaseTokenizer = function (obj) {
    var previous = '';
    return obj.toString().trim().split(/[\s\-]+|(?=[A-Z])/).reduce(function(acc, cur) {
        var current = cur.toLowerCase();
        if(acc.length === 0) {
            previous = current;
            return acc.concat(current);
        }
        previous = previous.concat(current);
        return acc.concat([current, previous]);
    }, []);
}
lunr.tokenizer.registerFunction(camelCaseTokenizer, 'camelCaseTokenizer')");
                            scriptBuilder.Replace("this.ref('id');", @"this.ref('id');
        this.tokenizer(camelCaseTokenizer);");
                            return scriptBuilder.ToString();
                        })
                        .WithPath("assets/js/searchIndex.js"),
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

            engine.Pipelines.Add(DocsPipelines.ValidateLinks,
                new If(ctx => ctx.Bool(DocsKeys.ValidateAbsoluteLinks) || ctx.Bool(DocsKeys.ValidateRelativeLinks),
                    new Documents(DocsPipelines.RenderPages),
                    new Concat(
                        new Documents(DocsPipelines.RenderBlogPosts)
                    ),
                    new Concat(
                        new Documents(DocsPipelines.RenderApi)
                    ),
                    new Concat(
                        new Documents(DocsPipelines.Resources)
                    ),
                    new Where((doc, ctx) =>
                    {
                        FilePath destinationPath = doc.FilePath(Keys.DestinationFilePath);
                        return destinationPath != null
                            && (destinationPath.Extension == ".html" || destinationPath.Extension == ".htm");
                    }),
                    new Execute(ctx =>
                        new ValidateLinks()
                            .ValidateAbsoluteLinks(ctx.Bool(DocsKeys.ValidateAbsoluteLinks))
                            .ValidateRelativeLinks(ctx.Bool(DocsKeys.ValidateRelativeLinks))
                            .AsError(ctx.Bool(DocsKeys.ValidateLinksAsError)
                        )
                    )
                )
            );
        }

        private string GetIgnoreFoldersGlob(IExecutionContext context) =>
            string.Join(",", context
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

        public void Scaffold(IFile configFile, IDirectory inputDirectory)
        {
            // Config file
            configFile?.WriteAllText(@"#recipe Docs");

            // Add info page
            inputDirectory.GetFile("about.md").WriteAllText(
@"Title: About This Project
---
This project is awesome!");

            // Add docs pages
            inputDirectory.GetFile("docs/command-line.md").WriteAllText(
@"Description: How to use the command line.
---
Here are some instructions on how to use the command line.");
            inputDirectory.GetFile("docs/usage.md").WriteAllText(
@"Description: Library usage instructions.
---
To use this library, take these steps...");

            // Add post page
            inputDirectory.GetFile("blog/new-release.md").WriteAllText(
@"Title: New Release
Published: 1/1/2016
Category: Release
Author: me
---
There is a new release out, go get it now.");
        }
    }
}
