using System;
using System.Collections.Generic;
using System.Text;
using Statiq.App;
using Statiq.Common;
using Statiq.Core;
using Statiq.Html;
using Statiq.Markdown;
using Statiq.Razor;
using Statiq.Yaml;

namespace Statiq.Web
{
    public class Pages : Pipeline
    {
        public Pages()
        {
            InputModules = new ModuleList
            {
                new ReadFiles("**/{!_,}*.{cshtml,md}")
            };

            ProcessModules = new ModuleList
            {
                new CacheDocuments
                {
                    new ProcessIncludes(),
                    new ExtractFrontMatter(new ParseYaml()),
                    new ExecuteIf(
                        Config.FromDocument(doc => doc.MediaTypeEquals("text/markdown")),
                        new RenderMarkdown().UseExtensions()),
                    new AddTitle(),
                    new SetDestination(".html")
                },
                new CreateTree().WithNesting(true, true)
            };

            TransformModules = new ModuleList
            {
                new FlattenTree(),
                new FilterDocuments(Config.FromDocument(doc => !doc.GetBool(Keys.TreePlaceholder))),
                new RenderRazor()
                    .WithLayout(Config.FromDocument((doc, ctx) =>
                    {
                        // Crawl up the tree looking for a layout
                        IDocument parent = doc;
                        while (parent != null)
                        {
                            if (parent.ContainsKey(WebKeys.Layout))
                            {
                                return parent.GetFilePath(WebKeys.Layout);
                            }
                            parent = parent.GetParent(ctx.Inputs);
                        }
                        return null;  // If no layout metadata, revert to default behavior
                    })),
                new MirrorResources()
            };

            OutputModules = new ModuleList
            {
                new WriteFiles()
            };
        }
    }
}
