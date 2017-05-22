using System;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;

namespace Wyam.Web.Pipelines
{
    /// <summary>
    /// Settings for the <see cref="RenderPages"/> pipeline.
    /// </summary>
    public class RenderPagesSettings
    {
        /// <summary>
        /// The pipelines from which to get page documents.
        /// </summary>
        public string[] Pipelines { get; set; }

        /// <summary>
        /// The Razor layout to use.
        /// </summary>
        public DocumentConfig Layout { get; set; }

        /// <summary>
        /// Sorts the documents based on a comparison. If <c>null</c>, the sorting will be based on the document title.
        /// </summary>
        public Comparison<IDocument> Sort { get; set; }
    }
}