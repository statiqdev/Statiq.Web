using System;
using System.Linq;
using Wyam.Common.Configuration;
using Wyam.Common.Execution;
using Wyam.Common.Modules;
using Wyam.Core.Modules.Contents;
using Wyam.Core.Modules.Control;
using Wyam.Core.Modules.Extensibility;
using Wyam.Core.Modules.IO;

namespace Wyam.Blog.Pipelines
{
    /// <summary>
    /// Generates any redirect placeholders and files.
    /// </summary>
    public class Redirects : Pipeline
    {
        /// <summary>
        /// Gets both rendered pages and posts.
        /// </summary>
        public const string GetDocuments = nameof(GetDocuments);

        /// <summary>
        /// Generate the redirect files.
        /// </summary>
        public const string GenerateRedirects = nameof(GenerateRedirects);

        /// <summary>
        /// Writes the documents to the file system.
        /// </summary>
        public const string WriteFiles = nameof(WriteFiles);

        internal Redirects()
            : base(GetModules())
        {
        }

        private static ModuleList GetModules() => new ModuleList
        {
            {
                GetDocuments,
                new ModuleCollection
                {
                    new Documents(Blog.RenderPages),
                    new Concat
                    {
                        new Documents(Blog.Posts)
                    }
                }
            },
            {
                GenerateRedirects,
                new Execute(ctx =>
                {
                    Redirect redirect = new Redirect()
                        .WithMetaRefreshPages(ctx.Bool(BlogKeys.MetaRefreshRedirects));
                    if (ctx.Bool(BlogKeys.NetlifyRedirects))
                    {
                        redirect.WithAdditionalOutput("_redirects", redirects =>
                            string.Join(Environment.NewLine, redirects.Select(r => $"/{r.Key} {r.Value}")));
                    }
                    return redirect;
                })
            },
            {
                WriteFiles,
                new WriteFiles()
            }
        };
    }
}