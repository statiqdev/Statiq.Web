namespace Wyam.Core.Documents
{
    // Other modules outside Core might refer to these as strings, so make sure to do a text search if renaming
    public static class MetadataKeys
    {
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
        public const string RelativeDestinationFilePath = "RelativeDestinationFilePath";
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
    }
}
