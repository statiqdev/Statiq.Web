using System.Collections.Generic;
using Statiq.Common;
using Statiq.Core;

namespace Statiq.Web.Pipelines
{
    public class Assets : Pipeline
    {
        public Assets(Templates templates)
        {
            Dependencies.Add(nameof(Inputs));

            ProcessModules = new ModuleList(templates.GetModule(ContentType.Asset, Phase.Process));

            PostProcessModules = new ModuleList(templates.GetModule(ContentType.Asset, Phase.PostProcess));

            OutputModules = new ModuleList
            {
                new FilterDocuments(Config.FromDocument(WebKeys.ShouldOutput, true)),
                new WriteFiles()
            };
        }
    }
}
