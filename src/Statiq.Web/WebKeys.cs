using System.Collections.Generic;
using Statiq.Common;
using Statiq.Lunr;

namespace Statiq.Web
{
    public static class WebKeys
    {
        ////////// Global

        /// <summary>
        /// The globbing pattern(s) used to read input files.
        /// </summary>
        /// <remarks>
        /// The files will be processed by the appropriate pipeline based on their
        /// media type and registered <see cref="Templates"/>. Files that don't match
        /// templates for data or content are treated as assets.
        /// </remarks>
        public const string InputFiles = nameof(InputFiles);

        /// <summary>
        /// The globbing pattern(s) that will be used to filter directory metadata files.
        /// </summary>
        /// <remarks>
        /// Any files that match the pattern will be further filtered by media types
        /// supported by <see cref="Templates"/> that match <see cref="ContentType.Data"/>.
        /// </remarks>
        public const string DirectoryMetadataFiles = nameof(DirectoryMetadataFiles);

        public const string OptimizeContentFileNames = nameof(OptimizeContentFileNames);

        public const string OptimizeDataFileNames = nameof(OptimizeDataFileNames);

        /// <summary>
        /// Set to <c>false</c> to prevent processing directory metadata.
        /// </summary>
        public const string ApplyDirectoryMetadata = nameof(ApplyDirectoryMetadata);

        /// <summary>
        /// Set to <c>false</c> to prevent processing sidecar files.
        /// </summary>
        public const string ProcessSidecarFiles = nameof(ProcessSidecarFiles);

        /// <summary>
        /// Indicates that a sitemap file should be generated if <c>true</c> (the default).
        /// </summary>
        public const string GenerateSitemap = nameof(GenerateSitemap);

        /// <summary>
        /// Set to <c>false</c> to exclude a document from the sitemap file.
        /// </summary>
        public const string IncludeInSitemap = nameof(IncludeInSitemap);

        /// <summary>
        /// Indicates that a client-side Lunr search index file should be generated if <c>true</c> (the default is <c>false</c>).
        /// </summary>
        public const string GenerateSearchIndex = nameof(GenerateSearchIndex);

        /// <summary>
        /// The destination path of the client-side Lunr search file ("search.js" by default).
        /// </summary>
        public const string SearchScriptPath = nameof(SearchScriptPath);

        /// <summary>
        /// Indicates whether the search index file should be gzipped (the default is <c>true</c>).
        /// If this is <c>true</c>, the JavaScript library pako must be included on the client to decompress the file.
        /// </summary>
        public const string ZipSearchIndexFile = nameof(ZipSearchIndexFile);

        /// <summary>
        /// Indicates whether the search results file should be gzipped (the default is <c>true</c>).
        /// If this is <c>true</c>, the JavaScript library pako must be included on the client to decompress the file.
        /// </summary>
        public const string ZipSearchResultsFile = nameof(ZipSearchResultsFile);

        /// <summary>
        /// If set to <c>true</c>, the <see cref="Keys.Host"/> will be included in the client-side Lunr search index links.
        /// </summary>
        public const string SearchIncludeHost = nameof(SearchIncludeHost);

        /// <summary>
        /// The metadata fields that will be searchable in addition to the default fields of "title", "content", and "tags".
        /// </summary>
        public const string AdditionalSearchableFields = nameof(AdditionalSearchableFields);

        /// <summary>
        /// The metadata fields that will be available in search results in addition to the default fields of "link" and "title".
        /// </summary>
        public const string AdditionalSearchResultFields = nameof(AdditionalSearchResultFields);

        /// <summary>
        /// Adds the "position" metadata to the metadata allowlist in the search index, which
        /// enables position information for each search term in the search results at the expense of index size.
        /// </summary>
        /// <remarks>
        /// See https://lunrjs.com/guides/core_concepts.html for more information.
        /// </remarks>
        public const string SearchAllowPositionMetadata = nameof(SearchAllowPositionMetadata);

        /// <summary>
        /// Specifies stops words to use for the search index. By default a pre-defined set of English stop words are used.
        /// </summary>
        public const string SearchStopWords = nameof(SearchStopWords);

        /// <summary>
        /// Specifies an input file that contains stop words to use. The file should contain
        /// one stop word per line.
        /// </summary>
        /// <remarks>
        /// If both this setting and <see cref="SearchStopWords"/> are specified, the defined
        /// stop words will be used and the file specified will be ignored.
        /// </remarks>
        public const string SearchStopWordsFilePath = nameof(SearchStopWordsFilePath);

