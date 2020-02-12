using System;
using System.Collections.Generic;
using System.Text;
using Statiq.App;
using Statiq.Common;
using Statiq.Core;
using Statiq.Html;
using Statiq.Less;
using Statiq.Markdown;
using Statiq.Razor;
using Statiq.Sass;
using Statiq.Yaml;

namespace Statiq.Web.Pipelines
{
    public class Sass : Pipeline
    {
        public Sass()
        {
            Isolated = true;

            InputModules = new ModuleList
            {
                new ReadFiles("**/{!_,}*.scss")
            };

            ProcessModules = new ModuleList
            {
                new CacheDocuments
                {
                    new CompileSass().WithCompactOutputStyle()
                }
            };

            OutputModules = new ModuleList
            {
                new WriteFiles()
            };
        }
    }
}
