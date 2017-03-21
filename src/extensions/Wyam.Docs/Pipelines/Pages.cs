using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Core.Modules.Contents;
using Wyam.Core.Modules.Control;
using Wyam.Core.Modules.Extensibility;
using Wyam.Core.Modules.IO;
using Wyam.Core.Modules.Metadata;
using Wyam.Html;

namespace Wyam.Docs.Pipelines
{
    /// <summary>
    /// Loads documentation content from Markdown and/or Razor files.
    /// </summary>
    public class Pages : Pipeline
    {
        internal Pages(ConcurrentDictionary<string, string> typeNamesToLink)
            : base(GetModules(typeNamesToLink))
        {
        }

        private static ModuleList GetModules(ConcurrentDictionary<string, string> typeNamesToLink) => new ModuleList
        {
            new ReadFiles(ctx => $"{{{GetIgnoreFoldersGlob(ctx)}}}/*.md"),
            new Meta(DocsKeys.EditFilePath, (doc, ctx) => doc.FilePath(Keys.RelativeFilePath)),
            new Include(),
            new FrontMatter(new Yaml.Yaml()),
            new Execute(ctx => new Markdown.Markdown().UseExtensions(ctx.Settings.List<Type>(DocsKeys.MarkdownExternalExtensions)).UseConfiguration(ctx.String(DocsKeys.MarkdownExtensions))),
            new Concat(
                new ReadFiles(ctx => $"{{{GetIgnoreFoldersGlob(ctx)}}}/{{!_,}}*.cshtml"), // Add any additional Razor pages
                new Include(),
                new FrontMatter(new Yaml.Yaml())),
            new If(
                ctx => ctx.Bool(DocsKeys.AutoLinkTypes),
                new AutoLink(typeNamesToLink)
                    .WithQuerySelector("code")
                    .WithMatchOnlyWholeWord()
            ),
            // This is an ugly hack to re-escape @ symbols in Markdown since AngleSharp unescapes them if it
            // changes text content to add an auto link, can be removed if AngleSharp #494 is addressed
            new If(
                (doc, ctx) => doc.String(Keys.SourceFileExt) == ".md",
                new Replace("@", "&#64;")
            ),
            new Excerpt(),
            new Title(),
            new WriteFiles(".html").OnlyMetadata(),
            new Tree()
                .WithPlaceholderFactory(TreePlaceholderFactory)
                .WithNesting(true, true)
        };

        private static string GetIgnoreFoldersGlob(IExecutionContext context) =>
            string.Join(",", context
                .List(DocsKeys.IgnoreFolders, Array.Empty<string>())
                .Select(x => "!" + x)
                .Concat(new[] { "!blog", "!api", "**" }));

        private static IDocument TreePlaceholderFactory(object[] path, MetadataItems items, IExecutionContext context)
        {
            FilePath indexPath = new FilePath(string.Join("/", path.Concat(new[] { "index.html" })));
            items.Add(Keys.RelativeFilePath, indexPath);
            items.Add(Keys.Title, Title.GetTitle(indexPath));
            return context.GetDocument(context.GetContentStream("@Html.Partial(\"_ChildPages\")"), items);
        }
    }
}
