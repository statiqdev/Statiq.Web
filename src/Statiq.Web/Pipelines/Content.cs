using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc.Formatters;
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
        public Content(Templates templates)
        {
            Dependencies.Add(nameof(Data));

            InputModules = new ModuleList
            {
                new ReadFiles(Config.FromSetting<IEnumerable<string>>(WebKeys.ContentFiles))
            };

            ProcessModules = new ModuleList
            {
                // Concat all documents from externally declared dependencies (exclude explicit dependencies above like "Data")
                new ConcatDocuments(Config.FromContext<IEnumerable<IDocument>>(ctx => ctx.Outputs.FromPipelines(ctx.Pipeline.GetAllDependencies(ctx).Except(Dependencies).ToArray()))),

                // Process front matter and sidecar files
                new ProcessMetadata(),

                // Filter out excluded documents
                new FilterDocuments(Config.FromDocument(doc => !doc.GetBool(WebKeys.Excluded))),

                // Filter out archive documents (they'll get processed by the Archives pipeline)
                new FilterDocuments(Config.FromDocument(doc => !Archives.IsArchive(doc))),

                new EnumerateValues(),
                new AddTitle(),
                new SetDestination(".html"),
                new ExecuteIf(Config.FromSetting(WebKeys.OptimizeContentFileNames, true))
                {
                    new OptimizeFileName()
                },
                new RenderProcessTemplates(templates),
                new GenerateExcerpt(), // Note that if the document was .cshtml the except might contain Razor instructions or might not work at all
                new GatherHeadings(),
                new OrderDocuments(),
                new CreateTree().WithNesting(true, true)
            };

            PostProcessModules = new ModuleList
            {
                new FlattenTree(),
                new FilterDocuments(Config.FromDocument(doc => !doc.GetBool(Keys.TreePlaceholder))), // Don't render placeholder pages
                new RenderPostProcessTemplates(templates)
            };

            OutputModules = new ModuleList
            {
                new WriteFiles()
            };
        }
    }
}
