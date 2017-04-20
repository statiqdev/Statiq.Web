using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.IO;
using Wyam.WebRecipe;

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
        /// Indicates where to locate project files for the API documentation.
        /// </summary>
        /// <type><see cref="string"/> or <c>IEnumerable&lt;string&gt;</c></type>
        public const string ProjectFiles = nameof(ProjectFiles);

        /// <summary>
        /// Indicates where to locate solution files for the API documentation.
        /// </summary>
        /// <type><see cref="string"/> or <c>IEnumerable&lt;string&gt;</c></type>
        public const string SolutionFiles = nameof(SolutionFiles);

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
        /// Controls the parent path where blog posts are placed. The default is "blog".
        /// This affects both input and output files (I.e., if you change this your input
        /// files must also be under the same path).
        /// </summary>
        /// <type><see cref="DirectoryPath"/> or <see cref="string"/></type>
        public const string BlogPath = nameof(BlogPath);

        /// <summary>
        /// Used by blog posts and pages to indicate the author.
        /// </summary>
        /// <type><see cref="string"/></type>
        public const string Author = nameof(Author);

        /// <inheritdoc cref="WebRecipeKeys.ValidateAbsoluteLinks" />
        public const string ValidateAbsoluteLinks = nameof(WebRecipeKeys.ValidateAbsoluteLinks);

        /// <inheritdoc cref="WebRecipeKeys.ValidateRelativeLinks" />
        public const string ValidateRelativeLinks = nameof(WebRecipeKeys.ValidateRelativeLinks);

        /// <inheritdoc cref="WebRecipeKeys.ValidateLinksAsError" />
        public const string ValidateLinksAsError = nameof(WebRecipeKeys.ValidateLinksAsError);

        /// <inheritdoc cref="WebRecipeKeys.MetaRefreshRedirects" />
        public const string MetaRefreshRedirects = nameof(WebRecipeKeys.MetaRefreshRedirects);

        /// <inheritdoc cref="WebRecipeKeys.NetlifyRedirects" />
        public const string NetlifyRedirects = nameof(WebRecipeKeys.NetlifyRedirects);

        /// <inheritdoc cref="WebRecipeKeys.IgnoreFolders" />
        public const string IgnoreFolders = nameof(WebRecipeKeys.IgnoreFolders);

        /// <inheritdoc cref="WebRecipeKeys.EditFilePath" />
        public const string EditFilePath = nameof(WebRecipeKeys.EditFilePath);

        /// <inheritdoc cref="WebRecipeKeys.MarkdownExtensions" />
        public const string MarkdownExtensions = nameof(WebRecipeKeys.MarkdownExtensions);

        /// <inheritdoc cref="WebRecipeKeys.MarkdownExternalExtensions" />
        public const string MarkdownExternalExtensions = nameof(WebRecipeKeys.MarkdownExternalExtensions);
    }
}
