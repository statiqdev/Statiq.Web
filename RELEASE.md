# 1.0.0-alpha.21

# 1.0.0-alpha.20

- Added `OutputPath` setting so the output path can be set from the command-line or configuration file.
- Added `ExcludedPaths` setting so excluded paths can be set from the command-line or configuration file.
- Added `InputPaths` setting so input paths can be set from the command-line or configuration file.
- Updated Statiq Framework to version [1.0.0-beta.16](https://github.com/statiqdev/Statiq.Framework/releases/tag/v1.0.0-beta.16).

# 1.0.0-alpha.19

- Added the new `Statiq.Web.props` file to a `buildTransitive` folder in the package so it flows transitively to consumers.

# 1.0.0-alpha.18

- Bug fix for unclosed `<ItemGroup>` in the new props file (#909, thanks @drmathias).

# 1.0.0-alpha.17

- Added a `IncludeInSitemap` setting to control whether a document should be included in the sitemap (#907, thanks @drmathias).
- Fixed a bug that required feed items to have URI IDs when the specification indicates they can also be arbitrary strings (#906).
- Added a props file to the Statiq.Web package to automatically set the default theme, extensions, and archetypes MSBuild props.
- Updated Statiq Framework to version [1.0.0-beta.14](https://github.com/statiqdev/Statiq.Framework/releases/tag/v1.0.0-beta.14).
- Added a `GatherHeadingsLevel` key that can be used to adjust the headings level when gathering headings globally or per-document (#904).

# 1.0.0-alpha.16

- Updated Statiq Framework to version [1.0.0-beta.13](https://github.com/statiqdev/Statiq.Framework/releases/tag/v1.0.0-beta.13).
- Added support for reading `themesettings.json` and `statiq.json` from the theme paths.
- Added support for theme paths and configuring them via the bootstrapper and/or settings.

# 1.0.0-alpha.15

- Added redirect support.
- Added deployment support for Azure App Service.
- Added deployment support for Netlify.
- Added deployment support for GitHub Pages.
- Added `Enumerate` support to data files.

# 1.0.0-alpha.14

- Added support for directory metadata for data files.
- Added support for front matter in data files.
- Added support for sidecar files as `_[filename].[json|yaml]`.
- Added `ProcessSidecarFiles` setting to turn sidecar files off.
- Added `ApplyDirectoryMetadata` setting to turn directory metadata off.
- Added better xref error messages.

# 1.0.0-alpha.13

- Fixed a bug in the preview command that exited on failures.
- Changed preview server to listen to any hostname/IP on the specified port (this allows use from services like Gitpod).
- Renamed the root namespaces of the extension libraries brought over from Statiq Framework to match new project names.

# 1.0.0-alpha.12

- Added a new `Bootstrapper.AddWeb()` extension to add Statiq Web functionality to an existing bootstrapper.

# 1.0.0-alpha.11

- Changed resource mirroring to be opt-in instead of opt-out (you now need to set `MirrorResources` to `true` to enable) (#896).
- Fix to filter tree placeholder pages out of the `Sitemap` pipeline (#895).

# 1.0.0-alpha.10

- Changed the default theme input path to "theme/input" in preparation for work on dedicated theme folders (see #891).
- Added a new `RenderPostProcessTemplates` key that prevents running post-processing templates like Razor.
- Added a new `ShouldOutput` key that controls outputting a particular document to disk (but doesn't remove it from the pipeline like `Excluded` does).
- Added support for directory metadata (by default as `_directory.yaml` files).
- Added new `ContentFiles` and `DataFiles` settings to control the file globbing patterns.
- Added a new `GenerateSitemap` setting and `Sitemap` pipeline to generate sitemap files by default.
- Added a new `Excluded` key that indicates a document should be filtered out of the content or data pipeline.
- Fixed a bug with feeds not flattening the content document tree.

# 1.0.0-alpha.9

- Fixed xref resolution to report all errors in a given document at once.
- Changed the xref space placeholder character to a dash to match/roundtrip automatic file name titles.
- Removed the `ChildPages` shortcode (it should really be part of the theme).

# 1.0.0-alpha.8

- Added support for validating links.
- Refactored xref error messages to display for all documents at once (instead of one at a time).

# 1.0.0-alpha.7

- Added xref support for links like "xref:xyz" where "xyz" is the value of the "Xref" metadata, the document title with spaces converted to underscores if no "Xref" value is defined, or the source file name if neither of those are available.
- Added `IExecutionContext.TryGetXrefDocument()` and `IExecutionContext.GetXrefDocument()` extension methods to get a document by xref.
- Added `IExecutionContext.TryGetXrefLink()` and `IExecutionContext.GetXrefLink()` extension methods to get a document link by xref.

# 1.0.0-alpha.6

- Added support for Handlebars for files with a ".hbs" or ".handlebars" extension.
- Added ability to specify a default template via the `Bootstrapper.SetDefaultTemplate()` extension.
- Added a powerful capability to add, modify, and remove template modules like Markdown, Razor, etc. via the `Bootstrapper.ConfigureTemplates()` extension.
- Refactored metadata processing into a new common `ProcessMetadata` module.
- Added the `OptimizeFileName` module with `OptimizeContentFileNames` and `OptimizeDataFileNames` settings to control it.
- Added the `SetDestination` module to the "Data" pipeline.

# 1.0.0-alpha.5

- Refactored the `ReadGitHub` module to take configuration values.
- The "Content" and "Data" pipelines now concatenate all documents from pipelines that declare themselves a dependency using `IPipeline.DependencyOf`.

# 1.0.0-alpha.4

- Added a new DeployGitHubPages module.
- Moved the preview and serve commands into Statiq.Web from Statiq.App.
- Moved Statiq.GitHub into Statiq Web as Statiq.Web.GitHub from Statiq Framework. 
- Moved Statiq.Netifly into Statiq Web as Statiq.Web.Netlify from Statiq Framework. 
- Moved Statiq.Azure into Statiq Web as Statiq.Web.Azure from Statiq Framework. 
- Moved Statiq.Aws into Statiq Web as Statiq.Web.Aws from Statiq Framework. 
- Moved Statiq.Hosting into Statiq Web as Statiq.Web.Hosting from Statiq Framework. 
- Moved HTML-based shortcodes from Statiq.Core.
- Fixed a bug with `ArchiveKey` when using a string-based key.
- Added support for setting archive document source.
- Added the `GatherHeadings` module.
- Added a `ChildPages` shortcode.
- Added shortcode support.
- Added `CreateTree`/`FlattenTree` to the Content pipeline.
- Added support for ordering documents in the Content pipeline using the "Index" metadata value.

# 1.0.0-alpha.3

- Added a new Feeds pipeline that creates RSS and Atom feeds based on a definition file.
- Added a new Data pipeline that reads YAML and JSON files.
- Added excerpt generation (in the "Excerpt" metadata key) to the Content pipeline.

# 1.0.0-alpha.2

- Added an Archives pipeline that can create archive indexes, groups, and pages.

# 1.0.0-alpha.1

- Initial version with Content, Assets, Less, and Sass pipelines.