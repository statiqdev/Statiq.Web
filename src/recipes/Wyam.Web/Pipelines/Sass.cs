using Wyam.Common.Execution;
using Wyam.Common.Modules;
using Wyam.Core.Modules.IO;

namespace Wyam.Web.Pipelines
{
    /// <summary>
    /// Processes any Sass stylesheets and outputs the resulting CSS files.
    /// </summary>
    public class Sass : Pipeline
    {
        /// <summary>
        /// Create the pipeline.
        /// </summary>
        /// <param name="name">The name of this pipeline.</param>
        public Sass(string name)
            : this(name, new[] { "**/{!_,}*.scss" })
        {
        }

        /// <summary>
        /// Create the pipeline.
        /// </summary>
        /// <param name="name">The name of this pipeline.</param>
        /// <param name="patterns">The patterns of Sass files to read.</param>
        public Sass(string name, string[] patterns)
            : base(name, GetModules(patterns))
        {
        }

        private static IModuleList GetModules(string[] patterns) => new ModuleList
        {
            new ReadFiles(patterns),
            new Wyam.Sass.Sass(),
            new WriteFiles()
        };
    }
}