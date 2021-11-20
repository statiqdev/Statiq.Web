using Statiq.Common;
using Statiq.Core;
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
                new GetPipelineDocuments(ContentType.Content),

                // Filter to non-archive content
                new FilterDocuments(Config.FromDocument(doc => !Archives.IsArchive(doc))),

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