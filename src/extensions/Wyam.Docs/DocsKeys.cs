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

        public const string AssemblyFiles = nameof(AssemblyFiles);

        /// <summary>
        /// The base URL for generating edit links for content and blog pages.
        /// The edit link combines this base URL with the relative path of the
        /// input file.
        /// </summary>
        public const string BaseEditUrl = nameof(BaseEditUrl);

        /// <summary>
        /// Set by the system for documents that support editing. Contains the
        /// relative path to the document to be appended to the base edit URL.
        /// </summary>
        public const string EditFilePath = nameof(EditFilePath);

        public const string IncludeGlobalNamespace = nameof(IncludeGlobalNamespace);
        
        public const string Description = nameof(Description);

        /// <summary>
        /// Controls whether type names from the API enclosed in code fences in either
        /// blog posts or content pages should be automatically linked to the
        /// corresponding API documentation page (the default is <c>true</c>).
        /// </summary>
        public const string AutoLinkTypes = nameof(AutoLinkTypes);

        /// <summary>
        /// Setting this to <c>true</c> in global metadata uses
        /// the year and date in the output path of blog posts.
        /// The default value is <c>false</c>.
        /// </summary>
        public const string IncludeDateInPostPath = nameof(IncludeDateInPostPath);

        /// <summary>
        /// Set to <c>false</c> to prevent a search index for named API types from being
        /// generated and presented on the API pages.
        /// </summary>
        public const string SearchIndex = nameof(SearchIndex);

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
        
        /// <summary>
        /// Setting this to <c>true</c> for a document will remove the
        /// sidebar from the page.
        /// </summary>
        public const string NoSidebar = nameof(NoSidebar);

        /// <summary>
        /// Setting this to <c>true</c> for a document will remove the
        /// surrounding container from a page, including the title.
        /// </summary>
        public const string NoContainer = nameof(NoContainer);

        /// <summary>
        /// Setting this to <c>true</c> for a document will remove the
        /// title banner from the page.
        /// </summary>
        public const string NoTitle = nameof(NoTitle);

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
        public const string Published = nameof(Published);

        /// <summary>
        /// Used by blog posts and pages to indicate the author.
        /// </summary>
        public const string Author = nameof(Author);

        /// <summary>
        /// Set to <c>true</c> (the default value) to generate meta refresh pages
        /// for any redirected documents (as indicated by a <c>RedirectFrom</c>
        /// metadata value).
        /// </summary>
        public const string MetaRefreshRedirects = nameof(MetaRefreshRedirects);

        /// <summary>
        /// Set to <c>true</c> (the default value is <c>false</c>) to generate
        /// a Netlify <c>_redirects</c> file from redirected documents
        /// (as indicated by a <c>RedirectFrom</c> metadata value).
        /// </summary>
        public const string NetlifyRedirects = nameof(NetlifyRedirects);
    }
}
