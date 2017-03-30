using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Common.Meta
{
    /// <summary>
    /// Common metadata keys for modules in the core library.
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
        /// Indicates the culture to use for reading and interpreting dates as input.
        /// </summary>
        public const string DateTimeInputCulture = nameof(DateTimeInputCulture);

        /// <summary>
        /// Indicates the culture to use for displaying dates in output.
        /// </summary>
        public const string DateTimeDisplayCulture = nameof(DateTimeDisplayCulture);

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

        /// <summary>
        /// The file name without any extension. Equivalent
        /// to <c>Path.GetFileNameWithoutExtension(DestinationFilePath)</c>.
        /// </summary>
        public const string DestinationFileBase = nameof(DestinationFileBase);

        /// <summary>
        /// The extension of the file. Equivalent
        /// to <c>Path.GetExtension(DestinationFilePath)</c>.
        /// </summary>
        public const string DestinationFileExt = nameof(DestinationFileExt);

        /// <summary>
        /// The full file name. Equivalent
        /// to <c>Path.GetFileName(DestinationFilePath)</c>.
        /// </summary>
        public const string DestinationFileName = nameof(DestinationFileName);

        /// <summary>
        /// The full absolute directory of the file.
        /// Equivalent to <c>Path.GetDirectoryName(DestinationFilePath)</c>.
        /// </summary>
        public const string DestinationFileDir = nameof(DestinationFileDir);

        /// <summary>
        /// The full absolute path (including file name)
        /// of the destination file.
        /// </summary>
        public const string DestinationFilePath = nameof(DestinationFilePath);

        /// <summary>
        /// The full absolute path (including file name)
        /// of the destination file without the file extension.
        /// </summary>
        public const string DestinationFilePathBase = nameof(DestinationFilePathBase);

        /// <summary>
        /// The extension to use when writing the file.
        /// </summary>
        public const string WriteExtension = nameof(WriteExtension);

        /// <summary>
        /// The file name to use when writing the file.
        /// </summary>
        public const string WriteFileName = nameof(WriteFileName);

        /// <summary>
        /// The path to use when writing the file.
        /// </summary>
        public const string WritePath = nameof(WritePath);

        // Paginate

        /// <summary>
        /// Contains all the documents for the current page.
        /// </summary>
        public const string PageDocuments = nameof(PageDocuments);

        /// <summary>
        /// The index of the current page (1 based).
        /// </summary>
        public const string CurrentPage = nameof(CurrentPage);

        /// <summary>
        /// The total number of pages.
        /// </summary>
        public const string TotalPages = nameof(TotalPages);

        /// <summary>
        /// The total number of items across all pages.
        /// </summary>
        public const string TotalItems = nameof(TotalItems);

        /// <summary>
        /// Whether there is another page after this one.
        /// </summary>
        public const string HasNextPage = nameof(HasNextPage);

        /// <summary>
        /// Whether there is another page before this one.
        /// </summary>
        public const string HasPreviousPage = nameof(HasPreviousPage);

        // GroupBy

        /// <summary>
        /// Contains all the documents for the current group.
        /// </summary>
        public const string GroupDocuments = nameof(GroupDocuments);

        /// <summary>
        /// The key for the current group.
        /// </summary>
        public const string GroupKey = nameof(GroupKey);

        // Index

        /// <summary>
        /// The one-based index of the current document relative to other documents in the pipeline.
        /// </summary>
        public const string Index = nameof(Index);

        // Sitemap

        /// <summary>
        /// Contains a document-specific sitemap item for use when generating a sitemap.
        /// </summary>
        public const string SitemapItem = nameof(SitemapItem);

        // Download

        /// <summary>
        /// The URI where the document was downloaded from.
        /// </summary>
        public const string SourceUri = nameof(SourceUri);

        /// <summary>
        /// The web headers of the document.
        /// </summary>
        public const string SourceHeaders = nameof(SourceHeaders);

        // Tree

        /// <summary>
        /// The parent of this node or <c>null</c> if it is a root.
        /// </summary>
        public const string Parent = nameof(Parent);

        /// <summary>
        /// All the children of this node.
        /// </summary>
        public const string Children = nameof(Children);

        /// <summary>
        /// The previous sibling, that is the previous node in the children
        /// collection of the parent or <c>null</c> if this is the first node in the collection or the parent is null.
        /// </summary>
        public const string PreviousSibling = nameof(PreviousSibling);

        /// <summary>
        /// The next sibling, that is the next node in the children collection
        /// of the parent or <c>null</c> if this is the last node in the collection or the parent is null.
        /// </summary>
        public const string NextSibling = nameof(NextSibling);

        /// <summary>
        /// The next node in the tree using a depth-first
        /// search or <c>null</c> if this was the last node.
        /// </summary>
        public const string Next = nameof(Next);

        /// <summary>
        /// The previous node in the tree using a depth-first
        /// search or <c>null</c> if this was the first node.
        /// </summary>
        public const string Previous = nameof(Previous);

        /// <summary>
        /// The path that represents this node in the tree.
        /// </summary>
        public const string TreePath = nameof(TreePath);

        // Title

        /// <summary>
        /// The calculated title of the document.
        /// </summary>
        public const string Title = nameof(Title);

        // RedirectFrom

        /// <summary>
        /// The path(s) where the document should be redirected from.
        /// </summary>
        public const string RedirectFrom = nameof(RedirectFrom);
    }
}
