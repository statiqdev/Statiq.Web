using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Common.Meta
{
    /// <summary>
    /// Common metadata keys for modules in Wyam.Core. The keys available in this class
    /// might lag behind to full set of available keys since Wyam.Common doesn't update
    /// as often. This set of keys will be updated whenever a new version of Wyam.Common
    /// is released for other reasons.
    /// </summary>
    public static class Keys
    {
        // Common
        public const string Hostname = "Hostname";

        // ReadFile/WriteFiles/CopyFiles
        public const string SourceFileRoot = "SourceFileRoot";
        public const string SourceFileBase = "SourceFileBase";
        public const string SourceFileExt = "SourceFileExt";
        public const string SourceFileName = "SourceFileName";
        public const string SourceFileDir = "SourceFileDir";
        public const string SourceFilePath = "SourceFilePath";
        public const string SourceFilePathBase = "SourceFilePathBase";
        public const string RelativeFilePath = "RelativeFilePath";
        public const string RelativeFilePathBase = "RelativeFilePathBase";
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

        // Sitemap
        public const string SitemapItem = "SitemapItem";

        // Download
        public static string SourceUri = "SourceUri";
        public static string SourceHeaders = "SourceHeaders";
    }
}
