using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
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
    /// This pipeline is designed to be used with documents from the <see cref="Pages"/> pipeline.
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
        /// Sorts the rendered documents.
        /// </summary>
        public const string Sort = nameof(Sort);

        /// <summary>
        /// Creates the pipeline.
        /// </summary>
        /// <param name="name">The name of this pipeline.</param>
        /// <param name="pipelines">The pipelines from which to get page documents.</param>
        /// <param name="layout">The Razor layout to use.</param>
        /// <param name="sort">Sorts the documents based on a comparison. If <c>null</c>, the sorting will be based on the document title.</param>
        public RenderPages(string name, string[] pipelines, DocumentConfig layout, Comparison<IDocument> sort)
            : base(name, GetModules(pipelines, layout, sort))
        {
        }

        private static IModuleList GetModules(string[] pipelines, DocumentConfig layout, Comparison<IDocument> sort) => new ModuleList
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
            },
            {
                Sort,
                new Sort(sort ?? ((x, y) => Comparer.Default.Compare(x.String(Keys.Title), y.String(Keys.Title))))
            }
        };
    }
}