        /// <summary>
        /// Turns on stemming for client-side search. By default an English stemmer is used
        /// (you must define your own <see cref="GenerateLunrIndex"/> module to specify alternate
        /// stemming behavior). Turning on stemming also results in the generated client search
        /// JavaScript file no longer supporting typeahead searches by default since stemming and
        /// wildcard searching don't work well together.
        /// </summary>
        public const string SearchStemming = nameof(SearchStemming);

        public const string MirrorResources = nameof(MirrorResources);

        /// <summary>
        /// Generates META-REFRESH redirect pages (the default value is <c>true</c>).
        /// </summary>
        public const string MetaRefreshRedirects = nameof(MetaRefreshRedirects);

        /// <summary>
        /// Generates a Netlify redirects file.
        /// </summary>
        public const string NetlifyRedirects = nameof(NetlifyRedirects);

        /// <summary>
        /// Theme paths specified as settings.
        /// </summary>
        /// <remarks>
        /// Note that paths are processed before the normal extensions in <see cref="BootstrapperSettingsExtensions"/>
        /// so you must use the <c>Initial...()</c> settings extensions, a setting file, or the extensions in
        /// <see cref="BootstrapperThemeExtensions"/> to change theme paths.
        /// </remarks>
        public const string ThemePaths = nameof(ThemePaths);

        /// <summary>
        /// Input directory paths specified as settings.
        /// </summary>
        /// <remarks>
        /// Note that paths are processed before the normal extensions in <see cref="BootstrapperSettingsExtensions"/>
        /// so you must use the <c>Initial...()</c> settings extensions, a setting file, or the extensions in
        /// <see cref="BootstrapperFileSystemExtensions"/> to change file system paths.
        /// </remarks>
        public const string InputPaths = nameof(InputPaths);

        /// <summary>
        /// Output directory path specified as a setting.
        /// </summary>
        /// <remarks>
        /// Note that paths are processed before the normal extensions in <see cref="BootstrapperSettingsExtensions"/>
        /// so you must use the <c>Initial...()</c> settings extensions, a setting file, or the extensions in
        /// <see cref="BootstrapperFileSystemExtensions"/> to change file system paths.
        /// </remarks>
        public const string OutputPath = nameof(OutputPath);

        /// <summary>
        /// Temp directory path specified as a setting.
        /// </summary>
        /// <remarks>
        /// Note that paths are processed before the normal extensions in <see cref="BootstrapperSettingsExtensions"/>
        /// so you must use the <c>Initial...()</c> settings extensions, a setting file, or the extensions in
        /// <see cref="BootstrapperFileSystemExtensions"/> to change file system paths.
        /// </remarks>
        public const string TempPath = nameof(TempPath);

        /// <summary>
        /// Cache directory path specified as a setting.
        /// </summary>
        /// <remarks>
        /// Note that paths are processed before the normal extensions in <see cref="BootstrapperSettingsExtensions"/>
        /// so you must use the <c>Initial...()</c> settings extensions, a setting file, or the extensions in
        /// <see cref="BootstrapperFileSystemExtensions"/> to change file system paths.
        /// </remarks>
        public const string CachePath = nameof(CachePath);

        /// <summary>
        /// Excluded directory paths specified as a setting.
        /// </summary>
        /// <remarks>
        /// Note that paths are processed before the normal extensions in <see cref="BootstrapperSettingsExtensions"/>
        /// so you must use the <c>Initial...()</c> settings extensions, a setting file, or the extensions in
        /// <see cref="BootstrapperFileSystemExtensions"/> to change file system paths.
        /// </remarks>
        public const string ExcludedPaths = nameof(ExcludedPaths);

        /// <summary>
        /// Additional paths to watch in preview mode (input paths are always watched).
        /// </summary>
        public const string WatchPaths = nameof(WatchPaths);

        /// <summary>
        /// The level at which to gather headings for a document (this can be set globally or per-document).
        /// </summary>
        public const string GatherHeadingsLevel = nameof(GatherHeadingsLevel);

        /// <summary>
        /// Converts relative links to absolute links.
        /// </summary>
        public const string MakeLinksAbsolute = nameof(MakeLinksAbsolute);

        /// <summary>
        /// Converts relative links to root-relative links.
        /// </summary>
        public const string MakeLinksRootRelative = nameof(MakeLinksRootRelative);

        /// <summary>
        /// Sets a semantic version range of Statiq Web that must be used.
        /// Particularly useful for themes to set a supported version range.
        /// </summary>
        public const string MinimumStatiqWebVersion = nameof(MinimumStatiqWebVersion);

