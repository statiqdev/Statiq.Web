using Statiq.Common;
using Statiq.Core;

namespace Statiq.Web.Pipelines
{
    public class Sitemap : Pipeline
    {
        public Sitemap()
        {
            Dependencies.AddRange(
                nameof(Content),
                nameof(Data),
                nameof(Archives));

            PostProcessModules = new ModuleList
            {
                new ExecuteIf(Config.FromSetting<bool>(WebKeys.GenerateSitemap))
                {
                    new ConcatDocuments(nameof(Content)),
                    new ConcatDocuments(nameof(Data))
                    {
                        new FilterDocuments(Config.FromSetting<bool>(WebKeys.OutputData))
                    },
                    new ConcatDocuments(nameof(Archives)),
                    new FlattenTree(),
                    new GenerateSitemap()
                }
            };

            OutputModules = new ModuleList
            {
                new WriteFiles()
            };
        }
    }
}
