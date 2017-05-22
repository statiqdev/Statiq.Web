using System;
using Wyam.Common.Configuration;

namespace Wyam.Web.Pipelines
{
    /// <summary>
    /// Settings for the <see cref="RenderBlogPosts"/> pipeline.
    /// </summary>
    public class RenderBlogPostsSettings
    {
        /// <summary>
        /// The pipelines from which to get page documents.
        /// </summary>
        public string[] Pipelines { get; set; }

        /// <summary>
        /// The metadata key where <see cref="DateTime"/> published dates are stored for each post.
        /// </summary>
        public string PublishedKey { get; set; }

        /// <summary>
        /// The Razor layout to use.
        /// </summary>
        public DocumentConfig Layout { get; set; }
    }
}