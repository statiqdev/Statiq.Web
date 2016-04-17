using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.IO;

namespace Wyam.Core.Execution
{
    internal static class LinkGenerator
    {
        public static string GetLink(NormalizedPath path, string host, DirectoryPath root, bool hideIndexPages, bool hideWebExtensions)
        {
            // Remove index pages and extensions if a file path
            FilePath filePath = path as FilePath;
            if (filePath != null)
            {
                if (hideIndexPages && (filePath.FileName.FullPath == "index.htm" || filePath.FileName.FullPath == "index.html"))
                {
                    path = filePath.Directory;
                }
                else if (hideWebExtensions && (filePath.Extension == ".htm" || filePath.Extension == ".html"))
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
                Path = rootLink + link
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