        /// <summary>
        /// Indicates that generation-time code highlighting should be performed using the
        /// highlight.js library (the highlight.js CSS files still need to be included in
        /// the site).
        /// </summary>
        public const string HighlightCode = nameof(HighlightCode);

        /// <summary>
        /// Indicates that unspecified languages should be code highlighted. Highlighting
        /// unspecified languages is much more tim consuming, so turning this off may improve
        /// generation times if a large number of unspecified code blocks are present.
        /// No effect if <see cref="HighlightCode"/> is <c>false</c>. The default is <c>true</c>.
        /// </summary>
        public const string HighlightUnspecifiedLanguage = nameof(HighlightUnspecifiedLanguage);

        /// <summary>
        /// The regular expressions used to identify front matter. Change this setting if you want
        /// to define entirely new front matter expressions. Otherwise, set
        /// <see cref="AdditionalFrontMatterRegexes"/> if you want to keep the defaults and only
        /// want to add new patterns.
        /// </summary>
        public const string FrontMatterRegexes = nameof(FrontMatterRegexes);

        /// <summary>
        /// Additional regular expressions used to identify front matter.
        /// </summary>
        public const string AdditionalFrontMatterRegexes = nameof(AdditionalFrontMatterRegexes);

        /// <summary>
        /// The name(s) of pipelines to use for xref lookups (defaults to "content", "archives", "assets", "data", "feeds").
        /// Change this setting if you want to define entirely new xref pipelines. Otherwise, set
        /// <see cref="AdditionalXrefPipelines"/> if you want to keep the defaults and only want to add new pipelines.
        /// </summary>
        public const string XrefPipelines = nameof(XrefPipelines);

        /// <summary>
        /// The name(s) of additional pipelines to use for xref lookups
        /// (defaults to "content", "archives", "assets", "data", "feeds", and "api").
        /// </summary>
        public const string AdditionalXrefPipelines = nameof(AdditionalXrefPipelines);

        /// <summary>
        /// Normally invalid xref targets result in an error. Set this to <c>true</c> to output
        /// a warning instead and continue execution.
        /// </summary>
        public const string IgnoreInvalidXrefs = nameof(IgnoreInvalidXrefs);

        ////////// Document

        /// <summary>
        /// Indicates the type of content and thus which pipeline will process the file.
        /// </summary>
        /// <remarks>
        /// This will be automatically set based on the media type of the file and other metadata,
        /// but it can be further adjusted by setting manually. For example, if you have a ".cshtml"
        /// file but you want to copy it to the output folder instead of processing as a Razor file,
        /// set the <see cref="ContentType"/> to <see cref="ContentType.Asset"/>.
        /// </remarks>
        public const string ContentType = nameof(ContentType);

        /// <summary>
        /// Changes the media type of the document (normally this is interpreted from the file extension).
        /// </summary>
        /// <remarks>
        /// For example, to force script evaluation for a file that isn't ".cs" or ".csx", set the
        /// <see cref="MediaType"/> front matter to <see cref="MediaTypes.CSharp"/>.
        /// </remarks>
        public const string MediaType = nameof(MediaType);

        /// <summary>
        /// Set to <c>true</c> to indicate that the content contains a script that should be evaluated before processing normally.
        /// </summary>
        public const string Script = nameof(Script);

        /// <summary>
        /// Indicates that if a script file has a second extension such as "foo.md.csx" that the script extension should be removed
        /// and the preceding extension should be used to reset the media type after script evaluation, and thus the templates that
        /// will be executed (the default is <c>true</c>).
        /// </summary>
        public const string RemoveScriptExtension = nameof(RemoveScriptExtension);

        /// <summary>
        /// The title of the document, often used by themes.
        /// </summary>
        public const string Title = nameof(Title);

        /// <summary>
        /// The description of the document, often used by themes.
        /// </summary>
        public const string Description = nameof(Description);

        /// <summary>
        /// The author of the site, post, or page (can be set at any level).
        /// </summary>
        public const string Author = nameof(Author);

        public const string Image = nameof(Image);

        public const string Copyright = nameof(Copyright);

        /// <summary>
        /// The date the file or post was published.
        /// </summary>
        /// <remarks>
        /// If you want to use a different metadata key to represent published dates you can
        /// globally fetch a value from a different key by setting <see cref="Published"/>
        /// in settings to an evaluated metadata script like <c>=> SomeOtherKey</c>.
        /// The pipelines will ensure that the <see cref="Published"/> value is always set by
        /// either reading from an existing value or by attempting to determine a published
        /// date using the file name or (optionally) the file last modified time.
        /// </remarks>
        public const string Published = nameof(Published);

