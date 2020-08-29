using System.Collections.Generic;
using System.Linq;
using dotless.Core.Parser.Infrastructure;
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
            Dependencies.AddRange(nameof(Inputs), nameof(Data));

            ProcessModules = new ModuleList
            {
                // Get inputs
                new ReplaceDocuments(nameof(Inputs)),

                // Concat all documents from externally declared dependencies (exclude explicit dependencies above like "Inputs")
                new ConcatDocuments(Config.FromContext<IEnumerable<IDocument>>(ctx => ctx.Outputs.FromPipelines(ctx.Pipeline.GetAllDependencies(ctx).Except(Dependencies).ToArray()))),

                // Filter to non-archive content
                new FilterDocuments(Config.FromDocument(doc => doc.Get<ContentType>(WebKeys.ContentType) == ContentType.Content && !Archives.IsArchive(doc))),

                // Process the content
                new CacheDocuments
                {
                    new AddTitle(),
                    new SetDestination(true),
                    new ExecuteIf(Config.FromSetting(WebKeys.OptimizeContentFileNames, true))
                    {
                        new OptimizeFileName()
                    },
                    new RenderContentProcessTemplates(templates),
                    new ExecuteIf(Config.FromDocument(doc => doc.MediaTypeEquals(MediaTypes.Html)))
                    {
                        // Excerpts and headings only work for HTML content
                        new GenerateExcerpt(),
                        new GatherHeadings(Config.FromDocument(WebKeys.GatherHeadingsLevel, 1))
                    }
                },

                new OrderDocuments()
            };

            PostProcessModules = new ModuleList
            {
                new RenderContentPostProcessTemplates(templates)
            };

            OutputModules = new ModuleList
            {
                new FilterDocuments(Config.FromDocument(WebKeys.ShouldOutput, true)),
                new WriteFiles()
            };
        }
    }
}
