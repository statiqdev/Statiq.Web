using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Execution;
using Wyam.Common.Modules;
using Wyam.Core.Modules.IO;

namespace Wyam.Web.Pipelines
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
            : this(name, new[] { "**/{!.git,}/**/*{!.cshtml,!.md,!.less,!.scss,}" })
        {
        }

        /// <summary>
        /// Creates the pipeline.
        /// </summary>
        /// <param name="name">The name of this pipeline.</param>
        /// <param name="patterns">The patterns of files to copy.</param>
        public Resources(string name, string[] patterns)
            : base(name, GetModules(patterns))
        {
        }

        private static IModuleList GetModules(string[] patterns) => new ModuleList
        {
            new CopyFiles(patterns)
        };
    }
}
