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

namespace Wyam.Docs.Pipelines
{
    /// <inheritdoc cref="WebRecipe.Pipelines.BlogPosts" />
    public class BlogPosts : Pipeline
    {
        /// <inheritdoc cref="WebRecipe.Pipelines.BlogPosts.MarkdownPosts" />
        public const string MarkdownPosts = nameof(WebRecipe.Pipelines.BlogPosts.MarkdownPosts);

        /// <inheritdoc cref="WebRecipe.Pipelines.BlogPosts.RazorPosts" />
        public const string RazorPosts = nameof(WebRecipe.Pipelines.BlogPosts.RazorPosts);

        /// <summary>
        /// Links type names from the API in pages.
        /// </summary>
        public const string LinkTypeNames = nameof(LinkTypeNames);

        /// <inheritdoc cref="WebRecipe.Pipelines.BlogPosts.Published" />
        public const string Published = nameof(WebRecipe.Pipelines.BlogPosts.Published);

        /// <inheritdoc cref="WebRecipe.Pipelines.BlogPosts.WriteMetadata" />
        public const string WriteMetadata = nameof(WebRecipe.Pipelines.BlogPosts.WriteMetadata);

        /// <inheritdoc cref="WebRecipe.Pipelines.BlogPosts.RelativeFilePath" />
        public const string RelativeFilePath = nameof(WebRecipe.Pipelines.BlogPosts.RelativeFilePath);

        /// <inheritdoc cref="WebRecipe.Pipelines.BlogPosts.OrderByPublished" />
        public const string OrderByPublished = nameof(WebRecipe.Pipelines.BlogPosts.OrderByPublished);

        internal BlogPosts(ConcurrentDictionary<string, string> typeNamesToLink)
            : base(GetModules(typeNamesToLink))
        {
        }

        private static IModuleList GetModules(ConcurrentDictionary<string, string> typeNamesToLink) =>
            new WebRecipe.Pipelines.BlogPosts(ctx => ctx.DirectoryPath(DocsKeys.BlogPath).FullPath)
                .InsertAfter(
                    WebRecipe.Pipelines.BlogPosts.RazorPosts,
                    LinkTypeNames,
                    new If(
                        ctx => ctx.Bool(DocsKeys.AutoLinkTypes),
                        new AutoLink(typeNamesToLink)
                            .WithQuerySelector("code")
                            .WithMatchOnlyWholeWord(),
#pragma warning disable SA1115 // Parameter must follow comma

                        // This is an ugly hack to re-escape @ symbols in Markdown since AngleSharp unescapes them if it
                        // changes text content to add an auto link, can be removed if AngleSharp #494 is addressed
                        new If(
                            (doc, ctx) => doc.String(Keys.SourceFileExt) == ".md",
                            new Replace("@", "&#64;"))));
#pragma warning restore SA1115 // Parameter must follow comma
    }
}
