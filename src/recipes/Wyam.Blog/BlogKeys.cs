using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.IO;
using Wyam.Core.Modules.IO;

namespace Wyam.Blog
{
    /// <summary>
    /// Metadata keys used by the blog recipe.
    /// </summary>
    public static class BlogKeys
    {
        /// <summary>The title of the blog, post, or page.</summary>
        /// <type><see cref="string"/></type>
        public const string Title = nameof(Title);

        /// <summary>The relative path to a page image.</summary>
        /// <type><see cref="string"/></type>
        public const string Image = nameof(Image);

        /// <summary>The description of your blog (usually placed on the home page).</summary>
        /// <type><see cref="string"/></type>
        public const string Description = nameof(Description);

        /// <summary>
        /// A short introduction to your blog (usually placed on the home page
        /// under the description).
        /// </summary>
        /// <type><see cref="string"/></type>
        public const string Intro = nameof(Intro);

        /// <summary>
        /// Controls the parent path where blog posts are placed. The default is "posts".
        /// This affects both input and output files (I.e., if you change this your input
        /// files must also be under the same path).
        /// </summary>
        /// <type><see cref="DirectoryPath"/> or <see cref="string"/></type>
        public const string PostsPath = nameof(PostsPath);

        /// <summary>
        /// Set to <c>true</c> to make tag groupings case-insensitive.
        /// </summary>
        /// <type><see cref="bool"/></type>
        public const string CaseInsensitiveTags = nameof(CaseInsensitiveTags);

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
        /// <type><see cref="IEnumerable{IMarkdownExtension}"/></type>
        public const string MarkdownExtensionTypes = nameof(MarkdownExtensionTypes);

        /// <summary>
        /// Indicates that include statements should be processed using the <see cref="Include"/> module.
        /// The default behavior is not to process includes.
        /// </summary>
        /// <type><see cref="bool"/></type>
        public const string ProcessIncludes = nameof(ProcessIncludes);

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
        /// Specifies the path of the template file that lists your blog posts
        /// (which is also typically used as the home page by default).
        /// By default this setting is "_Index.cshtml". The file path will be used to
        /// read the index template. If you change it, you are responsible for providing
        /// this file in your input path (for example, by copying the "_Index.cshtml" from
        /// a theme to the specified file name in your input folder).
        /// </summary>
        /// <type><see cref="FilePath"/> or <see cref="string"/></type>
        public const string IndexTemplate = nameof(IndexTemplate);

        /// <summary>
        /// Specifies the output path of the generated index. By default this
        /// is the home page of your site at "index.html". If you change this
        /// it lets you provide an alternate home page for your site while still
        /// generating a list of posts at the specified output location.
        /// </summary>
        /// <type><see cref="FilePath"/> or <see cref="string"/></type>
        public const string IndexPath = nameof(IndexPath);

        /// <summary>
        /// Specifies the path where the blog RSS file will be output.
        /// The default value is <c>feed.rss</c>. Set to <c>null</c>
        /// to prevent generating an RSS feed.
        /// </summary>
        /// <type><see cref="FilePath"/> or <see cref="string"/></type>
        public const string RssPath = nameof(RssPath);

        /// <summary>
        /// Specifies the path where the blog Atom file will be output.
        /// The default value is <c>feed.atom</c>. Set to <c>null</c>
        /// to prevent generating an Atom feed.
        /// </summary>
        /// <type><see cref="FilePath"/> or <see cref="string"/></type>
        public const string AtomPath = nameof(AtomPath);

        /// <summary>
        /// Specifies the path where the blog RDF file will be output.
        /// The default value is <c>null</c> which
        /// prevents generating an RDF feed.
        /// </summary>
        /// <type><see cref="FilePath"/> or <see cref="string"/></type>
        public const string RdfPath = nameof(RdfPath);

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
        /// The page size for tag archive pages (if not specified, no paging will be used).
        /// </summary>
        /// <type><see cref="int"/></type>
        public const string TagPageSize = nameof(TagPageSize);

        /// <summary>
        /// The page size for the index page(s) (if not specified, a default of 3 will be used).
        /// </summary>
        /// <type><see cref="int"/></type>
        public const string IndexPageSize = nameof(IndexPageSize);

        /// <summary>
        /// Specifies whether index pages should be generated (if not specified or <c>false</c>,
        /// only the first index page will be generated).
        /// </summary>
        /// <type><see cref="bool"/></type>
        public const string IndexPaging = nameof(IndexPaging);

        /// <summary>
        /// Specifies whether the full post content should be output on the index page(s)
        /// (if not specified or <c>false</c>, only the excerpt will be output).
        /// </summary>
        /// <type><see cref="bool"/></type>
        public const string IndexFullPosts = nameof(IndexFullPosts);

        /// <summary>
        /// The page size for the archive page (if not specified, no paging will be used).
        /// </summary>
        /// <type><see cref="int"/></type>
        public const string ArchivePageSize = nameof(ArchivePageSize);

        /// <summary>
        /// Specifies whether the excerpts should be output on the archive page(s)
        /// (if not specified or <c>false</c>, only the title and description will be displayed).
        /// Note that this also controls displaying excerpts on the tag archive pages.
        /// </summary>
        /// <type><see cref="bool"/></type>
        public const string ArchiveExcerpts = nameof(ArchiveExcerpts);

        /// <summary>
        /// Specifies whether an archive should be generated (the default is <c>true</c>).
        /// </summary>
        /// <type><see cref="bool"/></type>
        public const string GenerateArchive = nameof(GenerateArchive);

        /// <summary>
        /// The date of the post.
        /// </summary>
        /// <type><see cref="DateTime"/> or <see cref="string"/></type>
        public const string Published = nameof(Published);

        /// <summary>
        /// The tags for a given post.
        /// </summary>
        /// <type><see cref="string"/> or <c>IEnumerable&lt;string&gt;</c></type>
        public const string Tags = nameof(Tags);

        /// <summary>
        /// A short description of a particular blog post.
        /// </summary>
        /// <type><see cref="string"/></type>
        public const string Lead = nameof(Lead);

        /// <summary>
        /// An excerpt of the blog post, automatically set for each document
        /// by the recipe.
        /// </summary>
        /// <type><see cref="string"/></type>
        public const string Excerpt = nameof(Excerpt);

        /// <summary>
        /// Set to <c>false</c> to hide a particular page from the top-level navigation bar.
        /// </summary>
        /// <type><see cref="bool"/></type>
        public const string ShowInNavbar = nameof(ShowInNavbar);

        /// <summary>
        /// Set by the recipe for tag groups. Contains the set of documents with a given tag.
        /// </summary>
        /// <type><c>IEnumerable&lt;IDocument&gt;</c></type>
        public const string Posts = nameof(Posts);

        /// <summary>
        /// Set by the recipe to the name of the tag for each tag group.
        /// </summary>
        /// <type><see cref="string"/></type>
        public const string Tag = nameof(Tag);

        /// <summary>
        /// This should be a string or array of strings with the name(s)
        /// of root-level folders to ignore when scanning for content pages.
        /// Setting this global metadata value is useful when introducing
        /// your own pipelines for files under certain folders and you don't
        /// want the primary content page pipelines to pick them up.
        /// </summary>
        /// <type><see cref="string"/> or <c>IEnumerable&lt;string&gt;</c></type>
        public const string IgnoreFolders = nameof(IgnoreFolders);
    }
}
