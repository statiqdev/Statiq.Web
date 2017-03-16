using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Common.Meta
{
    /// <summary>
    /// Common metadata keys for modules in Wyam.Core.
    /// </summary>
    public static class Keys
    {
        // Settings

        /// <summary>
        /// The host to use when generating links.
        /// </summary>
        public const string Host = nameof(Host);

        /// <summary>
        /// Indicates if generated links should use HTTPS instead of HTTP as the scheme.
        /// </summary>
        public const string LinksUseHttps = nameof(LinksUseHttps);

        /// <summary>
        /// The default root path to use when generating links.
        /// </summary>
        public const string LinkRoot = nameof(LinkRoot);

        /// <summary>
        /// Indicates whether to hide index pages by default when generating links.
        /// </summary>
        public const string LinkHideIndexPages = nameof(LinkHideIndexPages);

        /// <summary>
        /// Indicates whether to hide ".html" and ".htm" extensions by default when generating links.
        /// </summary>
        public const string LinkHideExtensions = nameof(LinkHideExtensions);

        /// <summary>
        /// Indicates whether caching should be used.
        /// </summary>
        public const string UseCache = nameof(UseCache);

        /// <summary>
        /// Indicates whether to clean the output path on each execution.
        /// </summary>
        public const string CleanOutputPath = nameof(CleanOutputPath);

        /// <summary>
        /// Specifies that temporary files should be created to store document content. This reduces
        /// memory pressure for extremly large generations by not storing document content strings
        /// in memory. The tradeoff is performance since file I/O is much slower than memory.
        /// </summary>
        public const string UseTempContentFiles = nameof(UseTempContentFiles);

        // ReadFile/WriteFiles/CopyFiles

        /// <summary>
        /// The absolute Path to the folder of the original file.
        /// </summary>
        public const string SourceFileRoot = nameof(SourceFileRoot);

        /// <summary>
        /// The name of the original file without extension.
        /// </summary>
        public const string SourceFileBase = nameof(SourceFileBase);

        /// <summary>
        /// The extension of the original file (including the .).
        /// </summary>
        public const string SourceFileExt = nameof(SourceFileExt);

        /// <summary>
        /// The file name of the original file with extension.
        /// </summary>
        public const string SourceFileName = nameof(SourceFileName);

        /// <summary>
        /// The absolute Path to the folder of the original file.
        /// </summary>
        public const string SourceFileDir = nameof(SourceFileDir);

        /// <summary>
        /// The absolute path to the original file.
        /// </summary>
        public const string SourceFilePath = nameof(SourceFilePath);

        /// <summary>
        /// The absolute path to the original file without the file extension.
        /// </summary>
        public const string SourceFilePathBase = nameof(SourceFilePathBase);

        /// <summary>
        /// The path to the original file relative to the input folder.
        /// </summary>
        public const string RelativeFilePath = nameof(RelativeFilePath);

        /// <summary>
        /// The path to the original file relative to the input folder without extension.
        /// </summary>
        public const string RelativeFilePathBase = nameof(RelativeFilePathBase);

        /// <summary>
        /// The path to the original files folder relative to the input folder.
        /// </summary>
        public const string RelativeFileDir = nameof(RelativeFileDir);

        public const string DestinationFileBase = nameof(DestinationFileBase);
        public const string DestinationFileExt = nameof(DestinationFileExt);
        public const string DestinationFileName = nameof(DestinationFileName);
        public const string DestinationFileDir = nameof(DestinationFileDir);
        public const string DestinationFilePath = nameof(DestinationFilePath);
        public const string DestinationFilePathBase = nameof(DestinationFilePathBase);
        public const string WriteExtension = nameof(WriteExtension);
        public const string WriteFileName = nameof(WriteFileName);
        public const string WritePath = nameof(WritePath);

        // Paginate
        public const string PageDocuments = nameof(PageDocuments);
        public const string CurrentPage = nameof(CurrentPage);
        public const string TotalPages = nameof(TotalPages);
        public const string TotalItems = nameof(TotalItems);
        public const string HasNextPage = nameof(HasNextPage);
        public const string HasPreviousPage = nameof(HasPreviousPage);

        // GroupBy
        public const string GroupDocuments = nameof(GroupDocuments);
        public const string GroupKey = nameof(GroupKey);

        // Index
        public const string Index = nameof(Index);

        // Sitemap
        public const string SitemapItem = nameof(SitemapItem);

        // Download
        public const string SourceUri = nameof(SourceUri);
        public const string SourceHeaders = nameof(SourceHeaders);

        // Tree
        public const string Parent = nameof(Parent);
        public const string Children = nameof(Children);
        public const string PreviousSibling = nameof(PreviousSibling);
        public const string NextSibling = nameof(NextSibling);
        public const string Next = nameof(Next);
        public const string Previous = nameof(Previous);
        public const string TreePath = nameof(TreePath);

        // Title
        public const string Title = nameof(Title);

        // RedirectFrom
        public const string RedirectFrom = nameof(RedirectFrom);
    }
}
