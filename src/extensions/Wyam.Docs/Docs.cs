using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.CodeAnalysis;
using Wyam.Common.Configuration;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Core.Modules.Contents;
using Wyam.Core.Modules.Control;
using Wyam.Core.Modules.Extensibility;
using Wyam.Core.Modules.IO;
using Wyam.Core.Modules.Metadata;
using Wyam.Razor;

namespace Wyam.Docs
{
    public class Docs : IRecipe
    {
        public void Apply(IEngine engine)
        {
            // Global metadata defaults
            engine.GlobalMetadata[DocsKeys.SourceFiles] = "src/**/{!bin,!obj,!packages,!*.Tests,}/**/*.cs";
            engine.GlobalMetadata[DocsKeys.IncludeGlobal] = true;
            engine.GlobalMetadata[DocsKeys.ApiPathPrefix] = "api";

            engine.Pipelines.Add(DocsPipelines.Code,
                new ReadFiles(ctx => ctx.GlobalMetadata.List<string>(DocsKeys.SourceFiles))
            );
            
            engine.Pipelines.Add(DocsPipelines.Pages,
                new ReadFiles(ctx => $"{{!{ctx.GlobalMetadata.String(DocsKeys.ApiPathPrefix)},**}}/*.md"),
                new FrontMatter(new Yaml.Yaml()),
                new Markdown.Markdown(),
                new Replace("<pre><code>", "<pre class=\"prettyprint\"><code>"),
                new Concat(
                    // Add any additional Razor pages
                    new ReadFiles(ctx => $"{{!{ctx.GlobalMetadata.String(DocsKeys.ApiPathPrefix)},**}}/{{!_,}}*.cshtml"),
                    new FrontMatter(new Yaml.Yaml())),
                new Title(),
                new Tree()
                    .WithPlaceholderFactory((path, items, context) =>
                    {
                        FilePath indexPath = new FilePath(string.Join("/", path.Concat(new[] {"index.html"})));
                        items.Add(Keys.RelativeFilePath, indexPath);
                        items.Add(Keys.Title, Title.GetTitle(indexPath));
                        return context.GetDocument("@Html.Partial(\"_ChildPages\")", items);
                    })
                    .CollapseRoot()
            );

            engine.Pipelines.Add(DocsPipelines.RenderPages,
                new Documents(DocsPipelines.Pages),
                new Flatten(),
                // Hide the sidebar for root pages if there's no children
                new Meta(DocsKeys.NoSidebar, (doc, ctx) => doc.Get(DocsKeys.NoSidebar, 
                    (doc.DocumentList(Keys.Children)?.Count ?? 0) == 0)
                    && doc.Document(Keys.Parent) == null),
                new Title(),
                new Razor.Razor()
                    .WithLayout("/_Layout.cshtml"),
                new WriteFiles(".html")
            );

            engine.Pipelines.Add(DocsPipelines.Api,
                new Documents(DocsPipelines.Code),
                // Put analysis module inside execute to have access to global metadata at runtime
                new Execute(ctx => new AnalyzeCSharp()
                    .WhereNamespaces(ctx.GlobalMetadata.Get<bool>(DocsKeys.IncludeGlobal))
                    .WherePublic()
                    .WithCssClasses("pre", "prettyprint")
                    .WithWritePathPrefix(ctx.GlobalMetadata.String(DocsKeys.ApiPathPrefix))),
                new Razor.Razor()
                    .WithLayout("/_ApiLayout.cshtml"),
                new WriteFiles()
            );

            engine.Pipelines.Add(DocsPipelines.ApiIndex,
                new ReadFiles("_ApiIndex.cshtml"),
                new Meta(Keys.RelativeFilePath, 
                    ctx => new DirectoryPath(ctx.GlobalMetadata.String(DocsKeys.ApiPathPrefix)).CombineFile("index.html")),
                new Meta(Keys.SourceFileName, "index.html"),
                new Title("API"),
                new Meta(DocsKeys.NoSidebar, true),
                new Razor.Razor(),
                new WriteFiles()
            );

            engine.Pipelines.Add(DocsPipelines.Less,
                new ReadFiles("assets/css/*.less"),
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
    }
}