        public const string PublishedUsesLastModifiedDate = nameof(PublishedUsesLastModifiedDate);

        public const string Updated = nameof(Updated);

        /// <summary>
        /// Indicates the document should be excluded from pipeline processing if <c>true</c>.
        /// The default value looks at <see cref="Published"/>
        /// and filters out any future-dated content or data, though you can also define a value
        /// for this setting directly for each document.
        /// </summary>
        public const string Excluded = nameof(Excluded);

        /// <summary>
        /// Indicates that the file should be output.
        /// By default content and asset files are output and data files are not.
        /// </summary>
        public const string ShouldOutput = nameof(ShouldOutput);

        /// <summary>
        /// Set this to <c>true</c> to clear the content of data files after processing (the default is <c>false</c>).
        /// </summary>
        public const string ClearDataContent = nameof(ClearDataContent);

        /// <summary>
        /// Indicates the layout file that should be used for this document (used by the Razor template engine).
        /// </summary>
        public const string Layout = nameof(Layout);

        /// <summary>
        /// Specifies the cross-reference ID of the current document. If not explicitly provided, it will default
        /// to the title of the document with spaces replaced by underscores (which is derived from the source file name
        /// if no <see cref="Title"/> metadata is defined for the document).
        /// </summary>
        public const string Xref = nameof(Xref);

        /// <summary>
        /// Used with directory metadata to indicate if the metadata should be applied
        /// recursively to files in child directories (the default is <c>true</c>).
        /// </summary>
        public const string Recursive = nameof(Recursive);

        /// <summary>
        /// Indicates that post-process templates should be rendered (the default is <c>true</c>).
        /// </summary>
        /// <remarks>
        /// Set this to <c>false</c> for a document to prevent rendering post-process templates such as Razor.
        /// This can be helpful when you have small bits of content like Markdown that you want to render
        /// as HTML but not as an entire page so that it can be included in other pages.
        /// </remarks>
        public const string RenderPostProcessTemplates = nameof(RenderPostProcessTemplates);

        ////////// Archive

        /// <summary>
        /// The pipeline(s) to get documents for the archive from.
        /// Defaults to the <c>Content</c> pipeline if not defined.
        /// </summary>
        public const string ArchivePipelines = nameof(ArchivePipelines);

        /// <summary>
        /// A globbing pattern to filter documents from the
        /// archive pipeline(s) based on source path (or all documents from the pipeline(s) if not defined).
        /// </summary>
        public const string ArchiveSources = nameof(ArchiveSources);

        /// <summary>
        /// An additional metadata filter for documents from the
        /// archive pipeline(s) that should return a <c>bool</c>.
        /// </summary>
        public const string ArchiveFilter = nameof(ArchiveFilter);

        /// <summary>
        /// The key to use for generating archive groups. The
        /// source documents will be grouped by the key value(s).
        /// This can either be the name of a metadata key to use
        /// or a computed value that will get archive key values.
        /// If this is not defined, only a single archive index
        /// with the source documents will be generated.
        /// </summary>
        public const string ArchiveKey = nameof(ArchiveKey);

        /// <summary>
        /// An <c>IEqualityComparer&lt;object&gt;</c> to use for comparing archive groups.
        /// </summary>
        /// <remarks>
        /// A scripted metadata value can be used to return the appropriate comparer and
        /// the <see cref="IEqualityComparerExtensions.ToConvertingEqualityComparer{T}(IEqualityComparer{T})"/> extension method
        /// can be used to convert a typed comparer into a <c>IEqualityComparer&lt;object&gt;</c>.
        /// For example, to ignore case when generating an archive for tags, you can use
        /// <c>ArchiveKeyComparer: => StringComparer.OrdinalIgnoreCase.ToConvertingEqualityComparer()</c>.
        /// </remarks>
        public const string ArchiveKeyComparer = nameof(ArchiveKeyComparer);

        /// <summary>
        /// The number of items on each group page (or all group items if not defined).
        /// </summary>
        /// <remarks>
        /// The current page index is stored in the <c>Index</c> metadata value.
        /// </remarks>
        public const string ArchivePageSize = nameof(ArchivePageSize);

        /// <summary>
        /// The title of each group output document.
        /// </summary>
        /// <remarks>
        /// This is usually a computed value (starting with a
        /// <c>=&gt;</c>) that calculates the title based
        /// on the group key. If this value is not specified, the
        /// default title will be "[Archive Title] - [Group Key] (Page [Index (If Paged)])".
        /// </remarks>
        public const string ArchiveTitle = nameof(ArchiveTitle);

