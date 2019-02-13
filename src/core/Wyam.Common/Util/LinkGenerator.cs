using System;
using System.Linq;
using Wyam.Common.IO;

namespace Wyam.Common.Util
{
    /// <summary>
    /// Helps generate normalized links.
    /// </summary>
    public static class LinkGenerator
    {
        /// <summary>
        /// The default page names to hide in links.
        /// </summary>
        public static readonly string[] DefaultHidePages = { "index" };

        /// <summary>
        /// The default extensions to hide in links.
        /// </summary>
        public static readonly string[] DefaultHideExtensions = { ".htm", ".html" };

        /// <summary>
        /// Generates a normalized link given a path and other conditions.
        /// </summary>
        /// <param name="path">The path to get a link for.</param>
        /// <param name="host">The host for the link (or <c>null</c> to omit the host).</param>
        /// <param name="root">The root path for the link (or <c>null</c> for no root path).</param>
        /// <param name="scheme">The scheme for the link (or <c>null</c> for "http").</param>
        /// <param name="hidePages">An array of page names to hide (or <c>null</c> to not hide any pages).</param>
        /// <param name="hideExtensions">An array of file extensions to hide (or <c>null</c> to not hide extensions or an empty array to hide all file extensions).</param>
        /// <param name="lowercase">Indicates that the link should be rendered in all lowercase.</param>
        /// <returns>A generated link.</returns>
        public static string GetLink(NormalizedPath path, string host, DirectoryPath root, string scheme, string[] hidePages, string[] hideExtensions, bool lowercase)
        {
            // Remove index pages and extensions if a file path
            FilePath filePath = path as FilePath;
            if (filePath != null)
            {
                if (hidePages != null && filePath.FullPath != "/"
                    && hidePages.Where(x => x != null).Select(x => x.EndsWith(".") ? x : x + ".").Any(x => filePath.FileName.FullPath.StartsWith(x)))
                {
                    path = filePath.Directory;
                }
                else if (hideExtensions != null
                    && (hideExtensions.Length == 0 || hideExtensions.Where(x => x != null).Select(x => x.StartsWith(".") ? x : "." + x).Contains(filePath.Extension)))
                {
                    path = filePath.ChangeExtension(null);
                }
            }

            // Collapse the link to a string
            string link = string.Empty;
            if (path != null)
            {
                link = path.FullPath;
                if (string.IsNullOrWhiteSpace(link) || link == ".")
                {
                    link = "/";
                }
                if (!link.StartsWith("/"))
                {
                    link = "/" + link;
                }
            }

            // Collapse the root and combine
            string rootLink = root == null ? string.Empty : root.FullPath;
            if (rootLink.EndsWith("/"))
            {
                rootLink = rootLink.Substring(0, rootLink.Length - 1);
            }

            // Add the host and convert to URI for escaping
            UriBuilder builder = new UriBuilder
            {
                Path = rootLink + link,
                Scheme = scheme ?? "http"
            };
            bool hasHost = false;
            if (!string.IsNullOrWhiteSpace(host))
            {
                builder.Host = host;
                hasHost = true;
            }
            Uri uri = builder.Uri;
            string renderedLink = hasHost ? uri.AbsoluteUri : uri.AbsolutePath;
            return lowercase ? renderedLink.ToLowerInvariant() : renderedLink;
        }

        /// <summary>
        /// Checks if a string contains an absolute URI with a "http" or "https" scheme and returns it if it does.
        /// </summary>
        /// <param name="str">The string to check.</param>
        /// <param name="absoluteUri">The resulting absolute URI.</param>
        /// <returns><c>true</c> if the string contains an absolute URI, <c>false</c> otherwise.</returns>
        public static bool TryGetAbsoluteHttpUri(string str, out string absoluteUri)
        {
            if (!string.IsNullOrWhiteSpace(str))
            {
                // Return the actual URI if it's absolute
                if (Uri.TryCreate(str, UriKind.Absolute, out Uri uri)
                    && (uri.Scheme.Equals("http", StringComparison.OrdinalIgnoreCase)
                        || uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase)))
                {
                    absoluteUri = uri.ToString();
                    return true;
                }
            }
            absoluteUri = null;
            return false;
        }
    }
}
