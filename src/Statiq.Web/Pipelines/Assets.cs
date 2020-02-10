using System;
using System.Collections.Generic;
using System.Text;
using Statiq.App;
using Statiq.Common;
using Statiq.Core;
using Statiq.Html;
using Statiq.Markdown;
using Statiq.Razor;
using Statiq.Yaml;

namespace Statiq.Web.Pipelines
{
    public class Assets : Pipeline
    {
        public Assets()
        {
            Isolated = true;

            ProcessModules = new ModuleList
            {
                new CopyFiles("**/*{!.html,!.cshtml,!.md,!.less,!.yml,!.scss,!.config,}")
            };
        }
    }
}
