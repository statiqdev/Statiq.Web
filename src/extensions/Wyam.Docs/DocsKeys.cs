using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Docs
{
    public static class DocsKeys
    {
        /// <summary>
        /// The location of your source files. Can be a <c>string</c>
        /// or a collection of <c>string</c>. This will be evaluated
        /// from the context of the input virtual file system.
        /// </summary>
        public const string SiteTitle = nameof(SiteTitle);

        public const string SourceFiles = nameof(SourceFiles);

        public const string IncludeGlobalNamespace = nameof(IncludeGlobalNamespace);
        
        public const string Description = nameof(Description);

        /// <summary>
        /// Setting this to <c>true</c> in global metadata uses
        /// the year and date in the output path of blog posts.
        /// The default value is <c>false</c>.
        /// </summary>
        public const string IncludeDateInPostPath = nameof(IncludeDateInPostPath);

        /// <summary>
        /// Set this in global metadata to control the set of Markdown extensions for Markdig.
        /// </summary>
        public const string MarkdownExtensions = nameof(MarkdownExtensions);

        /// <summary>
        /// Used by blog posts to indicate the category of the post.
        /// Also used by pages to indicate the category of the page.
        /// </summary>
        public const string Category = nameof(Category);

        public const string Order = nameof(Order);

        public const string HideInMenu = nameof(HideInMenu);

        public const string NoSidebar = nameof(NoSidebar);

        public const string NoContainer = nameof(NoContainer);

        public const string NoGutter = nameof(NoGutter);

        /// <summary>
        /// This should be a string or array of strings with the name(s)
        /// of root-level folders to ignore when scanning for content pages.
        /// Setting this global metadata value is useful when introducing
        /// your own pipelines for files under certain folders and you don't
        /// want the primary content page pipelines to pick them up.
        /// </summary>
        public const string IgnoreFolders = nameof(IgnoreFolders);

        /// <summary>
        /// Used for blog posts to store the date of the post.
        /// </summary>
        public const string Date = nameof(Date);

        /// <summary>
        /// Used by blog posts and pages to indicate the author.
        /// </summary>
        public const string Author = nameof(Author);


    }
}
