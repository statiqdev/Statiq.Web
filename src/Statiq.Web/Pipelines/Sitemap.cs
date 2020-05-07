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
                    new ConcatDocuments(nameof(Content))
                    {
                        new FilterDocuments(Config.FromDocument(WebKeys.ShouldOutput, true))
                    },
                    new ConcatDocuments(nameof(Data))
                    {
                        new FilterDocuments(Config.FromDocument<bool>(WebKeys.ShouldOutput))
                    },
                    new ConcatDocuments(nameof(Archives)),
                    new FlattenTree(),
                    new FilterDocuments(Config.FromDocument(doc => !doc.GetBool(Keys.TreePlaceholder))),
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
