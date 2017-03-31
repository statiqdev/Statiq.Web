using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.IO;

namespace Wyam.Docs
{
    /// <summary>
    /// Metadata keys used by the docs recipe.
    /// </summary>
    public static class DocsKeys
    {
        /// <summary>
        /// The title of the site, post, or page.
        /// </summary>
        /// <type><see cref="string"/></type>
        public const string Title = nameof(Title);

        /// <summary>
        /// The path to a logo to use for the site.
        /// </summary>
        /// <type><see cref="FilePath"/></type>
        public const string Logo = nameof(Logo);

        /// <summary>
        /// Indicates where to locate source files for the API documentation.
        /// By default the globbing pattern "src/**/{!bin,!obj,!packages,!*.Tests,}/**/*.cs"
        /// is used which searches for all "*.cs" files at any depth under a "src" folder
        /// but not under "bin", "obj", "packages" or "Tests" folders. You can specify
        /// your own globbing pattern (or more than one globbing pattern) if your source
        /// files are found elsewhere.
        /// </summary>
        /// <type><see cref="string"/> or <c>IEnumerable&lt;string&gt;</c></type>
        public const string SourceFiles = nameof(SourceFiles);

        /// <summary>
        /// Indicates where to locate assemblies for the API documentation. You can specify
        /// one (or more) globbing pattern(s).
        /// </summary>
        /// <type><see cref="string"/> or <c>IEnumerable&lt;string&gt;</c></type>
        public const string AssemblyFiles = nameof(AssemblyFiles);

        /// <summary>
        /// The base URL for generating edit links for content and blog pages.
        /// The edit link combines this base URL with the relative path of the
        /// input file.
        /// </summary>
        /// <type><see cref="string"/></type>
        public const string BaseEditUrl = nameof(BaseEditUrl);

        /// <summary>
        /// Controls whether the global namespace is included in your API
        /// documentation.
        /// </summary>
        /// <type><see cref="bool"/></type>
        public const string IncludeGlobalNamespace = nameof(IncludeGlobalNamespace);

        /// <summary>
        /// Controls whether type names from the API enclosed in code fences in either
        /// blog posts or content pages should be automatically linked to the
        /// corresponding API documentation page (the default is <c>true</c>).
        /// </summary>
        /// <type><see cref="bool"/></type>
        public const string AutoLinkTypes = nameof(AutoLinkTypes);

        /// <summary>
        /// Setting this to <c>true</c> uses
        /// the year and date in the output path of blog posts.
        /// The default value is <c>false</c>.
        /// </summary>
        /// <type><see cref="bool"/></type>
        public const string IncludeDateInPostPath = nameof(IncludeDateInPostPath);

        /// <summary>
        /// Set to <c>false</c> to prevent a search index for API types from being
        /// generated and presented on the API pages.
        /// </summary>
        /// <type><see cref="bool"/></type>
        public const string SearchIndex = nameof(SearchIndex);

        /// <summary>
        /// Set this to control the activated set of Markdown extensions for the
        /// Markdig Markdown renderer. The default value is "advanced+bootstrap".
        /// </summary>
        /// <type><see cref="string"/></type>
        public const string MarkdownExtensions = nameof(MarkdownExtensions);

        /// <summary>
        /// Set this to add extension Markdown extensions for the Markdig Markdown
        /// renderer. The default value is null;
        /// </summary>
        /// <type><see cref="IEnumerable{IMarkDownExtension}"/></type>
        public const string MarkdownExternalExtensions = nameof(MarkdownExternalExtensions);

        /// <summary>
        /// This should be a string or array of strings with the name(s)
        /// of root-level folders to ignore when scanning for content pages.
        /// Setting this global metadata value is useful when introducing
        /// your own pipelines for files under certain folders and you don't
        /// want the primary content page pipelines to pick them up.
        /// </summary>
        /// <type><see cref="string"/> or <c>IEnumerable&lt;string&gt;</c></type>
        public const string IgnoreFolders = nameof(IgnoreFolders);

        /// <summary>
        /// Set to <c>true</c> (the default value) to generate meta refresh pages
        /// for any redirected documents (as indicated by a <c>RedirectFrom</c>
        /// metadata value in the document).
        /// </summary>
        /// <type><see cref="bool"/></type>
        public const string MetaRefreshRedirects = nameof(MetaRefreshRedirects);

