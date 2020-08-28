using System.Collections.Generic;
using Statiq.Common;
using Statiq.Core;

namespace Statiq.Web.Pipelines
{
    public class Assets : Pipeline
    {
        public Assets(Templates templates)
        {
            InputModules = new ModuleList
            {
                new ReadFiles(Config.FromSetting<IEnumerable<string>>(WebKeys.AssetFiles))
            };

            ProcessModules = new ModuleList(templates.GetModule(TemplateType.Asset));

            OutputModules = new ModuleList
            {
                new WriteFiles()
            };
        }
    }
}
