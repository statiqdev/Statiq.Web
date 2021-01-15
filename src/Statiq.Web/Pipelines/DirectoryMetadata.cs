using System.Collections.Generic;
using Statiq.Common;
using Statiq.Core;

namespace Statiq.Web.Pipelines
{
    public class DirectoryMetadata : Pipeline
    {
        public DirectoryMetadata(Templates templates)
        {
            InputModules = new ModuleList
            {
                new ReadFiles(Config.FromSetting<IEnumerable<string>>(WebKeys.DirectoryMetadataFiles))
            };

            ProcessModules = new ModuleList(templates.GetModule(ContentType.Data, Phase.Process));
        }
    }
}