        /// <summary>
        /// The destination path of each group output document.
        /// </summary>
        /// <remarks>
        /// This is usually a computed value (starting with a
        /// <c>=&gt;</c>) that calculates the destination based
        /// on the group key. If this value is not specified, the
        /// default group destination will be
        /// "[Archive File Path]/[Archive File Name]/[Group Key]/[Index (If Paged)].html".
        /// The destination path of the archive index follows
        /// normal destination calculation and will be placed at
        /// the same relative path as the archive file or can be
        /// changed with metadata like <c>DestinationPath</c>.
        /// </remarks>
        public const string ArchiveDestination = nameof(ArchiveDestination);

        /// <summary>
        /// A metadata key that contains the value that sorting should be based on.
        /// </summary>
        /// <remarks>
        /// If both <see cref="ArchiveOrder"/> and <see cref="ArchiveOrderKey"/> are specified,
        /// <see cref="ArchiveOrderKey"/> will be applied first followed by <see cref="ArchiveOrder"/>.
        /// </remarks>
        public const string ArchiveOrderKey = nameof(ArchiveOrderKey);

        /// <summary>
        /// A value that sorting should be based on (I.e. using a computed value).
        /// </summary>
        /// <remarks>
        /// If both <see cref="ArchiveOrder"/> and <see cref="ArchiveOrderKey"/> are specified,
        /// <see cref="ArchiveOrderKey"/> will be applied first followed by <see cref="ArchiveOrder"/>.
        /// </remarks>
        public const string ArchiveOrder = nameof(ArchiveOrder);

        /// <summary>
        /// Indicates the archive should be sorted in descending order.
        /// </summary>
        public const string ArchiveOrderDescending = nameof(ArchiveOrderDescending);

        ////////// Feed

        public const string FeedPipelines = nameof(FeedPipelines);

        public const string FeedSources = nameof(FeedSources);

        public const string FeedFilter = nameof(FeedFilter);

        public const string FeedOrderKey = nameof(FeedOrderKey);  // The key that should be sorted

        public const string FeedOrderDescending = nameof(FeedOrderDescending);  // Indicates the archive should be sorted in descending order

        public const string FeedSize = nameof(FeedSize);  // The number of items the feed should contain after sorting

        public const string FeedRss = nameof(FeedRss);

        public const string FeedAtom = nameof(FeedAtom);

        public const string FeedRdf = nameof(FeedRdf);

        public const string FeedId = nameof(FeedId);  // A Uri, links to the root of the site by default

        public const string FeedTitle = nameof(FeedTitle);  // Defaults to WebKeys.Title

        public const string FeedDescription = nameof(FeedDescription);  // Defaults to WebKeys.Description

        public const string FeedAuthor = nameof(FeedAuthor);  // Defaults to WebKeys.Author

        public const string FeedPublished = nameof(FeedPublished);

        public const string FeedUpdated = nameof(FeedUpdated);

        public const string FeedLink = nameof(FeedLink); // Uri

        public const string FeedImageLink = nameof(FeedImageLink); // Uri

        public const string FeedItemId = nameof(FeedItemId);

        public const string FeedItemTitle = nameof(FeedItemTitle);

        public const string FeedItemDescription = nameof(FeedItemDescription);

        public const string FeedItemAuthor = nameof(FeedItemAuthor);

        public const string FeedItemPublished = nameof(FeedItemPublished);

        public const string FeedItemUpdated = nameof(FeedItemUpdated);

        public const string FeedItemLink = nameof(FeedItemLink);

        public const string FeedItemImageLink = nameof(FeedItemImageLink);

        public const string FeedItemContent = nameof(FeedItemContent);

        public const string FeedItemThreadLink = nameof(FeedItemThreadLink);

        public const string FeedItemThreadCount = nameof(FeedItemThreadCount);

        public const string FeedItemThreadUpdated = nameof(FeedItemThreadUpdated);

        ////////// Deployment

        public const string GitHubOwner = nameof(GitHubOwner);

        public const string GitHubName = nameof(GitHubName);

        public const string GitHubUsername = nameof(GitHubUsername);

        public const string GitHubPassword = nameof(GitHubPassword);

        public const string GitHubToken = nameof(GitHubToken);

        public const string GitHubBranch = nameof(GitHubBranch);

        public const string NetlifySiteId = nameof(NetlifySiteId);

        public const string NetlifyAccessToken = nameof(NetlifyAccessToken);

        public const string AzureAppServiceSiteName = nameof(AzureAppServiceSiteName);

        public const string AzureAppServiceUsername = nameof(AzureAppServiceUsername);

        public const string AzureAppServicePassword = nameof(AzureAppServicePassword);
    }
}