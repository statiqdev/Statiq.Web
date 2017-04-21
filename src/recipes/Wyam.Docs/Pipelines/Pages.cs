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
using Wyam.Common.Util;
using Wyam.Core.Modules.Contents;
using Wyam.Core.Modules.Control;
using Wyam.Core.Modules.Extensibility;
using Wyam.Core.Modules.IO;
using Wyam.Core.Modules.Metadata;
using Wyam.Html;

#pragma warning disable SA1115, SA1515

namespace Wyam.Docs.Pipelines
{
    /// <inheritdoc cref="Web.Pipelines.Pages" />
    public class Pages : Pipeline
    {
        /// <inheritdoc cref="Web.Pipelines.Pages.MarkdownFiles" />
        public const string MarkdownFiles = nameof(Web.Pipelines.Pages.MarkdownFiles);

        /// <inheritdoc cref="Web.Pipelines.Pages.RazorFiles" />
        public const string RazorFiles = nameof(Web.Pipelines.Pages.RazorFiles);

        /// <summary>
        /// Links type names from the API in pages.
        /// </summary>
        public const string LinkTypeNames = nameof(LinkTypeNames);

        /// <inheritdoc cref="Web.Pipelines.Pages.WriteMetadata" />
        public const string WriteMetadata = nameof(Web.Pipelines.Pages.WriteMetadata);

        /// <inheritdoc cref="Web.Pipelines.Pages.CreateTree" />
        public const string CreateTree = nameof(Web.Pipelines.Pages.CreateTree);

        internal Pages(ConcurrentDictionary<string, string> typeNamesToLink)
            : base(GetModules(typeNamesToLink))
        {
        }

        private static IModuleList GetModules(ConcurrentDictionary<string, string> typeNamesToLink) =>
            new Web.Pipelines.Pages(
                nameof(Pages),
                ctx => new[] { ctx.DirectoryPath(DocsKeys.BlogPath).FullPath, "api" }
                    .Concat(ctx.List(DocsKeys.IgnoreFolders, Array.Empty<string>())),
                ctx => ctx.String(DocsKeys.MarkdownConfiguration),
                ctx => ctx.List<Type>(DocsKeys.MarkdownExtensionTypes),
                TreePlaceholderFactory)
                    .InsertAfter(
                        Web.Pipelines.Pages.RazorFiles,
                        LinkTypeNames,
                        new If(
                            ctx => ctx.Bool(DocsKeys.AutoLinkTypes),
                            new AutoLink(typeNamesToLink)
                                .WithQuerySelector("code")
                                .WithMatchOnlyWholeWord(),
                            // This is an ugly hack to re-escape @ symbols in Markdown since AngleSharp unescapes them if it
                            // changes text content to add an auto link, can be removed if AngleSharp #494 is addressed
                            new If(
                                (doc, ctx) => doc.String(Keys.SourceFileExt) == ".md",
                                new Replace("@", "&#64;"))));

        private static IDocument TreePlaceholderFactory(object[] path, MetadataItems items, IExecutionContext context)
        {
            FilePath indexPath = new FilePath(string.Join("/", path.Concat(new[] { "index.html" })));
            items.Add(Keys.RelativeFilePath, indexPath);
            items.Add(Keys.Title, Title.GetTitle(indexPath));
            return context.GetDocument(context.GetContentStream("@Html.Partial(\"_ChildPages\")"), items);
        }
    }
}

#pragma warning restore SA1115, SA1515