using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Execution;
using Wyam.Common.Modules;
using Wyam.Core.Modules.IO;

namespace Wyam.WebRecipe.Pipelines
{
    /// <summary>
    /// Copies all other resources to the output path.
    /// </summary>
    public class Resources : Pipeline
    {
        /// <summary>
        /// Creates the pipeline.
        /// </summary>
        /// <param name="name">The name of this pipeline.</param>
        public Resources(string name)
            : base(name, GetModules())
        {
        }

        private static IModuleList GetModules() => new ModuleList
        {
            new CopyFiles("**/*{!.cshtml,!.md,!.less,}")
        };
    }
}
