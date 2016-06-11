using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Core.Modules.Control;
using Wyam.Core.Modules.Extensibility;
using Wyam.Core.Modules.IO;
using Wyam.Core.Modules.Metadata;
using Wyam.Html;

namespace Wyam.Blog
{
    public class Blog : IRecipe
    {
        public void Apply(IEngine engine)
        {
            // TODO: RSS feed

            engine.Pipelines.Add("Posts",
                new ReadFiles("posts/*.md"),
                new FrontMatter(new Yaml.Yaml()),
                new Where((doc, ctx) => doc.ContainsKey("Published") && doc.Get<DateTime>("Published") <= DateTime.Today),
                new Markdown.Markdown(),
                new Concat(
                    // Add any posts written in Razor
                    new ReadFiles("posts/{!_,!index,}*.cshtml"),
                    new FrontMatter(new Yaml.Yaml())),
                new Razor.Razor(),
                new Excerpt(),
                new Excerpt("div#content")
                    .SetMetadataKey("Content")
                    .GetOuterHtml(false),
                new WriteFiles(".html"));

            engine.Pipelines.Add("Tags",
                new ReadFiles("tags/tag.cshtml"),
                new FrontMatter(new Yaml.Yaml()),
                new GroupByMany("Tags", new Documents("Posts")),
                new Meta("Tag", (doc, ctx) => doc.String(Keys.GroupKey)),
                new Meta("Posts", (doc, ctx) => doc.List<IDocument>(Keys.GroupDocuments)),
                new Meta(Keys.RelativeFilePath, (doc, ctx) =>
                {
                    string tag = doc.String(Keys.GroupKey);
                    return $"tags/{(tag.StartsWith(".") ? tag.Substring(1) : tag).ToLowerInvariant().Replace(' ', '-')}.html";
                }),
                new Razor.Razor(),
                new WriteFiles());

            engine.Pipelines.Add("TagIndex",
                new ReadFiles("tags/index.cshtml"),
                new FrontMatter(new Yaml.Yaml()),
                new Razor.Razor(),
                new WriteFiles(".html"));
            
            engine.Pipelines.Add("Content",
                new ReadFiles("{!posts,**}/*.md"),
                new FrontMatter(new Yaml.Yaml()),
                new Markdown.Markdown(),
                new Concat(
                    // Add any additional Razor pages
                    new ReadFiles("{!posts,!tags,**}/*.cshtml"),
                    new FrontMatter(new Yaml.Yaml())),
                new Concat(
                    // Add the posts index page
                    new ReadFiles("posts/index.cshtml"),
                    new FrontMatter(new Yaml.Yaml())),
                new Razor.Razor(),
                new WriteFiles(".html"));

            engine.Pipelines.Add("Resources",
                new CopyFiles("**/*{!.cshtml,!.md,}"));
        }

        public void Scaffold(IDirectory directory)
        {
            throw new NotImplementedException();
        }
    }
}
