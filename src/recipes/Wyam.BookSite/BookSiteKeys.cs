using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.IO;
using Wyam.Core.Modules.IO;

namespace Wyam.BookSite
{
    /// <summary>
    /// Metadata keys used by the BookSite recipe.
    /// </summary>
    public static class BookSiteKeys
    {
        /// <summary>
        /// Controls the parent path where blog posts are placed. The default is "blog".
        /// This affects both input and output files (I.e., if you change this your input
        /// files must also be under the same path).
        /// </summary>
        /// <type><see cref="DirectoryPath"/> or <see cref="string"/></type>
        public const string BlogPath = nameof(BlogPath);

        /// <summary>
        /// Controls the parent path where chapters are placed. The default is "chapters".
        /// This affects both input and output files (I.e., if you change this your input
        /// files must also be under the same path).
        /// </summary>
        /// <type><see cref="DirectoryPath"/> or <see cref="string"/></type>
        public const string ChaptersPath = nameof(ChaptersPath);

        /// <summary>An introduction to display above the chapters listing.</summary>
        /// <type><see cref="string"/></type>
        public const string ChaptersIntro = nameof(ChaptersIntro);

        /// <summary>
        /// Controls the parent path where sections for the homepage are placed. The default is "sections".
        /// </summary>
        /// <type><see cref="DirectoryPath"/> or <see cref="string"/></type>
        public const string SectionsPath = nameof(SectionsPath);

        /// <summary>
        /// Controls the order in which pages should appear in the navigation bar or sections should appear on the homepage.
        /// </summary>
        /// <type><see cref="int"/></type>
        public const string Order = nameof(Order);

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
        /// Set this to control the activated set of Markdown extensions for the
        /// Markdig Markdown renderer. The default value is "advanced+bootstrap".
        /// </summary>
        /// <type><see cref="string"/></type>
        public const string MarkdownConfiguration = nameof(MarkdownConfiguration);

        /// <summary>
        /// Set this to add extension Markdown extensions for the Markdig Markdown
        /// renderer. The default value is null;
        /// </summary>
        /// <type><see cref="IEnumerable{Type}"/></type>
        public const string MarkdownExtensionTypes = nameof(MarkdownExtensionTypes);

        /// <summary>
        /// Indicates that include statements should be processed using the <see cref="Include"/> module.
        /// The default behavior is not to process includes.
        /// </summary>
        /// <type><see cref="bool"/></type>
        public const string ProcessIncludes = nameof(ProcessIncludes);

        /// <summary>The title of the book, chapter, or page.</summary>
        /// <type><see cref="string"/></type>
        public const string Title = nameof(Title);

        /// <summary>The subtitle of the book.</summary>
        /// <type><see cref="string"/></type>
        public const string Subtitle = nameof(Subtitle);

        /// <summary>The description of your book (usually placed on the home page).</summary>
        /// <type><see cref="string"/></type>
        public const string Description = nameof(Description);

        /// <summary>
        /// Provides the chapter number for a chapter.
        /// </summary>
        /// <type><see cref="int"/></type>
        public const string ChapterNumber = nameof(ChapterNumber);

        /// <summary>The relative path to an image to display in the layout.</summary>
        /// <type><see cref="string"/></type>
        public const string Image = nameof(Image);

        /// <summary>A link to the book order form, download, etc.</summary>
        /// <type><see cref="string"/></type>
        public const string BookLink = nameof(BookLink);

        /// <summary>The text to display on book links. Defaults to "Order Now".</summary>
        /// <type><see cref="string"/></type>
        public const string BookLinkText = nameof(BookLinkText);

        /// <summary>The path to an image of the book, defaults to "/images/book.png".</summary>
        /// <type><see cref="string"/></type>
        public const string BookImage = nameof(BookImage);

        /// <summary>
        /// Setting this to <c>true</c> uses
        /// the year and date in the output path of blog posts.
        /// The default value is <c>false</c>.
        /// </summary>
        /// <type><see cref="bool"/></type>
        public const string IncludeDateInPostPath = nameof(IncludeDateInPostPath);

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
        /// The page size for blog index pages (the default is 5).
        /// </summary>
        /// <type><see cref="int"/></type>
        public const string BlogPageSize = nameof(BlogPageSize);

        /// <summary>
        /// The date of the post.
        /// </summary>
        /// <type><see cref="DateTime"/> or <see cref="string"/></type>
        public const string Published = nameof(Published);

        /// <summary>
        /// Set to <c>true</c> to hide a particular page from the top-level navigation bar.
        /// </summary>
        /// <type><see cref="bool"/></type>
        public const string ShowInNavbar = nameof(ShowInNavbar);

        /// <summary>
        /// Set to <c>true</c> to prepend a configured <c>LinkRoot</c> to all root-relative Markdown links.
        /// </summary>
        /// <type><see cref="bool"/></type>
        public const string MarkdownPrependLinkRoot = nameof(MarkdownPrependLinkRoot);
    }
}
