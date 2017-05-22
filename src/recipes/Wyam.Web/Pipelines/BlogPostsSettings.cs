using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Configuration;

namespace Wyam.Web.Pipelines
{
    /// <summary>
    /// Settings for the <see cref="BlogPosts"/> pipeline.
    /// </summary>
    public class BlogPostsSettings
    {
        /// <summary>
        /// The metadata key where <see cref="DateTime"/> published dates are stored for each post.
        /// </summary>
        public string PublishedKey { get; set; }

        /// <summary>
        /// A delegate that returns the string configuration for the Markdown processor.
        /// </summary>
        public ContextConfig MarkdownConfiguration { get; set; } = _ => Markdown.Markdown.DefaultConfiguration;

        /// <summary>
        /// A delegate that returns a sequence of <see cref="Type"/> for Markdown extensions.
        /// </summary>
        public ContextConfig MarkdownExtensionTypes { get; set; } = _ => null;

        /// <summary>
        /// A delegate that returns a <see cref="bool"/> indicating if documents should be processed with the <see cref="Include"/> module.
        /// </summary>
        public DocumentConfig ProcessIncludes { get; set; } = (doc, ctx) => false;

        /// <summary>
        /// A delegate that returns a <see cref="bool"/> indicating if post dates should be included in the output path.
        /// </summary>
        public ContextConfig IncludeDateInPostPath { get; set; } = _ => false;

        /// <summary>
        /// A delegate that should return a <see cref="string"/> with the path to blog post files.
        /// </summary>
        public ContextConfig PostsPath { get; set; }
    }
}
