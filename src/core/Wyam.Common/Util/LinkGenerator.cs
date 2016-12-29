using System;
using System.Linq;
using Wyam.Common.IO;

namespace Wyam.Common.Util
{
    public static class LinkGenerator
    {
        /// <summary>
        /// Generates a normalized link given a path and other conditions.
        /// </summary>
        /// <param name="path">The path to get a link for.</param>
        /// <param name="host">The host for the link (or <c>null</c> to omit the host).</param>
        /// <param name="root">The root path for the link (or <c>null</c> for no root path).</param>
        /// <param name="scheme">The scheme for the link (or <c>null</c> for "http").</param>
        /// <param name="hidePages">An array of page names to hide (or <c>null</c> to not hide any pages).</param>
        /// <param name="hideExtensions">An array of file extensions to hide (or <c>null</c> to not hide extensions or an empty array to hide all file extensions).</param>
        /// <returns>A generated link.</returns>
        public static string GetLink(NormalizedPath path, string host, DirectoryPath root, string scheme, string[] hidePages, string[] hideExtensions)
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
            if(path != null)
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
            return hasHost ? uri.AbsoluteUri : uri.AbsolutePath;
        }
    }
}
