using System;
using System.Linq;
using Statiq.Common;
using Statiq.Core;
using Statiq.Html;

namespace Statiq.Web.Pipelines
{
    public class Redirects : Pipeline
    {
        public Redirects()
        {
            Dependencies.Add(nameof(Content));

            ProcessModules = new ModuleList
            {
                new ReplaceDocuments(nameof(Content)),
                new ExecuteConfig(Config.FromSettings(settings =>
                {
                    GenerateRedirects generateRedirects = new GenerateRedirects()
                        .WithMetaRefreshPages(settings.GetBool(WebKeys.MetaRefreshRedirects, true));
                    if (settings.GetBool(WebKeys.NetlifyRedirects, false))
                    {
                        generateRedirects = generateRedirects.WithAdditionalOutput(
                            "_redirects",
                            redirects => string.Join(Environment.NewLine, redirects.Select(r => $"/{r.Key} {r.Value}")));
                    }
                    return generateRedirects;
                }))
            };

            OutputModules = new ModuleList
            {
                new WriteFiles()
            };
        }
    }
}
