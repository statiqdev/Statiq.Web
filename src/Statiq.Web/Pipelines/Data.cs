using System.Collections.Generic;
using System.Linq;
using Statiq.Common;
using Statiq.Core;
using Statiq.Web.Modules;
using Statiq.Yaml;

namespace Statiq.Web.Pipelines
{
    public class Data : Pipeline
    {
        public Data(Templates templates)
        {
            Dependencies.Add(nameof(Inputs));

            ProcessModules = new ModuleList
            {
                // Get inputs
                new ReplaceDocuments(nameof(Inputs)),

                // Concat all documents from externally declared dependencies (exclude explicit dependencies above like "Inputs")
                new ConcatDocuments(Config.FromContext<IEnumerable<IDocument>>(ctx => ctx.Outputs.FromPipelines(ctx.Pipeline.GetAllDependencies(ctx).Except(Dependencies).ToArray()))),

                // Filter to non-archive, non-feed data
                new FilterDocuments(Config.FromDocument(doc => doc.Get<ContentType>(WebKeys.ContentType) == ContentType.Data && !Archives.IsArchive(doc) && !Feeds.IsFeed(doc))),

                // Clear the content so data documents can be safely sent to the content pipeline or rendered with a layout
                new ExecuteIf(Config.FromDocument(WebKeys.ClearContent, true))
                {
                    new SetContent(string.Empty)
                },

                // Set the destination and optimize filenames
                new SetDestination(),
                new ExecuteIf(Config.FromSetting(WebKeys.OptimizeDataFileNames, true))
                {
                    new OptimizeFileName()
                }
            };

            PostProcessModules = new ModuleList(templates.GetModule(ContentType.Data, Phase.PostProcess));

            OutputModules = new ModuleList
            {
                new FilterDocuments(Config.FromDocument<bool>(WebKeys.ShouldOutput)),
                new WriteFiles()
            };
        }
    }
}
