using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.IO;

namespace Wyam.Blog
{
    public static class BlogKeys
    {
        // ***Global

        /// <summary>
        /// When used as a global setting, indicates the title of your blog. Otherwise,
        /// when used in document metadata, indicates the title of the post or page.
        /// </summary>
        /// <scope>Global</scope>
        /// <scope>Document</scope>
        /// <type><see cref="string"/></type>
        public const string Title = nameof(Title);

        /// <summary>
        /// When used in global metadata, the relative path to an image to display on the
        /// home page. When used in document metadata, the relative path to an image for
        /// the current post or page (often shown in the header of the page).
        /// </summary>
        /// <scope>Global</scope>
        /// <scope>Document</scope>
        /// <type><see cref="string"/></type>
        public const string Image = nameof(Image);

        /// <summary>
        /// When used in global metadata, this changes the header and nav bar text color on the
        /// home page. When used in document metadata, this changes the header and nav bar text color on
        /// the current post or page. The value should be a valid CSS color and you should surround it
        /// in quotes when defining in front matter. This setting has no effect in themes where the header
        /// text is not over an image.
        /// </summary>
        /// <scope>Global</scope>
        /// <scope>Document</scope>
        /// <type><see cref="string"/></type>
        public const string HeaderTextColor = nameof(HeaderTextColor);

        /// <summary>
        /// The description of your blog (usually placed on the home page).
        /// </summary>
        /// <scope>Global</scope>
        /// <type><see cref="string"/></type>
        public const string Description = nameof(Description);

        /// <summary>
        /// A short introduction to your blog (usually placed on the home page
        /// under the description).
        /// </summary>
        /// <scope>Global</scope>
        /// <type><see cref="string"/></type>
        public const string Intro = nameof(Intro);

        /// <summary>
        /// Controls the parent path where blog posts are placed. The default is "posts".
        /// This affects both input and output files (I.e., if you change this your input
        /// files must also be under the same path).
        /// </summary>
        /// <scope>Global</scope>
        /// <type><see cref="DirectoryPath"/> or <see cref="string"/></type>
        public const string PostsPath = nameof(PostsPath);

        /// <summary>
        /// Set to <c>true</c> to make tag groupings case-insensitive.
        /// </summary>
        /// <scope>Global</scope>
        /// <type><see cref="bool"/></type>
        public const string CaseInsensitiveTags = nameof(CaseInsensitiveTags);

        /// <summary>
        /// Set this to control the activated set of Markdown extensions for the
        /// Markdig Markdown renderer. The default value is "advanced+bootstrap".
        /// </summary>
        /// <scope>Global</scope>
        /// <type><see cref="string"/></type>
        public const string MarkdownExtensions = nameof(MarkdownExtensions);

        /// <summary>
        /// Set this to add extension Markdown extensions for the Markdig Markdown
        /// renderer. The default value is null;
        /// </summary>
        /// <scope>Global</scope>
        /// <type><see cref="IEnumerable{IMarkDownExtension}"/></type>
        public const string MarkdownExternalExtensions = nameof(MarkdownExternalExtensions);

        /// <summary>
        /// Setting this to <c>true</c> uses
        /// the year and date in the output path of blog posts.
        /// The default value is <c>false</c>.
        /// </summary>
        /// <scope>Global</scope>
        /// <type><see cref="bool"/></type>
        public const string IncludeDateInPostPath = nameof(IncludeDateInPostPath);

        /// <summary>
        /// Set to <c>true</c> (the default value) to generate meta refresh pages
        /// for any redirected documents (as indicated by a <c>RedirectFrom</c>
        /// metadata value in the document).
        /// </summary>
        /// <scope>Global</scope>
        /// <type><see cref="bool"/></type>
        public const string MetaRefreshRedirects = nameof(MetaRefreshRedirects);

        /// <summary>
        /// Set to <c>true</c> (the default value is <c>false</c>) to generate
        /// a Netlify <c>_redirects</c> file from redirected documents
        /// (as indicated by a <c>RedirectFrom</c> metadata value).
        /// </summary>
        /// <scope>Global</scope>
        /// <type><see cref="bool"/></type>
        public const string NetlifyRedirects = nameof(NetlifyRedirects);

