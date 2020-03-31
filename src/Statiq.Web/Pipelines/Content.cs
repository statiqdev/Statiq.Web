using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Statiq.App;
using Statiq.Common;
using Statiq.Core;
using Statiq.Html;
using Statiq.Markdown;
using Statiq.Razor;
using Statiq.Web.Modules;
using Statiq.Yaml;

namespace Statiq.Web.Pipelines
{
    public class Content : Pipeline
    {
        public Content()
        {
            Dependencies.Add(nameof(Data));

            InputModules = new ModuleList
            {
                new ReadFiles("**/{!_,}*.{html,cshtml,md}")
            };

            ProcessModules = new ModuleList
            {
                new ProcessIncludes(),
                new ExtractFrontMatter(new ParseYaml()),
                new FilterDocuments(Config.FromDocument(doc => !Archives.IsArchive(doc))),
                new EnumerateValues(),
                new AddTitle(),
                new SetDestination(".html"),
                new ProcessMarkup(),
                new GenerateExcerpt(), // Note that if the document was .cshtml the except might contain Razor instructions or might not work at all
                new GatherHeadings(),
                new OrderDocuments(),
                new CreateTree().WithNesting(true, true)
            };

            PostProcessModules = new ModuleList
            {
                new FlattenTree(),
                new FilterDocuments(Config.FromDocument(doc => !doc.GetBool(Keys.TreePlaceholder))), // Don't render placeholder pages
                new ProcessTemplates()
            };

            OutputModules = new ModuleList
            {
                new WriteFiles()
            };
        }
    }
}
