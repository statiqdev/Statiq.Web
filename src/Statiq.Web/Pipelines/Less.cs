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
using Statiq.Yaml;

namespace Statiq.Web.Pipelines
{
    public class Less : Pipeline
    {
        public Less()
        {
            Isolated = true;

            InputModules = new ModuleList
            {
                new ReadFiles("**/{!_,}*.less")
            };

            ProcessModules = new ModuleList
            {
                new CacheDocuments
                {
                    new CompileLess()
                }
            };

            OutputModules = new ModuleList
            {
                new WriteFiles()
            };
        }
    }
}
