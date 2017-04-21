using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Execution;
using Wyam.Common.Modules;
using Wyam.Core.Modules.Control;
using Wyam.Core.Modules.IO;

namespace Wyam.WebRecipe.Pipelines
{
    /// <summary>
    /// Processes any Less stylesheets and outputs the resulting CSS files.
    /// </summary>
    public class Less : Pipeline
    {
        /// <summary>
        /// Create the pipeline.
        /// </summary>
        /// <param name="name">The name of this pipeline.</param>
        /// <param name="patterns">The patterns of Less files to read.</param>
        public Less(string name, string[] patterns)
            : base(name, GetModules(patterns))
        {
        }

        private static IModuleList GetModules(string[] patterns) => new ModuleList
        {
            new ReadFiles(patterns),
            new Wyam.Less.Less(),
            new WriteFiles(".css")
        };
    }
}
