# 0.13.0

* [Feature] Implemented general support for recipies (#1)
* [Feature] Implemented Blog recipe
* [Refactoring] Local file system now uses non-blocking read/write sharing
* [Feature] Adds a default "theme" folder to the set of default input folders
* [Refactoring] Better automatic metadata conversion of enumerable types to atomic values
* [Refactoring] Improvements to collection and document collection extensions (#195)
* [Feature] Added `Where()` fluent method to `GroupBy` and `Paginate` modules
* [Feature] Added new `GroupByMany` module
* [Feature] Implemented themes (#72)
* [Feature] Implemented CleanBlog theme for the Blog recipe
* [Fix] Fixed some bugs with globbing and assembly loading from the file system
* [Feature] Implemented default theme support for recipes (#310)
* [Fix] Fixed a bug when watching non-existing input paths
* [Feature] Added support for initial document metadata to the CLI and Cake addin (#11)
* **[Breaking Change]** [Refactoring] Renamed the CLI argument for global metadata to `--global` (#11)
* **[Breaking Change]** [Refactoring] All metadata collections are now case-insensitive (including document metadata) (#312)
* **[Breaking Change]** [Refactoring] `IMetadataValue` no longer passes the requested key (#312)
* [Refactoring] Pipeline names are now case-insensitive (#313)
* [Feature] `IExecutionContext` now implements `IMetadata` for the global metadata (#311)
* **[Breaking Change]** [Refactoring] Method `IMetadata.Documents()` is renamed to `IMetadata.DocumentList()` to avoid conflicts with `IExecutionContext.Documents()` (#311)
* [Refactoring] Added support for different commands to the CLI (#309)
* [Feature] Added a `new` command to the CLI for scaffolding initial files for a recipe (#309)
* [Feature] Added a `preview` command to the CLI for previewing a site without actually building it (#309)
* [Feature] Added a `OnlyMetadata()` method to the `WriteFiles` module that adds the same metadata the `WriteFiles` module usually does but does not actually write the files to disk (useful for staging files in one pipeline and then writing them in another)
* [Fix] Warns when NuGet communication fails (instead of terminating)
* [Feature] Adds new empty `IExecutionContext.GetLink()` method to get a bare site link
* **[Breaking Change]** [Feature] New `GenerateFeeds` module in `Wyam.Feeds` package to replace legacy `Rss` module (can generate RSS, Atom, RDF) (#322)
* [Fix] Fixes for nullables in metadata type conversion
* [Fix] Fix for stemming toggle in `SearchIndex` module (#315)
* [Refactoring] Assemblies are now loaded in parallel for performance
* [Refatoring] Removed beta pre-release version designation

# 0.12.4

* **[Breaking Change]** [Refactoring] Extension NuGet packages have been renamed from `Wyam.*` to `Wyam.*` to better represent other non-module extension points (#295)
* **[Breaking Change]** [Feature] The NuGet package version now takes a *version range*, so you must use `[x.y.z]` instead of `x.y.z` to specify a specific version
* [Feature] Added a `use-global-source` flag to trigger the use of globally configured NuGet package sources
* [Feature] WriteFiles now supports an `Append()` method to trigger appending to existing file instead of overwritting them (#304)
* [Refactoring] WriteFiles now has better handling of outputting multiple documents to the same file (#303)
* [Feature] New `Sidecar` module to pull metadata for a document from a sidecar file (#306, thanks @LokiMidgard)
* [Refactoring] Additional documentation for Cake alias (#302, thanks @gep13)
* [Refactoring] New assembly type scanning algorithm will make future extension points easier to support (#281)
* [Refactoring] File providers are now specified as a URI (#277)
* [Feature] New `Tree` module to construct a hierarchy from a set of documents (#292, thanks @LokiMidgard)
* [Fix] Output directory is now created on demand instead of automatically at execution (#293)
* [Fix] Check for null stream if null content on `Document.ToString()` (#294, thanks @LokiMidgard)
* [Refactoring] Renamed CLI flag `--pause` to `--attach` and changed behavior to wait for a debugger to attach instead of requiring key press
* [Fix] Fixed some bugs with the Cake.Wyam NuGet package dependencies (#291)
* [Fix] Fixed some problems with the handling of dotfiles (#289)
* [Fix] Documentation fixes for `IDocument` (#288, thanks @LokiMidgard)

# 0.12.3

* [Fix] Fixed a bug where execution could hang in some environments that open stdin and leave it open like Azure CI or VS Code task execution (#287)
* [Feature] Added a `--latest` flag to the `#nuget` preprocessor directive to indicate that the latest available package version should always be installed
* **Breaking changes**: Please see http://wyam.io/knowledgebase/migrating-to-0.12.x for more information

# 0.12.2

* [Fix] Emergency resolution for a bug with NuGet operations ordering an file locks
* **Breaking changes**: Please see the release notes for 0.12.1 for more information

# 0.12.1

* **[Breaking Change]** [Refactoring] Non-core module packages are no longer included with the default distribution and will need to be downloaded and installed by NuGet at runtime, see http://wyam.io/knowledgebase/migrating-to-0.12.x for more information (#275)
* [Feature] Preprocessor directives are now supported in your configuration files for NuGet package and assembly loading (#274)
* [Feature] The CLI also supports a similar syntax to the preprocessor directives for specifying NuGet packages and assemblies on the command line (#280)
* [Feature] We now have a Windows installer! (#127 and #283, thanks @FlorianRappl)
* [Feature] We also now have a Cake addin! (#129 and #276, thanks @gep13)
* [Refactoring] Migrated internal NuGet libraries to v3 (#190)
* [Refactoring] Moved configuration file and NuGet API logic out of Wyam.Core to new Wyam.Configuration library (#279)

# 0.12.0

* **MAJOR BREAKING CHANGES, BEWARE YE WHO ENTER HERE!** - The entire I/O stack has (finally) been rewritten and modules that were using `System.IO` classes have been ported to the new abstraction - read about how to migrate your pre 0.12.0 code at http://wyam.io/knowledgebase/migrating-to-0.12.x and keep on the lookout for bugs
* [Feature] New globbing engine with brace expansion, negation, and wildcard support (see http://wyam.io/getting-started/io for more details)
* **[Breaking Change]** [Refactoring] ReadFiles now uses the new I/O API and several configuration methods have been removed or changed
* **[Breaking Change]** [Refactoring] `IDocument.Source` is now a `FilePath`
* [Feature] You can now explicitly specify if a given `FilePath` or `DirectoryPath` is absolute or not
* **[Breaking Change]** [Refactoring] Moved `IMetadata.Link()` to `IExecutionContext.GetLink()`
* [Feature] Control global link settings (like hostname or whether to hide extensions) from `ISettings` (available from config file as `Settings`)
* [Feature] New `MinifyHtml` module that can minify HTML content (#260, thanks @leekelleher)
* [Feature] New `MinifyCss` module that can minify CSS content (#266, thanks @leekelleher)
* [Feature] New `MinifyJs` module that can minify JavaScript content (#266, thanks @leekelleher)
* [Feature] New `MinifyXhtml` module that can minify XHTML content (#266, thanks @leekelleher)
* [Feature] New `MinifyXml` module that can minify XML content (#266, thanks @leekelleher)
* [Feature] Added `IMetadata.FilePath()` and `IMetadata.DirectoryPath()` to make getting strongly-typed paths from metadata easier
* **[Breaking Change]** [Refactoring] Refactored several methods in `FilePath` and `DirectoryPath` into properties (`FilePath.GetFilename()` to `FilePath.Filename`, etc.)
* [Fix] Added support for root sequences to the Yaml module
* [Fix] Engine now retains global metadata between invocations in watch mode (#269, thanks @miere43)
* [Feature] New `Combine` module that can combine multiple documents into one
* [Refactoring] Content from NuGet packages is no longer copied to a staging folder and is instead retrieved directly

# 0.11.5

* [Fix] Well this is embarrassing - fix for the fix to properly handle undownloaded NuGet packages

# 0.11.4

* [Fix] Ongoing work with IO abstraction layer (#123, #214) to resolve some file system errors on build

# 0.11.3

* [Fix] Specifying input path(s) on the command line now correctly removes the default input path (#241 and #231)
* [Fix] Correctly handle paths that contain single . chars (#244)
* [Fix] Duplicate trace messages when config file is changed in watch mode (#243)
* [Feature] New support for specifying global metadata on the command line and accessing it from config files, the engine, and the execution context (#233 and #237, thanks @miere43)
* [Fix] Incorrect number of pipelines reported in output (#235 and #236, thanks @miere43)
* [Fix] Exceptions are now highlighted in the CLI (#230 and #232, thanks @miere43)

# 0.11.2

- A special thanks to @deanebarker who contributed a ton of new functionality as well as generated lots of great ideas, discussion, and bug reports - this release wouldn't be what it was without his help and support
- Note that the I/O abstraction support is still under development and may continue to change in the next version
* [Feature] Support for custom document types (#183)
* [Feature] Support for reading from stdin, including a new `ReadApplicationInput` module (#226, thanks @deanebarker)
* [Feature] New command-line switch `--verify-config` for validating the config file without actually running (#223, thanks @deanebarker)
* [Feature] New `ValidateMeta` module for validating metadata (#220, thanks @deanebarker)
* [Feature] New command-line switch `--preview-root` to set an alternate root path for the preview server (#213, thanks @deanebarker)
* [Feature] New `Merge` module that merges the content and metadata from documents output by a module sequence into the input documents 
* **[Breaking Change]** [Refactoring] ReadFiles now outputs new documents and only reads the files once if supplying a search pattern, to replicate the old behavior where ReadFiles read the matching files for each input document wrap it in the new Merge module
* [Feature] New `GenerateCloudSearchData` module to generate JSON data for AWS Cloud Search (#213, thanks @deanebarker)
* [Feature] New `Take` module that only outputs the first N input documents (#208 and #211, thanks @deanebarker)
* [Feature] New `CopyMeta` module to allow copying metadata from one key to another with optional formatting (#209 and #207, thanks @deanebarker)
* [Feature] Added `.WithGuidMetaKey()` to the Rss module to specify a metadata item that has the GUID (#206, thanks @deanebarker)
* [Feature] A tools NuGet package is now published that includes the CLI (#204)
* [Feature] Added void delegates to Execute module so you no longer have to return a value (#194)
* [Feature] Added `.IsRegex()` to Replace module (#201 and #203, thanks @deanebarker)
* [Feature] New `ModuleCollection` module to wrap a sequence of child modules for reuse (#197)
* [Feature] Added `IMetadata.Documents()` to return an `IReadOnlyList<IDocument>` from metadata (#200)
* [Feature] Added `IMetadata.Document()` to return an `IDocument` from metadata
* [Fix] Type conversions now properly take compatibility with enumerated item type into account (#198)
* [Fix] Fixed possible race condition when cloning documents with different sources (#196)
* **[Breaking Change]** [Feature] Implemented new IO framework and replaced all uses of strings for passing path information with new IO classes, `Engine.RootFolder`, `Engine.InputFolder`, and `Engine.OutputFolder` are now replaced by `IFileSystem.RootPath`, `IFileSystem.InputPaths`, `IFileSystem.OutputPath` (#123)
* **[Breaking Change]** [Refactoring] Changed `Trace` to a static class to better support forthcoming parallel pipeline processing
* **[Breaking Change]** [Refactoring] `Metadata` property in config file renamed to `InitialMetadata` to distinguish from run-time metadata
* [Refactoring] Removed the need to pass `Engine` to many core classes since it was just needed for the `Trace` instance (which is now static)
* [Refactoring] Split internal configuration classes for better separation of concerns and testability
* [Refactoring] Added Wyam.Testing library with common testing classes
* [Refactoring] Reorganized tests to better follow a specific convention
* [Refactoring] Changed color of critical errors in the console to white on a red background for better readability (#182)
* [Refactoring] Changed model type of Razor pages to `IDocument` instead of `IMetadata` (#188)
* [Refactoring] Uncaught exceptions now cancel the build (#187)

# 0.11.1

* **[Breaking Change]** [Refactoring] Changed the name of the release file from `wyam.zip` to `wyam-[verson].zip`
* [Fix] Namespaces are now added for `Engine.Configure()` calls without a script (#147 and #148, thanks @heavenwing)
* [Fix] CopyFiles was no returning a sequence for invalid folders (#166)
* [Fix] Better documentation and small bug fixes for CopyFiles (#158)
* [Fix] Excludes the output folder from watching (#156)
* [Fix] Fixes some odd behavior with FileName used with WriteFiles (#159)
* [Fix] Better failure handling to overcome locked file errors when watching (thanks @miere43)
* [Fix] Implemented file operation retry for several modules (#167, #169)
* [Feature] New `Rss` module (#174, thanks @miere43)
* [Feature] Added support for unknown file types to preview server (#175)
* [Feature] Added more specific error codes (#171)
* [Refactoring] Small improvements to Yaml module (better error messages, parallel processing) (#177)
* [Feature] New `GitHub` module (#145)
* [Feature] Added `Meta.IgnoreNull()` to control if null values should still be added to metadata
* [Feature] Added `IMetadata.Dynamic()` to return metadata values as dynamic objects
* [Refactoring] Adds an extra optional key to the cache for use by module authors
* [Feature] Added additional Markdown module constructor to render Markdown content in metadata
* [Feature] New `Json` and `GenerateJson` modules to read and write JSON data (#5)
* [Feature] Added support for setting input and output path from the configuration script
* [Fix] Git module now correctly ascends folder hierarchy
* [Refactoring] Migrated build system to Cake

# 0.11.0

* [Feature] New `Download` module for downloading content during generation (#68 and #75, thanks @dodyg)
* [Feature] New `AnalyzeCSharp` module that performs code analysis on C# source files, including XML documentation comments (#70)
* [Feature] New `ReadProject` module that loads all the source files referenced in an MSBuild project (#70)
* [Feature] New `ReadSolution` module that loads all the source files referenced in an MSBuild solution (#70)
* [Feature] Added `WithoutExtensions()` to CopyFiles to make it easier to specify which files to copy
* [Feature] Added additional `IExecutionContext.GetNewDocument()` overloads
* [Refactoring] Moved `Crc32` to `Wyam.Common` so other module libraries can also use it
* [Fix] NuGet support now considers .NET 4.6 packages when resolving libraries
* [Fix] Console application now exists with a non-zero error code so automated builds can be aborted if something goes wrong
* [Refactoring] Additional trace output for debugging, especially in NuGet resolve process
* [Refactoring] Better example test fixture (thanks @yetanotherchris)
* [Refactoring] Moved `Wyam.Common` types to appropriate nested namespaces
* [Feature] Added `WithMatchOnlyWholeWord()` to AutoLink module (#80, thanks @LokiMidgard)
* [Feature] New `HtmlEscape` module for escaping HTML characters (#81, thanks @LokiMidgard)
* [Feature] Additional `ToLookup()`, `ToDocumentDictionary()`, and `ToDictionary()` extension methods for `IDocumentCollection` (#82)
* [Feature] Added new `Execute()` constructor to Execute module that only gets evaluated once per pipeline (#83, thanks @LokiMidgard)
* [Feature] New `GitCommits` and `GitContributors` modules for getting information from Git repositories (#84, thanks @LokiMidgard)
* [Feature] New `ConcatBranch` module for branching and then concatenating the output (#87, thanks @LokiMidgard)
* [Feature] New `DirectoryMeta` module for applying metadata supplied for an entire directory to all documents sourced from that directory (#86, thanks @LokiMidgard)
* [Fix] Fixed several file locking issues (#92)
* [Feature] Performance boost by using parallel disposal at the end of pipelines
* [Feature] Added `ThenBy()` to OrderBy module (#101, thanks @LokiMidgard)
* [Feature] Can now output the config script files the way Roslyn sees them (after adding modules, etc.) for debugging purposes using the `--output-scripts` flag (#99)
* [Feature] New `FileName` module for generating SEO-friendly file names (#99, #104, and #107, thanks @yetanotherchris)
* [Feature] Added `UseWriteMetadata()` to WriteFiles module to control if metadata values are used for file destination (#106)
* [Feature] Default for FrontMatter module is now to ignore delimiter(s) if on the first line
* [Feature] All configured namespaces are added as `IExecutionContext.Namespaces` for other code generating modules to use
* [Feature] WriteFiles module now sets relative path information metadata (#111, thanks @reliak)
* [Feature] New `Sitemap` module to generate standards-compliant sitemap files (#113, thanks @reliak)
* [Feature] New `Xmp` module to import XMP encoded metadata (#114, thanks @LokiMidgard)
* [Feature] Updates to CopyFiles for better path handling (#116, thanks @reliak)
* [Feature] New `SearchIndex` module to generate Lunr.js compliant search indexes (#118, thanks @reliak)
* [Feature] New `Switch` module to provide `switch` statement like control for a pipeline (#120, thanks @reliak)
* [Feature] Added `IgnoreEmptyContent()` to WriteFiles module to ignore empty files (#117)
* [Feature] New `Xslt` module to apply XSLT transformations (#126, thanks @LokiMidgard)
* [Feature] New `CsvToHtml` and `ExcelToCsv` modules for working with tabular data (#132, thanks @LokiMidgard)
* [Refactoring] Removed static helper methods in `MetadataHelper` and replaced with `MetadataItem` and `MetadataItems` classes to make working with metadata easier
* [Refactoring] Renamed `Wyam.Core.MetadataKeys` to `Keys` and moved to `Wyam.Common` for common access (#131)

# 0.10.0

* [Feature] Added additional overloads to the `Documents` module that allow you to create new documents from scratch
* [Feature] New `IExecutionContext.GetNewDocument(...)` method that also allows you to create a new document from scratch from within your modules
* [Feature] New `GenerateMeta` module that procedurally generates metadata text using a text template (#55)
* [Feature] New `GenerateContent` module that procedurally generates document content using a text template (#55)
* **[Breaking Change]** [Refactoring] Renamed several fluent methods to better follow accepted fluent method naming conventions (I.e., `Xyz(...)` to `WithXyz(...)`)
* [Fix] Resolved memory consumption problems with using a large number of Razor templates by caching referenced assemblies (#69)
* [Feature] New `UnwrittenFiles` module that can prevent processing pipeline documents that have already been written to disk in a prior generation (#56)
* [Feature] Added `.WithExtensions(...)` fluent method to the `ReadFiles` module that makes it easier to restrict file reads to a set of file extensions
* [Feature] New `HtmlQuery` module that provides powerful querying of HTML content and sets result document content and/or metadata based on the results

# 0.9.0

* [Feature] New `Paginate` module for partitioning documents into pages (#60)
* [Feature] New `GroupBy` module for grouping documents
* [Feature] New `OrderBy` module for ordering documents (#61)
* [Feature] Several new constructor overloads to the `Documents` module that allow you get documents based on context.
* [Fix] Errors in the config file now report the correct line numbers (you will never know the pain I felt trying to get this to work correctly) (#67)
* [Refactoring] Changed `IDocument.Stream` to `IDocument.GetStream()` which now returns a wrapper stream that blocks on usage and must be disposed after use to prevent concurrent stream access for the same stream (#65)
* [Feature] Added support for automatic lambda expansion of `@doc` and `@ctx` variables in the configuration file
* [Feature] Added `ContextConfig` and `DocumentConfig` delegates to support the new automatic lambda expansion and encourage a standardized way of lazily configuring a module (#63)
* [Feature] Added `ConfigHelper` to support the common module configuration pattern where a module has either a `ContextConfig` or a `DocumentConfig`
* [Refactoring] Removed the `ConcatDocuments` module. The behavior can be easily represented by using other existing modules: `Concat(Documents(...))`
* [Fix] Fixed a bug in the Execute module when returning null
* [Fix] Fixed a bug in the ReadFiles module when no files match the specified pattern
* [Fix] Fixed some bugs in automatic module constructor factory method generation for configuration files for generic modules
* [Refactoring] now using more collections from System.Collections.Immutable for improved performance and safety

# 0.8.0

* [Feature] Improved caching support in multiple modules including Razor and Markdown
* [Feature] New `IDocument.Source` property to uniquely identify the source of each document (#38)
* [Feature] New `AutoLink` module for automatically generating links within HTML content (#36)
* [Refactoring] Major changes to how Wyam handles the storage of content inside a document to include operating on streams and/or string buffers for increased performance (#42)
* [Refactoring] Improvements to document and engine life cycle for proper disposal of resources
* [Feature] New `Images` module for manipulation of images within a pipeline (#25 - thanks @dodyg)
* [Refactoring] Renamed `Wyam.Abstractions` to `Wyam.Common` to reflect that it also contains non-abstract common code (#45)
* [Feature] New `ForEach` module that allows processing child documents one at a time (#47)
* [Fix] Fixes to how the Less module handles relative URLs (#49 - thanks @rohansi)
* [Refactoring] Made the cache thread-safe for parallel processing within a module (#50)
* [Refactoring] Changed behavior of several modules including Razor, Less, Markdown, ReadFiles, and WriteFiles, to process documents in parallel resulting in significant performance improvements
* [Feature] ASCII art FTW
* [Refactoring] Moved `PathHelper` to `Wyam.Common`
* [Feature] New `Excerpt` module that can excerpt the first specified element from HTML content and add it to metadata (#26)
* [Feature] New `processDocumentsOnce` flag that can skip any documents previously processed by the pipeline (#56)
* [Fix] Fixed a bug with Razor sections and `IRazorPage` instance reuse on subsequent pipeline executions (#58)

# 0.7.0

- Moved all the libraries from alpha to beta to reflect improving stability
* [Refactoring] Updated all projects and solution to Visual Studio 2015 and .NET 4.6
* [Feature] Added ability to specify alternate ViewStart paths for the Razor module (#37 - thanks @mschumaker)
* [Feature] Added ability to specify file ignore prefixes for the Razor module, files prefixed with `_` are now ignored by default (#37 - thanks @mschumaker)
* [Feature] Added additional metadata to `ReadFiles`: `SourceFilePathBase`, `RelativeFilePathBase`, and `RelativeFileDir`
* [Feature] Added additional metadata to `WriteFiles`: `DestinationFileBase`, `DestinationFileExt`, `DestinationFileName`, `DestinationFileDir`, and `DestinationFilePathBase`
* [Fix] Better normalization of file paths
* [Feature] Adds a new `string IMetadata.Link(string key, string defaultValue = null)` method for automatically formatting links for HTML use (I.e., replacing slashes)
* [Refactoring] Updated to Roslyn 1.0
* [Feature] Adds a new universal `IExecutionCache` for modules to use for caching data

# 0.6.0

* [Feature] Yaml module now maps simple scalar sequences to metadata when flattening
* [Refactoring] Switched from Roslyn scripting engine to direct compilation for configuration files, improving capabilities of configuration file declarations (extension classes, etc.)
* [Fix] Lots of fixes and updates for assembly locating and loading
* [Feature] Implemented support for custom Razor base page types
* [Feature] Added new `Documents` module that replaces the documents in the pipeline with alternate ones
* [Feature] Added overload to the `Execute` module that allows use of the execution context
* [Refactoring] Moved `MetadataKeys` class out of `Wyam.Abstractions` to `Wyam.Core` so that `Wyam.Abstractions` would be more stable, though this means user code will have to go back to strings for metadata keys
* [Feature] Added ability to override write location in the `WriteFiles` module on a per-document basis by setting specific metadata
* [Feature] `IDocument` now implements `IMetadata` so metadata can be retrieved directly from the document object
* [Feature] Added a `.String(...)` method to `IMetadata` to make getting string values easier
* [Refactoring] Created a new `IDocumentCollection` interface and implementations to provide a better API for getting documents during execution
* [Feature] Implemented support for converting metadata between array types and between scalars and array types
* [Feature] Implemented a `.ToLookup<T>(...)` extension method to generate a lookup table of metadata values to documents (for example, get all tags and the documents that define those tags)

# 0.5.0

* [Feature] Implemented a **Less CSS** module
* [Fix] Fixes for NuGet package dependencies
* [Feature] Can now declare prerelease NuGet packages in configuration file without specifying version specification
* [Feature] Calls to `System.Diagnostics.Trace` from third-party libraries are now picked up by a custom `TraceListener` and output to the Wyam `TraceSource`
* [Refactoring] Renamed several metadata keys for consistency
* [Feature] `Engine` now implements `IDisposable` for better lifecycle management

# 0.4.0

* [Feature] Added a `Concat` module to concatenate documents from child modules with the current documents
* [Feature] Added a `ConcatDocuments` module to concatenate documents from previous pipelines with the current documents
* [Feature] Added a `Where` module to filter the current documents with a predicate
* [Refactoring] Moved the predicate in the `Branch` module to a fluent `.Where(...)` method to follow established conventions
* [Feature] `IPipelineCollection` now implements `IReadOnlyDictionary<string, IPipeline>`
* [Feature] `IPipeline` now implements `IList<IModule>`
* [Feature] A new `IReadOnlyPipeline` interface that implements `IReadOnlyList<IModule>` has been introduced for use during execution (since pipelines can't be changed once execution starts)
* [Feature] Added a test fixture to run all examples during testing
* [Feature] Added `RootFolder`, `InputFolder`, and `OutputFolder` as available properties in the configuration script
* [Refactoring] Renamed the `Metadata` module to `Meta` so it wouldn't conflict with the `Metadata` property in configuration scripts
* [Feature] Specifying `--watch` now also watches the configuration file and initializes a new engine if it changes
* [Refactoring] Updated the default configuration (for when no configuration file is specified or found) to use a single pipeline for Markdown and Razor documents

# 0.3.0

* [Refactoring] Added the Roslyn nightly packages to source control for easier builds (since they don't stay on the feed for long)
* [Feature] Added sequence of configured assemblies to the `IExecutionContext` (so modules like Razor can pick them up)
* [Feature] Added a new `MetadataKeys` static class to `Wyam.Abstractions` to help eliminate magic strings

# 0.2.0

* [Feature] Implements default configuration
* [Feature] CopyFiles now sets metadata values for SourcePath and DestinationPath for each document
* [Fix] Crashes when certain directories didn't exist
* [Refactoring] Trace module now follows fluent conventions

# 0.1.1

* [Fix] Fixes a bug with parsing some of the command line argument options
* [Feature] Adds support for specifying input and output folder on the command line

# 0.1.0

* Initial release!