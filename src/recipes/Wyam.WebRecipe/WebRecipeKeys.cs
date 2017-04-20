using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.IO;

namespace Wyam.WebRecipe
{
    public static class WebRecipeKeys
    {
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
        /// This should be a string or array of strings with the name(s)
        /// of root-level folders to ignore when scanning for content pages.
        /// Setting this global metadata value is useful when introducing
        /// your own pipelines for files under certain folders and you don't
        /// want the primary content page pipelines to pick them up.
        /// </summary>
        /// <type><see cref="string"/> or <c>IEnumerable&lt;string&gt;</c></type>
        public const string IgnoreFolders = nameof(IgnoreFolders);

        /// <summary>
        /// Set by the system for documents that support editing. Contains the
        /// relative path to the document to be appended to the base edit URL.
        /// </summary>
        /// <type><see cref="FilePath"/></type>
        public const string EditFilePath = nameof(EditFilePath);

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
    }
}
