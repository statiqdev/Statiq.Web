using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Blog
{
    public static class BlogPipelines
    {
        /// <summary>
        /// Loads page content from Markdown and/or Razor files.
        /// </summary>
        public const string Pages = nameof(Pages);

        /// <summary>
        /// Loads blog posts from Markdown and/or Razor files.
        /// </summary>
        public const string RawPosts = nameof(RawPosts);

        /// <summary>
        /// Generates tag groups from the tags on blog posts.
        /// </summary>
        public const string Tags = nameof(Tags);

        /// <summary>
        /// Renders blog post pages. This needs to come after the tags
        /// pipeline so that the listing of tags on each blog post page
        /// will have the correct counts.
        /// </summary>
        public const string Posts = nameof(Posts);

        /// <summary>
        /// Generates the blog RSS, Atom, and/or RDF feeds.
        /// </summary>
        public const string Feed = nameof(Feed);

        /// <summary>
        /// Renders and outputs the content pages using the template layouts.
        /// </summary>
        public const string RenderPages = nameof(RenderPages);

        /// <summary>
        /// Generates any redirect placeholders and files.
        /// </summary>
        public const string Redirects = nameof(Redirects);

        /// <summary>
        /// Copies all other resources to the output path.
        /// </summary>
        public const string Resources = nameof(Resources);

        /// <summary>
        /// Validates links.
        /// </summary>
        public const string ValidateLinks = nameof(ValidateLinks);

    }
}
