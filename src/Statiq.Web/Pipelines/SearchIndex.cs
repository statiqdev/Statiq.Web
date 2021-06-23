using Statiq.Common;
using Statiq.Core;
using Statiq.Lunr;

namespace Statiq.Web.Pipelines
{
    public class SearchIndex : Pipeline
    {
        public SearchIndex()
        {
            Dependencies.AddRange(nameof(Content));

            PostProcessModules = new ModuleList
            {
                new ExecuteIf(Config.FromSetting(WebKeys.GenerateSearchIndex, false))
                {
                    new ReplaceDocuments(nameof(Content)),
                    new FilterDocuments(Config.FromDocument(WebKeys.ShouldOutput, true)),
                    new ExecuteConfig(Config.FromContext(ctx =>
                        new GenerateLunrIndex(
                            ctx.GetPath(WebKeys.SearchIndexStopwordsPath),
                            ctx.GetBool(WebKeys.SearchIndexEnableStemming))
                            .WithDestination(ctx.GetPath(WebKeys.SearchIndexDestinationPath, GenerateLunrIndex.DefaultDestinationPath))
                            .IncludeHost(ctx.GetBool(WebKeys.SearchIndexIncludeHost))))
                }
            };

            OutputModules = new ModuleList
            {
                new WriteFiles()
            };
        }
    }
}
