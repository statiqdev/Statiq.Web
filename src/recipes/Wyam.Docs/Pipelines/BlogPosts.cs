using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Execution;
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
    /// <inheritdoc cref="Web.Pipelines.BlogPosts" />
    public class BlogPosts : Pipeline
    {
        /// <inheritdoc cref="Web.Pipelines.BlogPosts.MarkdownPosts" />
        public const string MarkdownPosts = nameof(Web.Pipelines.BlogPosts.MarkdownPosts);

        /// <inheritdoc cref="Web.Pipelines.BlogPosts.RazorPosts" />
        public const string RazorPosts = nameof(Web.Pipelines.BlogPosts.RazorPosts);

        /// <summary>
        /// Links type names from the API in pages.
        /// </summary>
        public const string LinkTypeNames = nameof(LinkTypeNames);

        /// <inheritdoc cref="Web.Pipelines.BlogPosts.Published" />
        public const string Published = nameof(Web.Pipelines.BlogPosts.Published);

        /// <inheritdoc cref="Web.Pipelines.BlogPosts.WriteMetadata" />
        public const string WriteMetadata = nameof(Web.Pipelines.BlogPosts.WriteMetadata);

        /// <inheritdoc cref="Web.Pipelines.BlogPosts.RelativeFilePath" />
        public const string RelativeFilePath = nameof(Web.Pipelines.BlogPosts.RelativeFilePath);

        /// <inheritdoc cref="Web.Pipelines.BlogPosts.OrderByPublished" />
        public const string OrderByPublished = nameof(Web.Pipelines.BlogPosts.OrderByPublished);

        internal BlogPosts(ConcurrentDictionary<string, string> typeNamesToLink)
            : base(GetModules(typeNamesToLink))
        {
        }

        private static IModuleList GetModules(ConcurrentDictionary<string, string> typeNamesToLink) =>
            new Web.Pipelines.BlogPosts(
                nameof(BlogPosts),
                DocsKeys.Published,
                ctx => ctx.String(DocsKeys.MarkdownConfiguration),
                ctx => ctx.List<Type>(DocsKeys.MarkdownExtensionTypes),
                ctx => ctx.Bool(DocsKeys.IncludeDateInPostPath),
                ctx => ctx.DirectoryPath(DocsKeys.BlogPath).FullPath)
                    .InsertAfter(
                        Web.Pipelines.BlogPosts.RazorPosts,
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
    }
}

#pragma warning restore SA1115, SA1515