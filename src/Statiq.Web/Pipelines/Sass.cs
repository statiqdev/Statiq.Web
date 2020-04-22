using Statiq.Common;
using Statiq.Core;
using Statiq.Sass;

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
