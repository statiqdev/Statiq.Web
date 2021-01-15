using Statiq.Common;
using Statiq.Core;
using Statiq.Web.Modules;

namespace Statiq.Web.Pipelines
{
    public class Data : Pipeline
    {
        public Data(Templates templates)
        {
            Dependencies.Add(nameof(Inputs));

            ProcessModules = new ModuleList
            {
                // We ran the data templates against documents in Inputs, but haven't run it against documents from dependencies
                new GetPipelineDocuments(ContentType.Data, templates.GetModule(ContentType.Data, Phase.Process)),

                // Filter to non-archive, non-feed data
                new FilterDocuments(Config.FromDocument(doc => !Archives.IsArchive(doc) && !Feeds.IsFeed(doc))),

                // Clear the content so data documents can be safely sent to the content pipeline or rendered with a layout
                new ExecuteIf(Config.FromDocument<bool>(WebKeys.ClearDataContent))
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
