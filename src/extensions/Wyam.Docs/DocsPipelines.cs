using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Docs
{
    [Obsolete("Please use keys from Docs, this class will be removed in a future version")]
    public static class DocsPipelines
    {
        /// <summary>
        /// Loads source files.
        /// </summary>
        [Obsolete("Please use keys from Docs, this class will be removed in a future version")]
        public const string Code = nameof(Code);

        /// <summary>
        /// Uses Roslyn to analyze any source files loaded in the previous
        /// pipeline along with any specified assemblies. This pipeline
        /// results in documents that represent Roslyn symbols.
        /// </summary>
        [Obsolete("Please use keys from Docs, this class will be removed in a future version")]
        public const string Api = nameof(Api);

        /// <summary>
        /// Loads documentation content from Markdown and/or Razor files.
        /// </summary>
        [Obsolete("Please use keys from Docs, this class will be removed in a future version")]
        public const string Pages = nameof(Pages);

        /// <summary>
        /// Loads blog posts from Markdown and/or Razor files.
        /// </summary>
        [Obsolete("Please use keys from Docs, this class will be removed in a future version")]
        public const string BlogPosts = nameof(BlogPosts);

        /// <summary>
        /// Generates the index pages for blog posts.
        /// </summary>
        [Obsolete("Please use keys from Docs, this class will be removed in a future version")]
        public const string BlogIndexes = nameof(BlogIndexes);

        /// <summary>
        /// Generates the category pages for blog posts.
        /// </summary>
        [Obsolete("Please use keys from Docs, this class will be removed in a future version")]
        public const string BlogCategories = nameof(BlogCategories);

        /// <summary>
        /// Generates the date-based archive pages for blog posts.
        /// </summary>
        [Obsolete("Please use keys from Docs, this class will be removed in a future version")]
        public const string BlogArchives = nameof(BlogArchives);

        /// <summary>
        /// Generates the author pages for blog posts.
        /// </summary>
        [Obsolete("Please use keys from Docs, this class will be removed in a future version")]
        public const string BlogAuthors = nameof(BlogAuthors);

        /// <summary>
        /// Generates the blog RSS, Atom, and/or RDF feeds.
        /// </summary>
        [Obsolete("Please use keys from Docs, this class will be removed in a future version")]
        public const string BlogFeed = nameof(BlogFeed);

        /// <summary>
        /// Renders and outputs the document content pages using the template layouts.
        /// </summary>
        [Obsolete("Please use keys from Docs, this class will be removed in a future version")]
        public const string RenderPages = nameof(RenderPages);

        /// <summary>
        /// Renders and outputs the blog posts using the template layouts.
        /// </summary>
        [Obsolete("Please use keys from Docs, this class will be removed in a future version")]
        public const string RenderBlogPosts = nameof(RenderBlogPosts);

        /// <summary>
        /// Generates any redirect placeholders and files.
        /// </summary>
        [Obsolete("Please use keys from Docs, this class will be removed in a future version")]
        public const string Redirects = nameof(Redirects);

        /// <summary>
        /// Renders and outputs the API pages using the API template layouts
        /// (this pipeline might take a bit of time).
        /// </summary>
        [Obsolete("Please use keys from Docs, this class will be removed in a future version")]
        public const string RenderApi = nameof(RenderApi);

        /// <summary>
        /// Generates the API index file.
        /// </summary>
        [Obsolete("Please use keys from Docs, this class will be removed in a future version")]
        public const string ApiIndex = nameof(ApiIndex);

        /// <summary>
        /// Generates the API search index.
        /// </summary>
        [Obsolete("Please use keys from Docs, this class will be removed in a future version")]
        public const string ApiSearchIndex = nameof(ApiSearchIndex);

        /// <summary>
        /// Processes any Less stylesheets and outputs the resulting CSS files.
        /// </summary>
        [Obsolete("Please use keys from Docs, this class will be removed in a future version")]
        public const string Less = nameof(Less);

        /// <summary>
        /// Copies all other resources to the output path.
        /// </summary>
        [Obsolete("Please use keys from Docs, this class will be removed in a future version")]
        public const string Resources = nameof(Resources);

        /// <summary>
        /// Validates links.
        /// </summary>
        [Obsolete("Please use keys from Docs, this class will be removed in a future version")]
        public const string ValidateLinks = nameof(ValidateLinks);
    }
}
