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