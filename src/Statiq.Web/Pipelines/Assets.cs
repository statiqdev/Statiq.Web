using System.Collections.Generic;
using System.Linq;
using Statiq.Common;
using Statiq.Core;

namespace Statiq.Web.Pipelines
{
    public class Assets : Pipeline
    {
        public Assets(Templates templates)
        {
            Dependencies.Add(nameof(Inputs));

            ProcessModules = new ModuleList
            {
                // Get inputs
                new ReplaceDocuments(nameof(Inputs)),

                // Concat all documents from externally declared dependencies (exclude explicit dependencies above like "Inputs")
                new ConcatDocuments(Config.FromContext<IEnumerable<IDocument>>(ctx => ctx.Outputs.FromPipelines(ctx.Pipeline.GetAllDependencies(ctx).Except(Dependencies).ToArray()))),

                // Filter to non-archive, non-feed data
                new FilterDocuments(Config.FromDocument(doc => doc.Get<ContentType>(WebKeys.ContentType) == ContentType.Asset)),

                // Execute asset templates
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
