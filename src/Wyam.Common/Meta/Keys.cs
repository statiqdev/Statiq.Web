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
        // ReadFile/WriteFiles/CopyFiles
        /// <summary>
        /// The absulute Path to the folder of the original file.
        /// </summary>
        public const string SourceFileRoot = "SourceFileRoot";
        /// <summary>
        /// The name of the orignal file without extension.
        /// </summary>
        public const string SourceFileBase = "SourceFileBase";
        /// <summary>
        /// The extension of the original file (including the .).
        /// </summary>
        public const string SourceFileExt = "SourceFileExt";
        /// <summary>
        /// The file name of the original file with extenion.
        /// </summary>
        public const string SourceFileName = "SourceFileName";
        /// <summary>
        /// The absulute Path to the folder of the original file.
        /// </summary>
        public const string SourceFileDir = "SourceFileDir";
        /// <summary>
        /// The absolute path to the original file.
        /// </summary>
        public const string SourceFilePath = "SourceFilePath";
        /// <summary>
        /// The absolute path to the original file without the file extension.
        /// </summary>
        public const string SourceFilePathBase = "SourceFilePathBase";
        /// <summary>
        /// The path to the original file relative to the input folder.
        /// </summary>
        public const string RelativeFilePath = "RelativeFilePath";
        /// <summary>
        /// The path to the original file relative to the input folder without extension.
        /// </summary>
        public const string RelativeFilePathBase = "RelativeFilePathBase";
        /// <summary>
        /// The path to the original files folder relative to the input folder.
        /// </summary>
        public const string RelativeFileDir = "RelativeFileDir";
        public const string DestinationFileBase = "DestinationFileBase";
        public const string DestinationFileExt = "DestinationFileExt";
        public const string DestinationFileName = "DestinationFileName";
        public const string DestinationFileDir = "DestinationFileDir";
        public const string DestinationFilePath = "DestinationFilePath";
        public const string DestinationFilePathBase = "DestinationFilePathBase";
        public const string WriteExtension = "WriteExtension";
        public const string WriteFileName = "WriteFileName";
        public const string WritePath = "WritePath";

        // Paginate
        public const string PageDocuments = "PageDocuments";
        public const string CurrentPage = "CurrentPage";
        public const string TotalPages = "TotalPages";
        public const string HasNextPage = "HasNextPage";
        public const string HasPreviousPage = "HasPreviousPage";

        // GroupBy
        public const string GroupDocuments = "GroupDocuments";
        public const string GroupKey = "GroupKey";

        // Index
        public const string Index = "Index";

        // Rss
        public const string RssItemUrl = "RssItemUrl";
        public const string IsRssFeed = "IsRssFeed";

        // Sitemap
        public const string SitemapItem = "SitemapItem";

        // Download
        public const string SourceUri = "SourceUri";
        public const string SourceHeaders = "SourceHeaders";

        // Tree
        public const string Parent = "Parent";
        public const string Children = "Children";
        public const string PreviousSibling = "PreviousSibling";
        public const string NextSibling = "NextSibling";
        public const string Next = "Next";
        public const string Previous = "Previous";
    }
}
