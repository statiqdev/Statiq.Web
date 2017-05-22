using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.IO;

namespace Wyam.Common.Meta
{
    /// <summary>
    /// Common metadata keys for modules in the core library.
    /// </summary>
    public static class Keys
    {
        /// <summary>
        /// The host to use when generating links.
        /// </summary>
        /// <type><see cref="string"/></type>
        public const string Host = nameof(Host);

        /// <summary>
        /// Indicates if generated links should use HTTPS instead of HTTP as the scheme.
        /// </summary>
        /// <type><see cref="bool"/></type>
        public const string LinksUseHttps = nameof(LinksUseHttps);

        /// <summary>
        /// The default root path to use when generating links.
        /// </summary>
        /// <type><see cref="string"/></type>
        public const string LinkRoot = nameof(LinkRoot);

        /// <summary>
        /// Indicates whether to hide index pages by default when generating links.
        /// </summary>
        /// <type><see cref="bool"/></type>
        public const string LinkHideIndexPages = nameof(LinkHideIndexPages);

        /// <summary>
        /// Indicates whether to hide ".html" and ".htm" extensions by default when generating links.
        /// </summary>
        /// <type><see cref="bool"/></type>
        public const string LinkHideExtensions = nameof(LinkHideExtensions);

        /// <summary>
        /// Indicates whether caching should be used.
        /// </summary>
        /// <type><see cref="bool"/></type>
        public const string UseCache = nameof(UseCache);

        /// <summary>
        /// Indicates whether to clean the output path on each execution.
        /// </summary>
        /// <type><see cref="bool"/></type>
        public const string CleanOutputPath = nameof(CleanOutputPath);

        /// <summary>
        /// Indicates the culture to use for reading and interpreting dates as input.
        /// </summary>
        /// <type><see cref="string"/> or <see cref="CultureInfo"/></type>
        public const string DateTimeInputCulture = nameof(DateTimeInputCulture);

        /// <summary>
        /// Indicates the culture to use for displaying dates in output.
        /// </summary>
        /// <type><see cref="string"/> or <see cref="CultureInfo"/></type>
        public const string DateTimeDisplayCulture = nameof(DateTimeDisplayCulture);

        // ReadFile/WriteFiles/CopyFiles

        /// <summary>
        /// The absolute root search path without any nested directories
        /// (I.e., the path that was searched, and possibly descended, for the given pattern).
        /// </summary>
        /// <type><see cref="DirectoryPath"/></type>
        public const string SourceFileRoot = nameof(SourceFileRoot);

        /// <summary>
        /// The name of the original file without extension.
        /// </summary>
        /// <type><see cref="FilePath"/></type>
        public const string SourceFileBase = nameof(SourceFileBase);

        /// <summary>
        /// The extension of the original file (including the ".").
        /// </summary>
        /// <type><see cref="string"/></type>
        public const string SourceFileExt = nameof(SourceFileExt);

        /// <summary>
        /// The file name of the original file with extension.
        /// </summary>
        /// <type><see cref="FilePath"/></type>
        public const string SourceFileName = nameof(SourceFileName);

        /// <summary>
        /// The absolute path to the folder of the original file.
        /// </summary>
        /// <type><see cref="DirectoryPath"/></type>
        public const string SourceFileDir = nameof(SourceFileDir);

        /// <summary>
        /// The absolute path to the original file.
        /// </summary>
        /// <type><see cref="FilePath"/></type>
        public const string SourceFilePath = nameof(SourceFilePath);

        /// <summary>
        /// The absolute path to the original file without the file extension.
        /// </summary>
        /// <type><see cref="FilePath"/></type>
        public const string SourceFilePathBase = nameof(SourceFilePathBase);

        /// <summary>
        /// The path to the file relative to the input folder. This metadata
        /// value is used when generating links to the document.
        /// </summary>
        /// <type><see cref="FilePath"/></type>
        public const string RelativeFilePath = nameof(RelativeFilePath);

        /// <summary>
        /// The path to the file relative to the input folder without extension.
        /// </summary>
        /// <type><see cref="FilePath"/></type>
        public const string RelativeFilePathBase = nameof(RelativeFilePathBase);

        /// <summary>
        /// The path to the folder containing the file relative to the input folder.
        /// </summary>
        /// <type><see cref="DirectoryPath"/></type>
        public const string RelativeFileDir = nameof(RelativeFileDir);

        /// <summary>
        /// The file name without any extension. Equivalent
        /// to <c>Path.GetFileNameWithoutExtension(DestinationFilePath)</c>.
        /// </summary>
        /// <type><see cref="FilePath"/></type>
        public const string DestinationFileBase = nameof(DestinationFileBase);

        /// <summary>
        /// The extension of the file. Equivalent
        /// to <c>Path.GetExtension(DestinationFilePath)</c>.
        /// </summary>
        /// <type><see cref="string"/></type>
        public const string DestinationFileExt = nameof(DestinationFileExt);

        /// <summary>
        /// The full file name. Equivalent
        /// to <c>Path.GetFileName(DestinationFilePath)</c>.
        /// </summary>
        /// <type><see cref="FilePath"/></type>
        public const string DestinationFileName = nameof(DestinationFileName);

        /// <summary>
        /// The full absolute directory of the file.
        /// Equivalent to <c>Path.GetDirectoryName(DestinationFilePath)</c>.
        /// </summary>
        /// <type><see cref="DirectoryPath"/></type>
        public const string DestinationFileDir = nameof(DestinationFileDir);

