using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Blog
{
    [Obsolete("Please use keys from Blog instead of BlogPipelines, this class will be removed in a future version")]
    public static class BlogPipelines
    {
        /// <summary>
        /// Loads page content from Markdown and/or Razor files.
        /// </summary>
        [Obsolete("Please use Blog.Pages instead of BlogPipelines.Pages which will be removed in a future version")]
        public const string Pages = nameof(Pages);

        /// <summary>
        /// Loads blog posts from Markdown and/or Razor files.
        /// </summary>
        [Obsolete("Please use Blog.RawPosts instead of BlogPipelines.RawPosts which will be removed in a future version")]
        public const string RawPosts = nameof(RawPosts);

        /// <summary>
        /// Generates tag groups from the tags on blog posts.
        /// </summary>
        [Obsolete("Please use Blog.Tags instead of BlogPipelines.Tags which will be removed in a future version")]
        public const string Tags = nameof(Tags);

        /// <summary>
        /// Renders blog post pages. This needs to come after the tags
        /// pipeline so that the listing of tags on each blog post page
        /// will have the correct counts.
        /// </summary>
        [Obsolete("Please use Blog.Posts instead of BlogPipelines.Posts which will be removed in a future version")]
        public const string Posts = nameof(Posts);

        /// <summary>
        /// Generates the blog RSS, Atom, and/or RDF feeds.
        /// </summary>
        [Obsolete("Please use Blog.Feed instead of BlogPipelines.Feed which will be removed in a future version")]
        public const string Feed = nameof(Feed);

        /// <summary>
        /// Renders and outputs the content pages using the template layouts.
        /// </summary>
        [Obsolete("Please use Blog.RenderPages instead of BlogPipelines.RenderPages which will be removed in a future version")]
        public const string RenderPages = nameof(RenderPages);

        /// <summary>
        /// Generates any redirect placeholders and files.
        /// </summary>
        [Obsolete("Please use Blog.Redirects instead of BlogPipelines.Redirects which will be removed in a future version")]
        public const string Redirects = nameof(Redirects);

        /// <summary>
        /// Copies all other resources to the output path.
        /// </summary>
        [Obsolete("Please use Blog.Resources instead of BlogPipelines.Resources which will be removed in a future version")]
        public const string Resources = nameof(Resources);

        /// <summary>
        /// Validates links.
        /// </summary>
        [Obsolete("Please use Blog.ValidateLinks instead of BlogPipelines.ValidateLinks which will be removed in a future version")]
        public const string ValidateLinks = nameof(ValidateLinks);
    }
}
