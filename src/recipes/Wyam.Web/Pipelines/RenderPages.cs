using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Configuration;
using Wyam.Common.Execution;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Util;
using Wyam.Core.Modules.Control;
using Wyam.Core.Modules.Extensibility;
using Wyam.Core.Modules.IO;
using Wyam.Core.Modules.Metadata;
using Wyam.Html;

namespace Wyam.Web.Pipelines
{
    /// <summary>
    /// Renders and outputs the document content pages using the template layouts.
    /// </summary>
    public class RenderPages : Pipeline
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
        /// Orders the posts by their published date. We need to do this
        /// again since the order would have gotten messed up by the concurrent Razor rendering.
        /// </summary>
        public const string OrderByPublished = nameof(OrderByPublished);

        /// <summary>
        /// Creates the pipeline.
        /// </summary>
        /// <param name="name">The name of this pipeline.</param>
        /// <param name="pipelines">The pipelines from which to get page documents.</param>
        /// <param name="layout">The Razor layout to use.</param>
        public RenderPages(string name, string[] pipelines, DocumentConfig layout)
            : base(name, GetModules(pipelines, layout))
        {
        }

        private static IModuleList GetModules(string[] pipelines, DocumentConfig layout) => new ModuleList
        {
            {
                GetDocuments,
                new ModuleCollection
                {
                    new Documents()
                        .FromPipelines(pipelines),
                    new Flatten()
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
            }
        };
    }
}
