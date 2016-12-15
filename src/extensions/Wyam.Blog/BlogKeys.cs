using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Blog
{
    public static class BlogKeys
    {
        // ***Global

        /// <summary>
        /// When used in global metadata, indicates the title of your blog. Otherwise,
        /// when used in document metadata, indicates the title of the post or page.
        /// </summary>
        /// <scope>Global</scope>
        /// <scope>Document</scope>
        /// <type><see cref="string"/></type>
        public const string Title = nameof(Title);

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
        /// The relative path to an image for the post or page (often shown in the header of the page).
        /// </summary>
        /// <scope>Document</scope>
        /// <type><see cref="string"/></type>
        public const string Image = nameof(Image);

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
