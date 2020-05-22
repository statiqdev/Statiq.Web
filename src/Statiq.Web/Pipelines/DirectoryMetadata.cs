using System.Collections.Generic;
using Statiq.Common;
using Statiq.Core;
using Statiq.Web.Modules;

namespace Statiq.Web.Pipelines
{
    public class DirectoryMetadata : Pipeline
    {
        public DirectoryMetadata()
        {
            InputModules = new ModuleList
            {
                new ReadFiles(Config.FromSetting<IEnumerable<string>>(WebKeys.DirectoryMetadataFiles))
            };

            ProcessModules = new ModuleList
            {
                new ParseDataContent()
            };
        }
    }
}
