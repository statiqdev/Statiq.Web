using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Configuration;
using Wyam.Common.Execution;
using Wyam.Common.Modules;
using Wyam.Core.Modules.Control;
using Wyam.Core.Modules.Extensibility;
using Wyam.Core.Modules.IO;
using Wyam.Core.Modules.Metadata;
using Wyam.Html;

namespace Wyam.Web.Pipelines
{
    /// <summary>
    /// Renders and outputs the blog posts using the template layouts.
    /// This pipeline is designed to be used with documents from the <see cref="BlogPosts"/> pipeline.
    /// </summary>
    public class RenderBlogPosts : Pipeline
    {
        /// <summary>
        /// Gets page documents from the requested pipeline(s) and flattens them.
        /// </summary>
        public const string GetDocuments = nameof(GetDocuments);

        /// <summary>
        /// Renders the pages.
        /// </summary>
        public const string Render = nameof(Render);

        /// <summary>
        /// Writes post-rendering metadata to the documents (such as headings and excerpts).
        /// </summary>
        public const string WriteMetadata = nameof(WriteMetadata);

        /// <summary>
        /// Writes the documents to the file system.
        /// </summary>
        public const string WriteFiles = nameof(WriteFiles);

        /// <summary>
        /// Orders the posts by their published date.
        /// </summary>
        public const string OrderByPublished = nameof(OrderByPublished);

        /// <summary>
        /// Creates the pipeline.
        /// </summary>
        /// <param name="name">The name of this pipeline.</param>
        /// <param name="pipelines">The pipelines from which to get page documents.</param>
        /// <param name="publishedKey">The metadata key where <see cref="DateTime"/> published dates are stored for each post.</param>
        /// <param name="layout">The Razor layout to use.</param>
        public RenderBlogPosts(string name, string[] pipelines, string publishedKey, DocumentConfig layout)
            : base(name, GetModules(pipelines, publishedKey, layout))
        {
        }

        private static IModuleList GetModules(string[] pipelines, string publishedKey, DocumentConfig layout) => new ModuleList
        {
            {
                GetDocuments,
                new ModuleCollection
                {
                    new Documents()
                        .FromPipelines(pipelines)
                }
            },
            {
                Render,
                new Razor.Razor()
                    .WithLayout(layout)
            },
            {
                WriteMetadata,
                new Headings()
            },
            {
                WriteFiles,
                new WriteFiles()
            },
            {
                OrderByPublished,
                new OrderBy((doc, ctx) => doc.Get<DateTime>(publishedKey)).Descending()
            }
        };
    }
}
