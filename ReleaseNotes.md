# 1.4.1

- [Fix] Fixed missing known extensions which should help make future version updates smoother (#652)
- [Feature] Added ability to turn on binary logging from MSBuild for `CodeAnalysis` modules
- [Feature] Updated Buildalyzer/MSBuild for `CodeAnalysis` modules

# 1.4.0

- [Refactoring] Totally rewrote assembly loading
- [Refactoring] Converted a bunch of support and extension libraries to .NET Standard (#646, thanks @pauldotknopf)

# 1.3.0

- [Refactoring] Configuration caches will always append to original configuration file ("config.wyam.dll", "config.wyam.hash", "config.wyam.packages.xml", etc.) (#619) - **Note** while this isn't a breaking change, it may require you to change your `.gitignore` files to handle the revised cache file names
- [Fix] Graceful handling of missing cached config dll file (#617)
- [Refactoring] Trace settings at startup as verbose (#618)
- [Feature] Added an overload to `Replace` module that allows using the document in the content finder delegate (#625)
- [Fix] Switches to `Regex.Replace()` when using a replacement delegate in the `Replace` module to avoid an infinite recursion edge case (#624)
- [Fix] Better handling of . (dot) character in `RedirectFrom` module (#620, #621, thanks @wozzo)
- [Feature] Added a new default behavior for `Sitemap` to generate a document link if no `SitemapItem` key is provided or usable (#622)
- [Feature] Adds support for defining custom content types in the preview server
- [Feature] Added `IMetadata.GetMetadata()` to get a new metadata object with only the specified keys
- [Feature] Adds an additional constructor to the `Execute` module to process the entire input set at once
- [Feature] Adds ability to serialize selected metadata keys to a JSON object in `GenerateJson` module
- [Feature] Adds ability to specify additional serializer settings for `GenerateJson` module
- [Fix] Fixes bug with `GenerateJson` module not setting metadata key
- [Refactoring] Removed the Wyam.Windows installer, it's been replaced by Chocolatey and eventually a .NET CLI global tool

# 1.2.0

- [Feature] Locks in matching recipe and theme package versions (#587)
- [Refactoring] Some docs updates (#604, thanks @olevett)
- [Feature] Can add Markdown extension instances directly (#602, thanks @Silvenga)
- [Fix] Fixes adding multiple custom Markdown extensions (#601, thanks @Silvenga)
- [Fix] Fixes output corruption when using `--noclean` (#593, thanks @Silvenga)
- [Feature] Added support for generating a Chocolatey package (#598, #95, thanks @phillipsj)
- [Fix] CSS cleanup for arrows in the docs theme
- [Feature] Docs recipe now ignores Razor pages from the theme if an equivalent Markdown page exists (#595)
- [Feature] Added a `Where()` method to the `Concat` module 
- [Feature] Added `DocsKeys.ShowInSidebar` to hide pages from the sidebar in the docs recipe
- [Fix] Ignore case when locating index files for the `Tree` module
- [Feature] Added support for JSON content types to the embedded web server
- [Feature] Added sidebar header and footer overrides to docs theme
- [Refactoring] Updated MD5 hashes to SHA512 for FIPS compliance (#597, thanks @kakrofoon)
- [Feature] Enabled code signing for wyam.exe
- [Feature] Adds `DocsKeys.ApiPath` to allow changing the path where API docs are placed in the docs recipe

# 1.1.0

- **[Breaking Change]**[Refactoring] Removed the `Git` module since it's not fully SDK compatible
- **[Breaking Change]**[Refactoring] Removed the `Xslt2` module since it uses packages that are not cross platform or SDK compatible
- [Fix] Referenced assemblies are not loaded if a version was already directly loaded
- [Fix] Fix for excerpts in the BlogTemplate blog theme (#580, thanks @AWulkan)
- [Fix] Fix for "Back to Posts" link in the BlogTemplate blog theme (#581, #583, thanks @jbrinkman)
- [Fix] `CodeAnalysis` and the docs recipe can now read SDK and .NET Core projects via [Buildalyzer](https://github.com/daveaglick/Buildalyzer) (#575)
- [Refactoring] Updated Cake.Wyam to Cake 0.22.2 and targeting .NET Framework 4.6 and .NET Standard 1.6 (#579, #573, thanks @ProgrammerAl and @RLittlesII)
- [Fix] Fix for generated syntax from `CodeAnalysis` for more than one attribute within a bracket (#577, thanks @jonasdoerr)
- [Refactoring] Converted most of the projects to the SDK (no impact on release artifacts, but important for contributors)

# 1.0.0

- **[Breaking Change]**[Refactoring] Removed all deprecated code including string-based document creation methods
- **[Breaking Change]**[Refactoring] Renamed `Download.CacheResponse()` to `Download.CacheResponses()`
- **[Breaking Change]**[Refactoring] Renamed `RequestHeader` class for use with the `Download` module to `RequestHeaders`
- [Feature] Added support for custom headers to `RequestHeaders` for use with `Download` module
- [Feature] New `YouTube` module for interacting with the YouTube API (#547, thanks @ghuntley)
- [Feature] New Trophy theme for Blog recipe (#522, thanks @adamclifford)
- [Fix] Fixed a bug in Docs recipe when generating lowercase links
- [Fix] `NormalizedPath` no longer trims spaces (#566) 
- [Feature] New Stellar theme for Blog recipe (#563, thanks @arebee)
- [Feature] The compiled config script is now cached to disk as `config.wyam.dll` and `config.wyam.hash` (prefixed with whatever the config file name is) (#557, thanks @Silvenga)
- [Feature] New `Objects` module for populating metadata by reflecting on an arbitrary object (#539, thanks @deanebarker)
- [Feature] New `Xml` module for populating metadata by reading elements from XML content (#539, thanks @deanebarker)
- [Feature] New `Sql` module for populating metadata by reading data from a SQL server (#539, thanks @deanebarker)
- [Feature] New `ReadDataModule` abstract base module for populating metadata from arbitrary data (#539, thanks @deanebarker)
- [Fix] Several fixes related to generic types in `AnalyzeCSharp` module and Docs recipe (#494, #564)
- [Fix] Excludes empty namespaces from `AnalyzeCSharp` module by default (#526)
- [Feature] Updated globbing pattern in Blog and Docs recipes to include blog posts in subfolders (#560, thanks @archnaut)
- [Feature] If a module implements `IDisposable` it will now be disposed when the engine is
- [Refactoring] Disabled data protection services in `Razor` module (#559, thanks @Silvenga)
- [Fix] Additional inherit doc support for properties and events in `AnalyzeCSharp` module (#553)
- [Fix] Fix for search index items when virtual directory is used (#554)
- [Fix] Normalizes culture in all the examples (#544)
- [Fix] Fixes HTML escaping in API search results in Docs theme (#552)

# 0.18.6

- [Feature] Added flag `DocsKeys.ImplicitInheritDoc` to docs recipe to globally assume `inheritdoc` (#551)
- [Feature] Added `AnalyzeCSharp.WithImplicitInheritDoc()` to assume `inheritdoc` for symbols without any XML comments (#551) 
- [Fix] Fixed a bug in the `AutoLink` module when containing node also has escaped HTML content (#550)
- [Fix] Fixed a bug with `If` module when using a `ContextConfig` delegate, unmatched documents were not falling through. You may need to add `.WithUnmatchedDocuments()` if you are using an `If` module with the incorrect behavior.
- [Feature] Added `Keys.LinkLowercase` to always generate lowercase links (default is false) (#546)
- [Fix] Fixed a bug when using `NoSidebar` in docs recipe (#549)

# 0.18.5

- [Refactoring] Moved the blog theme index template file from `index.cshtml` to `_Index.cshtml` to match other template conventions (#520)
- [Feature] Added settings to the blog recipe to configure index template and output path (#541)
- [Feature] Added note to readme file about platform support (#540, thanks @perlun)
- [Fix] CLI commands are now case-insensitive, though options and flags are still case-sensitive for now (#539)
- [Refactoring] Preview server now logs a full localhost URL so terminals can link it (#533)
- [Refactoring] Switching exit key combination to Ctrl-C instead of any key (#534)
- [Feature] New BlogTemplate theme for the blog recipe to use as basis for new themes (#518, thanks @adamclifford)
- [Fix] Docs fix for Cake addin (#535, thanks @ghuntley)
- [Refactoring] Suppress CS1701 in config file compilation instead of filtering (#529, thanks @filipw)
- [Refactoring] Performance boost for Razor rending (#509, thanks @jontdelorme)
- [Feature] New SolidState theme for the blog recipe (#514, thanks @RLittlesII)

# 0.18.4

- [Fix] Fixes feed content and description for common web pipelines (#528)

# 0.18.3

- **[Breaking Change]**[Refactoring] Moved Blog recipe theme file `/_PostIndex.cshtml` to `/_Archive.cshtml`, no other changes should be needed to this file in themes other than to move it - sorry for the rename (again), the first name was kind of dumb, this one is better
- **[Breaking Change]**[Refactoring] Moved Blog recipe theme file `/tags/index.cshtml` to `/_Tags.cshtml`, no other changes should be needed to this file in themes other than to move it
- [Feature] Suppressed tag page generation if no tags are present in Blog recipe (#456)
- [Refactoring] Refactored `Wyam.Web` pipelines to encapsulate settings in classes
- [Feature] Added a ton of flexibility and new settings to index and archive pages for the Blog recipe (#516)
- [Feature] Adds the background check JS to BookSite Velocity theme
- [Feature] Excludes `.git` from all recipes in case an input subfolder was closed from a repository
- [Fix] Resolved some edge-case bugs with the globbing engine

# 0.18.2

- [Fix] Fix for invalid metadata key in Docs theme (#515)

# 0.18.1

- **[Breaking Change]**[Refactoring] Moved a couple theme template files in Blog themes: `posts/index.cshtml` -> `_PostIndex.cshtml`, `tags/tag.cshtml` -> `_Tag.cshtml` - if you were overriding these theme files, you'll need to move and rename your override file to match
- [Refactoring] Deprecated `BlogKeys.HeaderTextColor` and the CleanBlog theme now automatically calculates header text color from background image using JS
- [Feature] Added default settings to recipe scaffolding (#503)
- [Fix] Manually copies the native git libraries for the `Git` module (#506)
- [Feature] Added support for Less and Sass files to the Blog recipe
- [Feature] Added `WithPageMetadata()` method to `Paginate` module to add metadata to generated page documents
- [Feature] Added `NextPage` and `PreviousPage` output metadata to `Paginate` module
- [Refactoring] Prefixed all Less include files in the Docs theme with an underscore and excluded underscore files from being processed directly by recipe (#507)
- [Feature] `Less` module now sets `Keys.WritePath` with a `.css` extension - this may result in warnings if you follow the `Less` module with the `WriteFiles` module with an extension specified, remove the extension from the `WriteFiles` module and the warnings should go away
- [Feature] `Less` module will now automatically check for underscore prefixes and add `.less` extension when looking for includes (#507)
- [Feature] New `Sass` module (#7, thanks @enkafan)
- [Feature] New `Sort` module for ordering documents using a comparison delegate
- [Feature] `If` module can now ignore unmatched input documents
- [Refactoring] Made `IModuleList` extensions fluent
- [Feature] `Documents` module can now get documents from more than one pipeline at a time
- [Refactoring] Moved source code for recipes to a dedicated folder
- [Refactoring] All current recipes now share a common set of reusable pipelines from the new `Wyam.Web` library (#501)
- [Feature] New BookSite recipe for book and ebook marketing sites (#488)

# 0.17.7

- [Fix] Fixes LiveReload support when under a virtual directory (#496)
- [Fix] Fixed some asset links in docs theme when under a virtual directory (#497)

# 0.17.6

- [Fix] Fixes some dependency issues with the MEF 2 libraries and Roslyn packages

# 0.17.5

- [Feature] `CodeAnalysis` module (and docs recipe) now supports loading source code directly from projects and solutions (#493)
- [Fix] Fixes regression of missing `BlogKeys.Posts` for tag documents (it broke when pagination was added)

# 0.17.4

- [Feature] Adds a warning when output path is also one of the input paths (#490)
- [Refactoring] Refactored `IMetadata` type conversion convenience methods into extension methods
- [Refactoring] Refactored `IExecutionContext.GetLink(...)` methods into extension methods
- [Feature] Adds settings and extensions for dealing with input and output date cultures (#475)
- [Refactoring] Tons of documentation comment updates
- [Refactoring] Added server tests back in (#484)

# 0.17.3

- [Fix] Fixed occasional missing XML doc comments from assembly XML doc files (#483)
- [Fix] Fixed inheritdoc behavior when interface is inherited from base class (#481)
- [Refactoring] Turned off x86 runtime preference and switched to x64 libuv - this means the Wyam preview server will no longer work on x86 platforms without manually swapping the libuv library (#484)
- [Refactoring] Continuing to tweak memory pool settings
- [Fix] Excluding partials from the blog pages pipeline

# 0.17.2

- [Feature] Added `IExecutionContext.GetContentStream()` utility method to get streams for document content
- [Refactoring] Removed string content support from documents and deprecated string-based document factories (#477)
- [Feature] Added support for alternate content storage strategies (#476)
- [Feature] Special handling for CLI directory path inputs ending in an escape (#472)
- [Feature] Updated to latest Roslyn libraries supporting C# 7 (#473)
- [Feature] Added support for operators to the docs recipe (#468)
- [Feature] Added `Keys.TotalItems` for use with the `Paginate` module
- [Feature] Added optional pagination to tag archives for blog themes using `BlogKeys.TagPageSize` (#469, thanks @alfadormx)

# 0.17.1

- [Fix] Fixed regression in `Execute` module that caused tags not to be output in Blog recipe
- [Feature] Added a new `Pipeline` class in `Wyam.Common` to use as base for predefined pipelines (I.e., in recipes)
- [Refactoring] Moved `ModuleCollection` module to `Extensibility` namespace
- [Refactoring] Code quality ruleset updates
- [Fix] Resolves server errors with LiveReload (#465, thanks @Silvenga)

# 0.17.0

- [Fix] `CodeAnalysis` module only displays duplicate comments once for partial classes (#460, #463, thanks @M-Zuber)
- [Feature] New `Join` module for joining documents together (#461, #23, thanks @JamesaFlanagan)
- [Feature] Implemented a NuGet dependency cache making subsiquent generations *much* faster (#317)
- [Refactoring] Updated Cake to 0.17.0 (#457, thanks @pascalberger)
- [Refactoring] Ported hosting code to new `Wyam.Hosting` library that can be used outside Wyam (#385)
- [Fix] Ensures result documents get set, even for empty pipelines (#455)
- [Feature] New `Highlight` module for generation-time highlight.js highlighting (#452, thanks @enkafan)
- [Feature] Implements a new JavaScript engine framework including runtime engine switching for modules to use JavaScript libraries! (#452, thanks @enkafan)
- [Refactoring] Filters JetBrains assembly from loaded namespace list (#453, thanks @enkafan)
- [Feature] Adds LiveReload support for the preview server! (#420, thanks @Silvenga)
- **[Breaking Change]** [Refactoring] Removed `IPipelineCollection` overloads for specifying documents should be processed once to use a fluent method instead
- [Refactoring] Refactoring of `IPipelineCollection` to move a bunch of implementation logic into extension methods
- [Refactoring] Changed preview web server from Katana to Kestrel (#449, thanks @Silvenga)
- [Refactoring] Updated LibGit2Sharp to 0.23.1 (#450, thanks @pauldotknopf)
- [Feature] Adds extension methods to `IModuleList` to help tweak recipes (#445, thanks @enkafan)
- [Refactoring] Implements a new `IModuleList` interface and implementations that support named module collections and applies the concept to control modules as appropriate (#447, #448)
- [Refactoring] Moved `TraceExceptionsExtensions` to `Wyam.Common.Execution`
- [Feature] Added several code quality checkers and rulesets (#443, #444, thanks @Silvenga)
- [Fix] Relative URLs are now unescaped in `ValidateLinks` module

# 0.16.3

- [Feature] Docs recipe now checks for an "src" folder both inside and alongside the "input" folder (#436)
- [Feature] Outputs current settings on every build
- [Refactoring] Switched blog theme string constants to use the correct `BlogPipelines` keys (#435, thanks @enkafan)
- [Fix] Fixed null check in Phantom blog theme footer (#433, thanks @enkafan)
- [Feature] Added helpers for module developers to trace per-document exception messages (#320)
- [Feature] Greatly improved per-document error messages (#320)
- [Fix] jQuery CSS reference casing was wrong in docs theme

# 0.16.2

- [Fix] Fixes a bug when multiple input paths have the same starting characters (#414)
- [Fix] Adds support for duplicate single-value directives when values are equivalent (#430)
- [Feature] Adds ability to specify Markdig extension classes within the Blog and Docs recipes (#429, thanks @enkafan)
- [Refactoring] Changed MSBuild to use maximum number of CPUs, speeding up compile times (#428, thanks @enkafan)
- [Fix] Fixed guard clauses in `Image` module (#427, thanks @n-develop)
- [Feature] Exceptions and other execution errors no longer kill the watch loop (#422, #421, thanks @Silvenga)
- [Feature] Improved error messages when delegate invocation return types don't match
- [Feature] Adds support for complex nested object sequences to the `Yaml` module
- [Feature] Adds `IMetadata.DateTime()` shorthand method for getting `DateTime` values from metadata
- [Fix] Adds the missing `CsvToMarkdown` file to the project (#418, thanks @LokiMidgard)
- [Feature] Adds support for the `--attach` flag to the Cake addin

# 0.16.1

- [Refactoring] Docs recipe now allows you to specify a logo file using the `DocsKeys.Logo` setting
- [Fix] Fixes recipes and `Title` module so that page titles wouldn't use the global title settings if no explicit title was set
- [Feature] Added `IDocument.WithoutSettings` for getting metadata for a document without the global settings
- [Feature] Added `IMetadata.Bool(...)` for getting Boolean values from metadata

# 0.16.0

- [Refactoring] Out of beta!
- [Refactoring] All libraries now target .NET Framework 4.6.2
- [Refactoring] `GlobalMetadata`, `InitialMetadata`, and `Settings` have all been moved to a consolidated `Settings` metadata collection (#379)
- [Feature] An error is now displayed when running under Mono (#375)
- [Feature] Recipes can now output a config file (#388)
- [Fix] Preview server can now handle escaped URL characters when supporting extensionless files (#413)
- [Fix] Fixes CSS for block quote margins in CleanBlog theme (#412, thanks @n-develop)
- [Fix] Fixes CSS for dropdown menus in CleanBlog theme (#409, thanks @n-develop)
- [Fix] Fixes use of `PostsPath` setting for archive links in themes (#400, thanks @kamranayub)
- [Fix] Fixes `params` parameter types in `CodeAnalysis` module (#407)
- [Feature] Adds support for escaping CDATA XML doc comments to `CodeAnalysis` module (#411)
- [Feature] Adds a filter function for `CopyFiles` module to fine-tune destination path (#398, thanks @deanebarker)
- [Fix] Fixes support for `RssPath` and `AtomPath` settings in themes (#396, #397, thanks @kamranayub)
- [Feature] Adds option to replace RegEx matching groups in `Replace` module (#386, thanks @LokiMidgard)
- [Feature] New `CsvToMarkdown` module (#384, thanks @LokiMidgard)

# 0.15.8

- [Refactoring] Removed Turbolinks from the docs theme due to some oddness with JavaScript loading
- [Fix] Removes overridden members from the members collection in `CodeAnalysis` module
- [Refactoring] Moved remarks section up in docs theme
- [Feature] Added `_Head.cshtml` override file to the blog themes
- [Feature] Support for `<inheritdoc/>` in the `CodeAnalysis` module and docs recipe (#364)
- [Refactoring] Updated Markdig version

# 0.15.7

- [Fix] Fixed a bug with new docs recipe package downloads
- [Fix] Fix for wrong System.Console binding in Wyam.Windows (#372, thanks @adamclifford)
- [Fix] Fix for logo path in docs recipe (#369, thanks @meil)

# 0.15.6

- [Fix] Post date comparison now performed with current time instead of midnight (#365)
- [Refactoring] Better warnings when posts are skipped due to future or missing date in docs and blog recipes (#365)
- [Feature] Adds support for serving from a virtual directory to the preview server using the `--virtual-dir` option
- [Feature] New `ValidateLinks` module with opt-in use for blog and docs recipes (#15)
- [Fix] Modifying all assets and links in themes to use link generation for virtual directory support
- [Refactoring] Improvements to `LinkGenerator` to make it more flexible (this class isn't typically used by user code)

# 0.15.5

- [Fix] Fixed missing default paths for blog feeds
- [Refactoring] Made all theme resources except Google Fonts local for better HTTPS support

# 0.15.4

- [Fix] Fixing some deployment issues with last release and package dependencies

# 0.15.3

- [Refactoring] Updated Roslyn and Razor NuGet packages
- [Refactoring] Additional caching for Razor page compilation under certain conditions (especially for docs recipe)

# 0.15.2

- [Fix] Adds a warning for unparsed publish dates in docs and blog recipes (#361)
- [Fix] Added support for fields and constant values to `CodeAnalysis` module and docs recipe (#363)

# 0.15.1

- [Fix] Added the Docs recipe and Samson theme to the set of known recipes and themes

# 0.15.0

- **[Breaking Change]** [Refactoring] Moved several of the blog recipe files to the root to accomodate different post paths. If you have a file override for `/posts/_PostFooter.cshtml` it'll need to be moved to `/_PostFooter.cshtml` (likewise with other theme files in `/posts` except for `/posts/index.cshtml` which is still in `/posts`).
- [Fix] Suppresses CS1701 (binding redirect) warnings in config script host
- [Refactoring] Switched the blog themes to highlight.js (#345)
- [Feature] Added ability to set a comparer for `GroupBy`, `GroupByMany`, and `OrderBy` modules
- [Fix] Several bug fixes for `AutoLink` module
- [Refactoring] Moved `LinkGenerator` to `Wyam.Common`
- [Feature] New `Redirect` module for generating redirect pages (#44)
- [Feature] `IMetadataValues` are now searched recursivly (so an `IMetadataValue` and return another `IMetadaValue`)
- [Feature] New customization options in `SearchIndex` module
- [Feature] Added support for `NamespaceDoc` classes to `AnalyzeCSharp` module
- [Feature] Added support for namespace XML doc comments to `AnalyzeCSharp` module
- [Feature] Added support for inherited members to `AnalyzeCSharp` module
- [Feature] Ability to insert new pipelines before and after other named pipelines (useful for customizing recipes)
- [Feature] Added support for analyzing attributes to `AnalyzeCSharp` module
- [Fix] Fixed a bug in `Xslt2` regarding base URIs (#355, thanks @LokiMidgard)
- [Feature] Support for array values when specifying global/initial metadata via the CLI and Cake
- [Feature] New `AnalyzeCSharp.WithAssemblies()` method for assembly-based code analysis
- [Feature] New `TestExecutionContext` class in `Wyam.Testing` for easier test mocking
- [Feature] New `Headings` module for adding HTML headings in content to metadata
- [Feature] Added `.WithNesting()` to `Tree` module to nest documents inside metadata as a hierarchy
- [Fix] Fixed a bug with content folders in NuGet packages
- [Refactoring] Updated NuGet libraries to 3.5
- [Fix] Added `UseGlobalSources` flag to Cake addin
- [Fix] Fixed some bugs with Cake addin when specifying NuGet packages
- [Feature] `If` module now supports context-only predicates that only evaluate once for all input documents
- [Feature] Added `Meta.OnlyIfNonExisting()` to prevent setting metadata if value already exists
- [Feature] Support for Jekyll-style excerpt comment separators in the `Excerpt` module 
- [Feature] New `Include` module for including content from a file into an existing document
- [Feature] New `Title` module for setting title metadata
- [Feature] New `Flatten` module for flattening hierarchical document trees
- [Feature] Lots of improvements and fixes to `Tree` module
- [Feature] Adds new `Docs` recipe (#342)
- [Feature] Adds the `AdventureTime` sample from [Static-Site-Samples](https://github.com/remotesynth/Static-Site-Samples)
- [Fix] Fixes bug with link generation in `CodeAnalysis` module for nested types
- [Feature] Adds `Razor.WithModel()` for specifying a view model in config script
- [Feature] Support for alternate model types in Razor views (including partials)
- [Fix] Fixes some bugs with complex recursive globbing patterns
- [Feature] Adds `IDirectory.Parent` for finding parent directory
- [Refactoring] Big refactoring of source code organization
- [Fix] Add NuSpec for Xslt2 module, resolves #341

# 0.14.1

- [Feature] `Execute` module now processes documents in parallel with a new option to fall back to sequential processing
- [Feature] Adds support for executing modules and replacing content to the `Execute` module (this makes it much more powerful)
- [Feature] Adds `IsRegex()` to `ReplaceIn` module
- [Feature] New `Xslt2` module that uses Saxon to apply XSLT 2 transforms (#340, thanks @LokiMidgard)
- [Fix] Add themes to the set of known extensions, ensuring they stay up to date

# 0.14.0

- Note: this release introduces a new Razor engine and host based on ASP.NET Core MVC. It should be similar to the old one, but there might be some breaks, especially if using layouts and partial pages. Most of the differences have to do with stricter path handling by ASP.NET Core MVC than the previous implementation. If you find Razor pages aren't building correctly, try tweaking your layout and partial paths. 
- [Refactoring] When specifying official Wyam extension packages the current Wyam version will be used if no version is specified, forcing package download if the matching version isn't already available (#338)
- [Refactoring] Tweaked the naming scheme of test classes and methods (#329)
- [Feature] Adds `.WithLayout()` fluent methods to the `Razor` module
- **[Breaking Change]** [Refactoring] Totally rewrote the `Razor` module on top of ASP.NET Core MVC (#141)
- [Fix] Fixed some spelling mistakes (#337, thanks @hyrmn)
- [Feature] Added an example showing integration in ASP.NET web applications (with a fix in #335, thanks @jamiepollock)
- [Refactoring] Moved to a higher-performance `ConcurrentHashSet` implementation (#325)
- [Fix] Fixed excerpt rendering for posts in blog themes
- [Fix] Fixes related to using and generating links for HTTPS (#332, #333, thanks @leekelleher)
- [Fix] Inverted the order of tags in blog themes (#331, thanks @ibebbs)

# 0.13.5

- [Fix] Fixed lifting of using directives in the configuration file

# 0.13.4

- [Fix] Fixed duplicate posts in archive for CleanBlog and Phantom Blog themes
- [Feature] Added support for post footers in Blog themes
- [Refactoring] Changed Markdown processor to Markdig (#328, #165, thanks @FlorianRappl)
- **[Breaking Change]** [Refactoring] Removed declarations section of config file, any global classes are now automatically "lifted" out of the config method (#316)
- [Refactoring] Switched to a CDN for Bootstrap and jQuery resources in CleanBlog theme
- [Feature] Pretty print CSS class now automatically added to Blog recipe posts 

# 0.13.3

- [Refactoring] Removed cache hit/miss messages from the verbose log due to excessive noise
- [Fix] NuGet no longer crashes for lack of network access (#326)
- **[Breaking Change]** [Refactoring] Removed the `an` directive and command line argument, all assemblies should now be loaded with the `a` directive/argument regardless of if a full name, simple name, globbing pattern, or file path is specified
- [Feature] Will now attempt to load assemblies from a simple name like `System.Xml` with the same version as the current framework
- [Refactoring] Lots of refactoring related to assembly loading (#324)
- [Feature] Added new Phantom theme for the Blog recipe
- [Feature] Added support for hero images to the Blog recipe
- [Feature] Added an argument to recipe and theme directives to indicate any known recipe/theme packages should be ignored (mainly for debugging new recipe/themes)
- [Feature] Added an "about" page to the Blog recipe scaffolding

# 0.13.2

- [Fix] Added a NuGet dependency to the Wyam.Feeds package in the Wyam.Blog recipe package

# 0.13.1

- [Fix] Fix for scaffolding a recipe to a non-existing directory

# 0.13.0

- [Feature] Implemented general support for recipies (#1)
- [Feature] Implemented Blog recipe
- [Refactoring] Local file system now uses non-blocking read/write sharing
- [Feature] Adds a default "theme" folder to the set of default input folders
- [Refactoring] Better automatic metadata conversion of enumerable types to atomic values
- [Refactoring] Improvements to collection and document collection extensions (#195)
- [Feature] Added `Where()` fluent method to `GroupBy` and `Paginate` modules
- [Feature] Added new `GroupByMany` module
- [Feature] Implemented themes (#72)
- [Feature] Implemented CleanBlog theme for the Blog recipe
- [Fix] Fixed some bugs with globbing and assembly loading from the file system
- [Feature] Implemented default theme support for recipes (#310)
- [Fix] Fixed a bug when watching non-existing input paths
- [Feature] Added support for initial document metadata to the CLI and Cake addin (#11)
- **[Breaking Change]** [Refactoring] Renamed the CLI argument for global metadata to `--global` (#11)
- **[Breaking Change]** [Refactoring] All metadata collections are now case-insensitive (including document metadata) (#312)
- **[Breaking Change]** [Refactoring] `IMetadataValue` no longer passes the requested key (#312)
- [Refactoring] Pipeline names are now case-insensitive (#313)
- [Feature] `IExecutionContext` now implements `IMetadata` for the global metadata (#311)
- **[Breaking Change]** [Refactoring] Method `IMetadata.Documents()` is renamed to `IMetadata.DocumentList()` to avoid conflicts with `IExecutionContext.Documents()` (#311)
- [Refactoring] Added support for different commands to the CLI (#309)
- [Feature] Added a `new` command to the CLI for scaffolding initial files for a recipe (#309)
- [Feature] Added a `preview` command to the CLI for previewing a site without actually building it (#309)
- [Feature] Added a `OnlyMetadata()` method to the `WriteFiles` module that adds the same metadata the `WriteFiles` module usually does but does not actually write the files to disk (useful for staging files in one pipeline and then writing them in another)
- [Fix] Warns when NuGet communication fails (instead of terminating)
- [Feature] Adds new empty `IExecutionContext.GetLink()` method to get a bare site link
- **[Breaking Change]** [Feature] New `GenerateFeeds` module in `Wyam.Feeds` package to replace legacy `Rss` module (can generate RSS, Atom, RDF) (#322)
- [Fix] Fixes for nullables in metadata type conversion
- [Fix] Fix for stemming toggle in `SearchIndex` module (#315)
- [Refactoring] Assemblies are now loaded in parallel for performance
- [Refactoring] Removed beta pre-release version designation

# 0.12.4

- **[Breaking Change]** [Refactoring] Extension NuGet packages have been renamed from `Wyam.*` to `Wyam.*` to better represent other non-module extension points (#295)
- **[Breaking Change]** [Feature] The NuGet package version now takes a *version range*, so you must use `[x.y.z]` instead of `x.y.z` to specify a specific version
- [Feature] Added a `use-global-source` flag to trigger the use of globally configured NuGet package sources
- [Feature] WriteFiles now supports an `Append()` method to trigger appending to existing file instead of overwritting them (#304)
- [Refactoring] WriteFiles now has better handling of outputting multiple documents to the same file (#303)
- [Feature] New `Sidecar` module to pull metadata for a document from a sidecar file (#306, thanks @LokiMidgard)
- [Refactoring] Additional documentation for Cake alias (#302, thanks @gep13)
- [Refactoring] New assembly type scanning algorithm will make future extension points easier to support (#281)
- [Refactoring] File providers are now specified as a URI (#277)
- [Feature] New `Tree` module to construct a hierarchy from a set of documents (#292, thanks @LokiMidgard)
- [Fix] Output directory is now created on demand instead of automatically at execution (#293)
- [Fix] Check for null stream if null content on `Document.ToString()` (#294, thanks @LokiMidgard)
- [Refactoring] Renamed CLI flag `--pause` to `--attach` and changed behavior to wait for a debugger to attach instead of requiring key press
- [Fix] Fixed some bugs with the Cake.Wyam NuGet package dependencies (#291)
- [Fix] Fixed some problems with the handling of dotfiles (#289)
- [Fix] Documentation fixes for `IDocument` (#288, thanks @LokiMidgard)

# 0.12.3

- [Fix] Fixed a bug where execution could hang in some environments that open stdin and leave it open like Azure CI or VS Code task execution (#287)
- [Feature] Added a `--latest` flag to the `#nuget` preprocessor directive to indicate that the latest available package version should always be installed
- **Breaking changes**: Please see https://wyam.io/docs/advanced/migrating-to-0.12.x for more information

# 0.12.2

- [Fix] Emergency resolution for a bug with NuGet operations ordering an file locks
- **Breaking changes**: Please see the release notes for 0.12.1 for more information

# 0.12.1

- **[Breaking Change]** [Refactoring] Non-core module packages are no longer included with the default distribution and will need to be downloaded and installed by NuGet at runtime, see https://wyam.io/knowledgebase/migrating-to-0.12.x for more information (#275)
- [Feature] Preprocessor directives are now supported in your configuration files for NuGet package and assembly loading (#274)
- [Feature] The CLI also supports a similar syntax to the preprocessor directives for specifying NuGet packages and assemblies on the command line (#280)
- [Feature] We now have a Windows installer! (#127 and #283, thanks @FlorianRappl)
- [Feature] We also now have a Cake addin! (#129 and #276, thanks @gep13)
- [Refactoring] Migrated internal NuGet libraries to v3 (#190)
- [Refactoring] Moved configuration file and NuGet API logic out of Wyam.Core to new Wyam.Configuration library (#279)

# 0.12.0

- **MAJOR BREAKING CHANGES, BEWARE YE WHO ENTER HERE!*- - The entire I/O stack has (finally) been rewritten and modules that were using `System.IO` classes have been ported to the new abstraction - read about how to migrate your pre 0.12.0 code at https://wyam.io/docs/advanced/migrating-to-0.12.x and keep on the lookout for bugs
- [Feature] New globbing engine with brace expansion, negation, and wildcard support (see https://wyam.io/docs/concepts/io for more details)
- **[Breaking Change]** [Refactoring] ReadFiles now uses the new I/O API and several configuration methods have been removed or changed
- **[Breaking Change]** [Refactoring] `IDocument.Source` is now a `FilePath`
- [Feature] You can now explicitly specify if a given `FilePath` or `DirectoryPath` is absolute or not
- **[Breaking Change]** [Refactoring] Moved `IMetadata.Link()` to `IExecutionContext.GetLink()`
- [Feature] Control global link settings (like hostname or whether to hide extensions) from `ISettings` (available from config file as `Settings`)
- [Feature] New `MinifyHtml` module that can minify HTML content (#260, thanks @leekelleher)
- [Feature] New `MinifyCss` module that can minify CSS content (#266, thanks @leekelleher)
- [Feature] New `MinifyJs` module that can minify JavaScript content (#266, thanks @leekelleher)
- [Feature] New `MinifyXhtml` module that can minify XHTML content (#266, thanks @leekelleher)
- [Feature] New `MinifyXml` module that can minify XML content (#266, thanks @leekelleher)
- [Feature] Added `IMetadata.FilePath()` and `IMetadata.DirectoryPath()` to make getting strongly-typed paths from metadata easier
- **[Breaking Change]** [Refactoring] Refactored several methods in `FilePath` and `DirectoryPath` into properties (`FilePath.GetFilename()` to `FilePath.Filename`, etc.)
- [Fix] Added support for root sequences to the Yaml module
- [Fix] Engine now retains global metadata between invocations in watch mode (#269, thanks @miere43)
- [Feature] New `Combine` module that can combine multiple documents into one
- [Refactoring] Content from NuGet packages is no longer copied to a staging folder and is instead retrieved directly

# 0.11.5

- [Fix] Well this is embarrassing - fix for the fix to properly handle undownloaded NuGet packages

# 0.11.4

- [Fix] Ongoing work with IO abstraction layer (#123, #214) to resolve some file system errors on build

# 0.11.3

- [Fix] Specifying input path(s) on the command line now correctly removes the default input path (#241 and #231)
- [Fix] Correctly handle paths that contain single . chars (#244)
- [Fix] Duplicate trace messages when config file is changed in watch mode (#243)
- [Feature] New support for specifying global metadata on the command line and accessing it from config files, the engine, and the execution context (#233 and #237, thanks @miere43)
- [Fix] Incorrect number of pipelines reported in output (#235 and #236, thanks @miere43)
- [Fix] Exceptions are now highlighted in the CLI (#230 and #232, thanks @miere43)

# 0.11.2

- A special thanks to @deanebarker who contributed a ton of new functionality as well as generated lots of great ideas, discussion, and bug reports - this release wouldn't be what it was without his help and support
- Note that the I/O abstraction support is still under development and may continue to change in the next version
- [Feature] Support for custom document types (#183)
- [Feature] Support for reading from stdin, including a new `ReadApplicationInput` module (#226, thanks @deanebarker)
- [Feature] New command-line switch `--verify-config` for validating the config file without actually running (#223, thanks @deanebarker)
- [Feature] New `ValidateMeta` module for validating metadata (#220, thanks @deanebarker)
- [Feature] New command-line switch `--preview-root` to set an alternate root path for the preview server (#213, thanks @deanebarker)
- [Feature] New `Merge` module that merges the content and metadata from documents output by a module sequence into the input documents 
- **[Breaking Change]** [Refactoring] ReadFiles now outputs new documents and only reads the files once if supplying a search pattern, to replicate the old behavior where ReadFiles read the matching files for each input document wrap it in the new Merge module
- [Feature] New `GenerateCloudSearchData` module to generate JSON data for AWS Cloud Search (#213, thanks @deanebarker)
- [Feature] New `Take` module that only outputs the first N input documents (#208 and #211, thanks @deanebarker)
- [Feature] New `CopyMeta` module to allow copying metadata from one key to another with optional formatting (#209 and #207, thanks @deanebarker)
- [Feature] Added `.WithGuidMetaKey()` to the Rss module to specify a metadata item that has the GUID (#206, thanks @deanebarker)
- [Feature] A tools NuGet package is now published that includes the CLI (#204)
- [Feature] Added void delegates to Execute module so you no longer have to return a value (#194)
- [Feature] Added `.IsRegex()` to Replace module (#201 and #203, thanks @deanebarker)
- [Feature] New `ModuleCollection` module to wrap a sequence of child modules for reuse (#197)
- [Feature] Added `IMetadata.Documents()` to return an `IReadOnlyList<IDocument>` from metadata (#200)
- [Feature] Added `IMetadata.Document()` to return an `IDocument` from metadata
- [Fix] Type conversions now properly take compatibility with enumerated item type into account (#198)
- [Fix] Fixed possible race condition when cloning documents with different sources (#196)
- **[Breaking Change]** [Feature] Implemented new IO framework and replaced all uses of strings for passing path information with new IO classes, `Engine.RootFolder`, `Engine.InputFolder`, and `Engine.OutputFolder` are now replaced by `IFileSystem.RootPath`, `IFileSystem.InputPaths`, `IFileSystem.OutputPath` (#123)
- **[Breaking Change]** [Refactoring] Changed `Trace` to a static class to better support forthcoming parallel pipeline processing
- **[Breaking Change]** [Refactoring] `Metadata` property in config file renamed to `InitialMetadata` to distinguish from run-time metadata
- [Refactoring] Removed the need to pass `Engine` to many core classes since it was just needed for the `Trace` instance (which is now static)
- [Refactoring] Split internal configuration classes for better separation of concerns and testability
- [Refactoring] Added Wyam.Testing library with common testing classes
- [Refactoring] Reorganized tests to better follow a specific convention
- [Refactoring] Changed color of critical errors in the console to white on a red background for better readability (#182)
- [Refactoring] Changed model type of Razor pages to `IDocument` instead of `IMetadata` (#188)
- [Refactoring] Uncaught exceptions now cancel the build (#187)

# 0.11.1

- **[Breaking Change]** [Refactoring] Changed the name of the release file from `wyam.zip` to `wyam-[verson].zip`
- [Fix] Namespaces are now added for `Engine.Configure()` calls without a script (#147 and #148, thanks @heavenwing)
- [Fix] CopyFiles was no returning a sequence for invalid folders (#166)
- [Fix] Better documentation and small bug fixes for CopyFiles (#158)
- [Fix] Excludes the output folder from watching (#156)
- [Fix] Fixes some odd behavior with FileName used with WriteFiles (#159)
- [Fix] Better failure handling to overcome locked file errors when watching (thanks @miere43)
- [Fix] Implemented file operation retry for several modules (#167, #169)
- [Feature] New `Rss` module (#174, thanks @miere43)
- [Feature] Added support for unknown file types to preview server (#175)
- [Feature] Added more specific error codes (#171)
- [Refactoring] Small improvements to Yaml module (better error messages, parallel processing) (#177)
- [Feature] New `GitHub` module (#145)
- [Feature] Added `Meta.IgnoreNull()` to control if null values should still be added to metadata
- [Feature] Added `IMetadata.Dynamic()` to return metadata values as dynamic objects
- [Refactoring] Adds an extra optional key to the cache for use by module authors
- [Feature] Added additional Markdown module constructor to render Markdown content in metadata
- [Feature] New `Json` and `GenerateJson` modules to read and write JSON data (#5)
- [Feature] Added support for setting input and output path from the configuration script
- [Fix] Git module now correctly ascends folder hierarchy
- [Refactoring] Migrated build system to Cake

# 0.11.0

- [Feature] New `Download` module for downloading content during generation (#68 and #75, thanks @dodyg)
- [Feature] New `AnalyzeCSharp` module that performs code analysis on C# source files, including XML documentation comments (#70)
- [Feature] New `ReadProject` module that loads all the source files referenced in an MSBuild project (#70)
- [Feature] New `ReadSolution` module that loads all the source files referenced in an MSBuild solution (#70)
- [Feature] Added `WithoutExtensions()` to CopyFiles to make it easier to specify which files to copy
- [Feature] Added additional `IExecutionContext.GetNewDocument()` overloads
- [Refactoring] Moved `Crc32` to `Wyam.Common` so other module libraries can also use it
- [Fix] NuGet support now considers .NET 4.6 packages when resolving libraries
- [Fix] Console application now exists with a non-zero error code so automated builds can be aborted if something goes wrong
- [Refactoring] Additional trace output for debugging, especially in NuGet resolve process
- [Refactoring] Better example test fixture (thanks @yetanotherchris)
- [Refactoring] Moved `Wyam.Common` types to appropriate nested namespaces
- [Feature] Added `WithMatchOnlyWholeWord()` to AutoLink module (#80, thanks @LokiMidgard)
- [Feature] New `HtmlEscape` module for escaping HTML characters (#81, thanks @LokiMidgard)
- [Feature] Additional `ToLookup()`, `ToDocumentDictionary()`, and `ToDictionary()` extension methods for `IDocumentCollection` (#82)
- [Feature] Added new `Execute()` constructor to Execute module that only gets evaluated once per pipeline (#83, thanks @LokiMidgard)
- [Feature] New `GitCommits` and `GitContributors` modules for getting information from Git repositories (#84, thanks @LokiMidgard)
- [Feature] New `ConcatBranch` module for branching and then concatenating the output (#87, thanks @LokiMidgard)
- [Feature] New `DirectoryMeta` module for applying metadata supplied for an entire directory to all documents sourced from that directory (#86, thanks @LokiMidgard)
- [Fix] Fixed several file locking issues (#92)
- [Feature] Performance boost by using parallel disposal at the end of pipelines
- [Feature] Added `ThenBy()` to OrderBy module (#101, thanks @LokiMidgard)
- [Feature] Can now output the config script files the way Roslyn sees them (after adding modules, etc.) for debugging purposes using the `--output-scripts` flag (#99)
- [Feature] New `FileName` module for generating SEO-friendly file names (#99, #104, and #107, thanks @yetanotherchris)
- [Feature] Added `UseWriteMetadata()` to WriteFiles module to control if metadata values are used for file destination (#106)
- [Feature] Default for FrontMatter module is now to ignore delimiter(s) if on the first line
- [Feature] All configured namespaces are added as `IExecutionContext.Namespaces` for other code generating modules to use
- [Feature] WriteFiles module now sets relative path information metadata (#111, thanks @reliak)
- [Feature] New `Sitemap` module to generate standards-compliant sitemap files (#113, thanks @reliak)
- [Feature] New `Xmp` module to import XMP encoded metadata (#114, thanks @LokiMidgard)
- [Feature] Updates to CopyFiles for better path handling (#116, thanks @reliak)
- [Feature] New `SearchIndex` module to generate Lunr.js compliant search indexes (#118, thanks @reliak)
- [Feature] New `Switch` module to provide `switch` statement like control for a pipeline (#120, thanks @reliak)
- [Feature] Added `IgnoreEmptyContent()` to WriteFiles module to ignore empty files (#117)
- [Feature] New `Xslt` module to apply XSLT transformations (#126, thanks @LokiMidgard)
- [Feature] New `CsvToHtml` and `ExcelToCsv` modules for working with tabular data (#132, thanks @LokiMidgard)
- [Refactoring] Removed static helper methods in `MetadataHelper` and replaced with `MetadataItem` and `MetadataItems` classes to make working with metadata easier
- [Refactoring] Renamed `Wyam.Core.MetadataKeys` to `Keys` and moved to `Wyam.Common` for common access (#131)

# 0.10.0

- [Feature] Added additional overloads to the `Documents` module that allow you to create new documents from scratch
- [Feature] New `IExecutionContext.GetNewDocument(...)` method that also allows you to create a new document from scratch from within your modules
- [Feature] New `GenerateMeta` module that procedurally generates metadata text using a text template (#55)
- [Feature] New `GenerateContent` module that procedurally generates document content using a text template (#55)
- **[Breaking Change]** [Refactoring] Renamed several fluent methods to better follow accepted fluent method naming conventions (I.e., `Xyz(...)` to `WithXyz(...)`)
- [Fix] Resolved memory consumption problems with using a large number of Razor templates by caching referenced assemblies (#69)
- [Feature] New `UnwrittenFiles` module that can prevent processing pipeline documents that have already been written to disk in a prior generation (#56)
- [Feature] Added `.WithExtensions(...)` fluent method to the `ReadFiles` module that makes it easier to restrict file reads to a set of file extensions
- [Feature] New `HtmlQuery` module that provides powerful querying of HTML content and sets result document content and/or metadata based on the results

# 0.9.0

- [Feature] New `Paginate` module for partitioning documents into pages (#60)
- [Feature] New `GroupBy` module for grouping documents
- [Feature] New `OrderBy` module for ordering documents (#61)
- [Feature] Several new constructor overloads to the `Documents` module that allow you get documents based on context.
- [Fix] Errors in the config file now report the correct line numbers (you will never know the pain I felt trying to get this to work correctly) (#67)
- [Refactoring] Changed `IDocument.Stream` to `IDocument.GetStream()` which now returns a wrapper stream that blocks on usage and must be disposed after use to prevent concurrent stream access for the same stream (#65)
- [Feature] Added support for automatic lambda expansion of `@doc` and `@ctx` variables in the configuration file
- [Feature] Added `ContextConfig` and `DocumentConfig` delegates to support the new automatic lambda expansion and encourage a standardized way of lazily configuring a module (#63)
- [Feature] Added `ConfigHelper` to support the common module configuration pattern where a module has either a `ContextConfig` or a `DocumentConfig`
- [Refactoring] Removed the `ConcatDocuments` module. The behavior can be easily represented by using other existing modules: `Concat(Documents(...))`
- [Fix] Fixed a bug in the Execute module when returning null
- [Fix] Fixed a bug in the ReadFiles module when no files match the specified pattern
- [Fix] Fixed some bugs in automatic module constructor factory method generation for configuration files for generic modules
- [Refactoring] now using more collections from System.Collections.Immutable for improved performance and safety

# 0.8.0

- [Feature] Improved caching support in multiple modules including Razor and Markdown
- [Feature] New `IDocument.Source` property to uniquely identify the source of each document (#38)
- [Feature] New `AutoLink` module for automatically generating links within HTML content (#36)
- [Refactoring] Major changes to how Wyam handles the storage of content inside a document to include operating on streams and/or string buffers for increased performance (#42)
- [Refactoring] Improvements to document and engine life cycle for proper disposal of resources
- [Feature] New `Images` module for manipulation of images within a pipeline (#25 - thanks @dodyg)
- [Refactoring] Renamed `Wyam.Abstractions` to `Wyam.Common` to reflect that it also contains non-abstract common code (#45)
- [Feature] New `ForEach` module that allows processing child documents one at a time (#47)
- [Fix] Fixes to how the Less module handles relative URLs (#49 - thanks @rohansi)
- [Refactoring] Made the cache thread-safe for parallel processing within a module (#50)
- [Refactoring] Changed behavior of several modules including Razor, Less, Markdown, ReadFiles, and WriteFiles, to process documents in parallel resulting in significant performance improvements
- [Feature] ASCII art FTW
- [Refactoring] Moved `PathHelper` to `Wyam.Common`
- [Feature] New `Excerpt` module that can excerpt the first specified element from HTML content and add it to metadata (#26)
- [Feature] New `processDocumentsOnce` flag that can skip any documents previously processed by the pipeline (#56)
- [Fix] Fixed a bug with Razor sections and `IRazorPage` instance reuse on subsequent pipeline executions (#58)

# 0.7.0

- Moved all the libraries from alpha to beta to reflect improving stability
- [Refactoring] Updated all projects and solution to Visual Studio 2015 and .NET 4.6
- [Feature] Added ability to specify alternate ViewStart paths for the Razor module (#37 - thanks @mschumaker)
- [Feature] Added ability to specify file ignore prefixes for the Razor module, files prefixed with `_` are now ignored by default (#37 - thanks @mschumaker)
- [Feature] Added additional metadata to `ReadFiles`: `SourceFilePathBase`, `RelativeFilePathBase`, and `RelativeFileDir`
- [Feature] Added additional metadata to `WriteFiles`: `DestinationFileBase`, `DestinationFileExt`, `DestinationFileName`, `DestinationFileDir`, and `DestinationFilePathBase`
- [Fix] Better normalization of file paths
- [Feature] Adds a new `string IMetadata.Link(string key, string defaultValue = null)` method for automatically formatting links for HTML use (I.e., replacing slashes)
- [Refactoring] Updated to Roslyn 1.0
- [Feature] Adds a new universal `IExecutionCache` for modules to use for caching data

# 0.6.0

- [Feature] Yaml module now maps simple scalar sequences to metadata when flattening
- [Refactoring] Switched from Roslyn scripting engine to direct compilation for configuration files, improving capabilities of configuration file declarations (extension classes, etc.)
- [Fix] Lots of fixes and updates for assembly locating and loading
- [Feature] Implemented support for custom Razor base page types
- [Feature] Added new `Documents` module that replaces the documents in the pipeline with alternate ones
- [Feature] Added overload to the `Execute` module that allows use of the execution context
- [Refactoring] Moved `MetadataKeys` class out of `Wyam.Abstractions` to `Wyam.Core` so that `Wyam.Abstractions` would be more stable, though this means user code will have to go back to strings for metadata keys
- [Feature] Added ability to override write location in the `WriteFiles` module on a per-document basis by setting specific metadata
- [Feature] `IDocument` now implements `IMetadata` so metadata can be retrieved directly from the document object
- [Feature] Added a `.String(...)` method to `IMetadata` to make getting string values easier
- [Refactoring] Created a new `IDocumentCollection` interface and implementations to provide a better API for getting documents during execution
- [Feature] Implemented support for converting metadata between array types and between scalars and array types
- [Feature] Implemented a `.ToLookup<T>(...)` extension method to generate a lookup table of metadata values to documents (for example, get all tags and the documents that define those tags)

# 0.5.0

- [Feature] Implemented a **Less CSS*- module
- [Fix] Fixes for NuGet package dependencies
- [Feature] Can now declare prerelease NuGet packages in configuration file without specifying version specification
- [Feature] Calls to `System.Diagnostics.Trace` from third-party libraries are now picked up by a custom `TraceListener` and output to the Wyam `TraceSource`
- [Refactoring] Renamed several metadata keys for consistency
- [Feature] `Engine` now implements `IDisposable` for better lifecycle management

# 0.4.0

- [Feature] Added a `Concat` module to concatenate documents from child modules with the current documents
- [Feature] Added a `ConcatDocuments` module to concatenate documents from previous pipelines with the current documents
- [Feature] Added a `Where` module to filter the current documents with a predicate
- [Refactoring] Moved the predicate in the `Branch` module to a fluent `.Where(...)` method to follow established conventions
- [Feature] `IPipelineCollection` now implements `IReadOnlyDictionary<string, IPipeline>`
- [Feature] `IPipeline` now implements `IList<IModule>`
- [Feature] A new `IReadOnlyPipeline` interface that implements `IReadOnlyList<IModule>` has been introduced for use during execution (since pipelines can't be changed once execution starts)
- [Feature] Added a test fixture to run all examples during testing
- [Feature] Added `RootFolder`, `InputFolder`, and `OutputFolder` as available properties in the configuration script
- [Refactoring] Renamed the `Metadata` module to `Meta` so it wouldn't conflict with the `Metadata` property in configuration scripts
- [Feature] Specifying `--watch` now also watches the configuration file and initializes a new engine if it changes
- [Refactoring] Updated the default configuration (for when no configuration file is specified or found) to use a single pipeline for Markdown and Razor documents

# 0.3.0

- [Refactoring] Added the Roslyn nightly packages to source control for easier builds (since they don't stay on the feed for long)
- [Feature] Added sequence of configured assemblies to the `IExecutionContext` (so modules like Razor can pick them up)
- [Feature] Added a new `MetadataKeys` static class to `Wyam.Abstractions` to help eliminate magic strings

# 0.2.0

- [Feature] Implements default configuration
- [Feature] CopyFiles now sets metadata values for SourcePath and DestinationPath for each document
- [Fix] Crashes when certain directories didn't exist
- [Refactoring] Trace module now follows fluent conventions

# 0.1.1

- [Fix] Fixes a bug with parsing some of the command line argument options
- [Feature] Adds support for specifying input and output folder on the command line

# 0.1.0

- Initial release!