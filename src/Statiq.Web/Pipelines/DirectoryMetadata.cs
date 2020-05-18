using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using Statiq.App;
using Statiq.Common;
using Statiq.Core;
using Statiq.Html;
using Statiq.Markdown;
using Statiq.Razor;
using Statiq.Web.Modules;
using Statiq.Yaml;

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
