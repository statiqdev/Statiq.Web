using System.Collections.Generic;
using System.Linq;
using Statiq.Common;
using Statiq.Core;
using Statiq.Html;
using Statiq.Web.Modules;

namespace Statiq.Web.Pipelines
{
    public class Content : Pipeline
    {
        public Content(Templates templates)
        {
            Dependencies.AddRange(nameof(Data), nameof(DirectoryMetadata));

            InputModules = new ModuleList
            {
                new ReadFiles(Config.FromSetting<IEnumerable<string>>(WebKeys.ContentFiles))
            };

            ProcessModules = new ModuleList
            {
                // Concat all documents from externally declared dependencies (exclude explicit dependencies above like "Data")
                new ConcatDocuments(Config.FromContext<IEnumerable<IDocument>>(ctx => ctx.Outputs.FromPipelines(ctx.Pipeline.GetAllDependencies(ctx).Except(Dependencies).ToArray()))),

                // Process directory metadata, sidecar files, and front matter
                new ProcessMetadata(),

                // Filter out excluded documents
                new FilterDocuments(Config.FromDocument(doc => !doc.GetBool(WebKeys.Excluded))),

                // Filter out archive documents (they'll get processed by the Archives pipeline)
                new FilterDocuments(Config.FromDocument(doc => !Archives.IsArchive(doc))),

                // Enumerate metadata values
                new EnumerateValues(),

                new CacheDocuments
                {
                    new AddTitle(),
                    new SetDestination(".html"),
                    new ExecuteIf(Config.FromSetting(WebKeys.OptimizeContentFileNames, true))
                    {
                        new OptimizeFileName()
                    },
                    new RenderProcessTemplates(templates),
                    new GenerateExcerpt(), // Note that if the document was .cshtml the excerpt might contain Razor instructions or might not work at all
                    new GatherHeadings(Config.FromDocument(WebKeys.GatherHeadingsLevel, 1))
                },

                new OrderDocuments(),
                new CreateTree(),
                new RemoveTreePlaceholders(), // Filter out the placeholder documents right away
            };

            PostProcessModules = new ModuleList
            {
                new RenderPostProcessTemplates(templates)
            };

            OutputModules = new ModuleList
            {
                new FilterDocuments(Config.FromDocument(WebKeys.ShouldOutput, true)),
                new WriteFiles()
            };
        }
    }
}