        /// <summary>
        /// The full absolute path (including file name)
        /// of the destination file.
        /// </summary>
        /// <type><see cref="FilePath"/></type>
        public const string DestinationFilePath = nameof(DestinationFilePath);

        /// <summary>
        /// The full absolute path (including file name)
        /// of the destination file without the file extension.
        /// </summary>
        /// <type><see cref="FilePath"/></type>
        public const string DestinationFilePathBase = nameof(DestinationFilePathBase);

        /// <summary>
        /// The extension to use when writing the file.
        /// </summary>
        /// <type><see cref="string"/></type>
        public const string WriteExtension = nameof(WriteExtension);

        /// <summary>
        /// The file name to use when writing the file.
        /// </summary>
        /// <type><see cref="FilePath"/></type>
        public const string WriteFileName = nameof(WriteFileName);

        /// <summary>
        /// The path to use when writing the file.
        /// </summary>
        /// <type><see cref="FilePath"/></type>
        public const string WritePath = nameof(WritePath);

        // Paginate

        /// <summary>
        /// Contains all the documents for the current page.
        /// </summary>
        /// <type><c>IEnumerable&lt;IDocument&gt;</c></type>
        public const string PageDocuments = nameof(PageDocuments);

        /// <summary>
        /// The index of the current page (1 based).
        /// </summary>
        /// <type><see cref="int"/></type>
        public const string CurrentPage = nameof(CurrentPage);

        /// <summary>
        /// The total number of pages.
        /// </summary>
        /// <type><see cref="int"/></type>
        public const string TotalPages = nameof(TotalPages);

        /// <summary>
        /// The total number of items across all pages.
        /// </summary>
        /// <type><see cref="int"/></type>
        public const string TotalItems = nameof(TotalItems);

        /// <summary>
        /// Whether there is another page after this one.
        /// </summary>
        /// <type><see cref="bool"/></type>
        public const string HasNextPage = nameof(HasNextPage);

        /// <summary>
        /// Whether there is another page before this one.
        /// </summary>
        /// <type><see cref="bool"/></type>
        public const string HasPreviousPage = nameof(HasPreviousPage);

        /// <summary>
        /// The next page.
        /// </summary>
        /// <type><see cref="IDocument"/></type>
        public const string NextPage = nameof(NextPage);

        /// <summary>
        /// The previous page.
        /// </summary>
        /// <type><see cref="IDocument"/></type>
        public const string PreviousPage = nameof(PreviousPage);

        // GroupBy

        /// <summary>
        /// Contains all the documents for the current group.
        /// </summary>
        /// <type><c>IEnumerable&lt;IDocument&gt;</c></type>
        public const string GroupDocuments = nameof(GroupDocuments);

        /// <summary>
        /// The key for the current group.
        /// </summary>
        /// <type><see cref="object"/></type>
        public const string GroupKey = nameof(GroupKey);

        // Index

        /// <summary>
        /// The one-based index of the current document relative to other documents in the pipeline.
        /// </summary>
        /// <type><see cref="int"/></type>
        public const string Index = nameof(Index);

        // Sitemap

        /// <summary>
        /// Contains a document-specific sitemap item for use when generating a sitemap.
        /// </summary>
        /// <type><see cref="Modules.Contents.SitemapItem"/></type>
        public const string SitemapItem = nameof(SitemapItem);

        // Download

        /// <summary>
        /// The URI where the document was downloaded from.
        /// </summary>
        /// <type><see cref="string"/></type>
        public const string SourceUri = nameof(SourceUri);

        /// <summary>
        /// The web headers of the document.
        /// </summary>
        /// <type><c>Dictionary&lt;string, string&gt;</c></type>
        public const string SourceHeaders = nameof(SourceHeaders);

        // Tree

        /// <summary>
        /// The parent of this node or <c>null</c> if it is a root.
        /// </summary>
        /// <type><see cref="IDocument"/></type>
        public const string Parent = nameof(Parent);

        /// <summary>
        /// All the children of this node.
        /// </summary>
        /// <type><see cref="IReadOnlyCollection{IDocument}"/></type>
        public const string Children = nameof(Children);

        /// <summary>
        /// The previous sibling, that is the previous node in the children
        /// collection of the parent or <c>null</c> if this is the first node in the collection or the parent is null.
        /// </summary>
        /// <type><see cref="IDocument"/></type>
        public const string PreviousSibling = nameof(PreviousSibling);

        /// <summary>
        /// The next sibling, that is the next node in the children collection
        /// of the parent or <c>null</c> if this is the last node in the collection or the parent is null.
        /// </summary>
        /// <type><see cref="IDocument"/></type>
        public const string NextSibling = nameof(NextSibling);

        /// <summary>
        /// The next node in the tree using a depth-first
        /// search or <c>null</c> if this was the last node.
        /// </summary>
        /// <type><see cref="IDocument"/></type>
        public const string Next = nameof(Next);

        /// <summary>
        /// The previous node in the tree using a depth-first
        /// search or <c>null</c> if this was the first node.
        /// </summary>
        /// <type><see cref="IDocument"/></type>
        public const string Previous = nameof(Previous);

        /// <summary>
        /// The path that represents this node in the tree.
        /// </summary>
        /// <type><see cref="Array"/></type>
        public const string TreePath = nameof(TreePath);

        // Title

        /// <summary>
        /// The calculated title of the document.
        /// </summary>
        /// <type><see cref="string"/></type>
        public const string Title = nameof(Title);

        // RedirectFrom

        /// <summary>
        /// The path(s) where the document should be redirected from.
        /// </summary>
        /// <type><see cref="FilePath"/></type>
        public const string RedirectFrom = nameof(RedirectFrom);
    }
}
