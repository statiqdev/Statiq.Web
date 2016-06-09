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
using Wyam.Html;

namespace Wyam.Blog
{
    public class Blog : IRecipe
    {
        public void Apply(IEngine engine)
        {
            engine.Pipelines.Add("Posts",
                new ReadFiles("posts/*.md"),
                new FrontMatter(new Yaml.Yaml()),
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

            // TODO: RSS feed

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

            engine.Pipelines.Add("Tags",
                new ReadFiles(@"tags/index.cshtml"),
                new FrontMatter(new Yaml.Yaml()),
                new Execute((doc, ctx) => ctx.Documents
                    .Where(x => x.ContainsKey("Published") && x.Get<DateTime>("Published") <= DateTime.Today && x.ContainsKey("Tags"))
                    .SelectMany(x => x.List<string>("Tags"))
                    .Distinct()
                    .Select(x => ctx.GetDocument(doc, new MetadataItems
                    {
                        {"Title", x},
                        {"Tag", x},
                        {"Link", $"tags/{(x.StartsWith(".") ? x.Substring(1) : x).ToLowerInvariant().Replace(' ', '-')}"}
                    }))),
                new Razor.Razor(),
                new WriteFiles((doc, ctx) => $"{doc.String("Link")}.html"));

            engine.Pipelines.Add("Resources",
                new CopyFiles("**/*{!.cshtml,!.md,}"));
        }

        public void Scaffold(IDirectory directory)
        {
            throw new NotImplementedException();
        }
    }
}