        /// <summary>
        /// Set to <c>true</c> (the default value is <c>false</c>) to generate
        /// a Netlify <c>_redirects</c> file from redirected documents
        /// (as indicated by a <c>RedirectFrom</c> metadata value).
        /// </summary>
        /// <type><see cref="bool"/></type>
        public const string NetlifyRedirects = nameof(NetlifyRedirects);

        /// <summary>
        /// Specifies the path where the blog RSS file will be output.
        /// The default value is <c>feed.rss</c>. Set to <c>null</c>
        /// to prevent generating an RSS feed.
        /// </summary>
        /// <type><see cref="FilePath"/> or <see cref="string"/></type>
        public const string BlogRssPath = nameof(BlogRssPath);

        /// <summary>
        /// Specifies the path where the blog Atom file will be output.
        /// The default value is <c>feed.atom</c>. Set to <c>null</c>
        /// to prevent generating an Atom feed.
        /// </summary>
        /// <type><see cref="FilePath"/> or <see cref="string"/></type>
        public const string BlogAtomPath = nameof(BlogAtomPath);

        /// <summary>
        /// Specifies the path where the blog RDF file will be output.
        /// The default value is <c>null</c> which
        /// prevents generating an RDF feed.
        /// </summary>
        /// <type><see cref="FilePath"/> or <see cref="string"/></type>
        public const string BlogRdfPath = nameof(BlogRdfPath);

        /// <summary>
        /// Set to <c>true</c> (the default value is <c>false</c>) to
        /// validate all absolute links. Note that this may add considerable
        /// time to your generation process.
        /// </summary>
        /// <type><see cref="bool"/></type>
        public const string ValidateAbsoluteLinks = nameof(ValidateAbsoluteLinks);

        /// <summary>
        /// Set to <c>true</c> (the default value) to
        /// validate all relative links.
        /// </summary>
        /// <type><see cref="bool"/></type>
        public const string ValidateRelativeLinks = nameof(ValidateRelativeLinks);

        /// <summary>
        /// Set to <c>true</c> (the default value is <c>false</c>) to
        /// report errors on link validation failures.
        /// </summary>
        /// <type><see cref="bool"/></type>
        public const string ValidateLinksAsError = nameof(ValidateLinksAsError);

        /// <summary>
        /// Set by the system for documents that support editing. Contains the
        /// relative path to the document to be appended to the base edit URL.
        /// </summary>
        /// <type><see cref="FilePath"/></type>
        public const string EditFilePath = nameof(EditFilePath);

        /// <summary>
        /// Used to provide a description for pages and blog posts that can be used
        /// in the meta description tag and also for page listings.
        /// </summary>
        /// <type><see cref="string"/></type>
        public const string Description = nameof(Description);

        /// <summary>
        /// Used by blog posts to indicate the category of the post.
        /// Also used by pages to indicate the category of the page.
        /// </summary>
        /// <type><see cref="string"/></type>
        public const string Category = nameof(Category);

        /// <summary>
        /// Indicates the relative order of pages to each other. If
        /// no value is supplied for a document, the default order
        /// is 1000 (I.e., after most pages that do have a defined
        /// order).
        /// </summary>
        /// <type><see cref="int"/></type>
        public const string Order = nameof(Order);

        /// <summary>
        /// Setting this to <c>true</c> for a document will remove the
        /// sidebar from the page.
        /// </summary>
        /// <type><see cref="bool"/></type>
        public const string NoSidebar = nameof(NoSidebar);

        /// <summary>
        /// Setting this to <c>true</c> for a document will remove the
        /// surrounding container from a page, including the title.
        /// </summary>
        /// <type><see cref="bool"/></type>
        public const string NoContainer = nameof(NoContainer);

        /// <summary>
        /// Setting this to <c>true</c> for a document will remove the
        /// title banner from the page.
        /// </summary>
        /// <type><see cref="bool"/></type>
        public const string NoTitle = nameof(NoTitle);

        /// <summary>
        /// Setting this to <c>true</c> will remove the gutter area
        /// from around a page.
        /// </summary>
        /// <type><see cref="bool"/></type>
        public const string NoGutter = nameof(NoGutter);

        /// <summary>
        /// Used for blog posts to store the date of the post.
        /// </summary>
        /// <type><see cref="DateTime"/> or <see cref="string"/></type>
        public const string Published = nameof(Published);

        /// <summary>
        /// Used by blog posts and pages to indicate the author.
        /// </summary>
        /// <type><see cref="string"/></type>
        public const string Author = nameof(Author);
    }
}