        /// <summary>
        /// Specifies the path where the blog RSS file will be output.
        /// The default value is <c>feed.rss</c>. Set to <c>null</c>
        /// to prevent generating an RSS feed.
        /// </summary>
        /// <scope>Global</scope>
        /// <type><see cref="FilePath"/> or <see cref="string"/></type>
        public const string RssPath = nameof(RssPath);

        /// <summary>
        /// Specifies the path where the blog Atom file will be output.
        /// The default value is <c>feed.atom</c>. Set to <c>null</c>
        /// to prevent generating an Atom feed.
        /// </summary>
        /// <scope>Global</scope>
        /// <type><see cref="FilePath"/> or <see cref="string"/></type>
        public const string AtomPath = nameof(AtomPath);

        /// <summary>
        /// Specifies the path where the blog RDF file will be output.
        /// The default value is <c>null</c> which
        /// prevents generating an RDF feed.
        /// </summary>
        /// <scope>Global</scope>
        /// <type><see cref="FilePath"/> or <see cref="string"/></type>
        public const string RdfPath = nameof(RdfPath);

        /// <summary>
        /// Set to <c>true</c> (the default value is <c>false</c>) to
        /// validate all absolute links. Note that this may add considerable
        /// time to your generation process.
        /// </summary>
        /// <scope>Global</scope>
        /// <type><see cref="bool"/></type>
        public const string ValidateAbsoluteLinks = nameof(ValidateAbsoluteLinks);

        /// <summary>
        /// Set to <c>true</c> (the default value) to
        /// validate all relative links.
        /// </summary>
        /// <scope>Global</scope>
        /// <type><see cref="bool"/></type>
        public const string ValidateRelativeLinks = nameof(ValidateRelativeLinks);

        /// <summary>
        /// Set to <c>true</c> (the default value is <c>false</c>) to
        /// report errors on link validation failures.
        /// </summary>
        /// <scope>Global</scope>
        /// <type><see cref="bool"/></type>
        public const string ValidateLinksAsError = nameof(ValidateLinksAsError);

        // ***Document

        /// <summary>
        /// The date of the post.
        /// </summary>
        /// <scope>Document</scope>
        /// <type><see cref="DateTime"/> or <see cref="string"/></type>
        public const string Published = nameof(Published);

        /// <summary>
        /// The tags for a given post.
        /// </summary>
        /// <scope>Document</scope>
        /// <type><see cref="string"/> or <c>IEnumerable&lt;string&gt;</c></type>
        public const string Tags = nameof(Tags);

        /// <summary>
        /// A short description of a particular blog post.
        /// </summary>
        /// <scope>Document</scope>
        /// <type><see cref="string"/></type>
        public const string Lead = nameof(Lead);

        /// <summary>
        /// An excerpt of the blog post, automatically set for each document
        /// by the recipe.
        /// </summary>
        /// <scope>Document</scope>
        /// <type><see cref="string"/></type>
        public const string Excerpt = nameof(Excerpt);

        /// <summary>
        /// Set to <c>true</c> to hide a particular page from the top-level navigation bar.
        /// </summary>
        /// <scope>Document</scope>
        /// <type><see cref="bool"/></type>
        public const string ShowInNavbar = nameof(ShowInNavbar);

        /// <summary>
        /// Set by the recipe to the content of the post (without any of the wrapping HTML elements).
        /// Used primarily by the feed generation module to ensure feed items don't include the whole layout.
        /// </summary>
        /// <scope>Document</scope>
        /// <type><see cref="string"/></type>
        public const string Content = nameof(Content);

        /// <summary>
        /// Set by the recipe for tag groups. Contains the set of documents with a given tag.
        /// </summary>
        /// <scope>Document</scope>
        /// <type><c>IEnumerable&lt;IDocument&gt;</c></type>
        public const string Posts = nameof(Posts);

        /// <summary>
        /// Set by the recipe to the name of the tag for each tag group.
        /// </summary>
        /// <scope>Document</scope>
        /// <type><see cref="string"/></type>
        public const string Tag = nameof(Tag);
    }
}
