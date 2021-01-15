using Statiq.Common;
using Statiq.Core;
using Statiq.Web.Modules;

namespace Statiq.Web.Pipelines
{
    public class Assets : Pipeline
    {
        public Assets(Templates templates)
        {
            Dependencies.Add(nameof(Inputs));

            ProcessModules = new ModuleList
            {
                new GetPipelineDocuments(ContentType.Asset),
                templates.GetModule(ContentType.Asset, Phase.Process),
                new SetDestination()
            };

            PostProcessModules = new ModuleList(templates.GetModule(ContentType.Asset, Phase.PostProcess));

            OutputModules = new ModuleList
            {
                new FilterDocuments(Config.FromDocument(WebKeys.ShouldOutput, true)),
                new WriteFiles()
            };
        }
    }
}
