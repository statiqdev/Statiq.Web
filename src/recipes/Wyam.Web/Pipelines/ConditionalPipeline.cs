using System.Linq;
using Wyam.Common.Configuration;
using Wyam.Common.Execution;
using Wyam.Common.Modules;
using Wyam.Core.Modules.Control;

namespace Wyam.Web.Pipelines
{
    /// <summary>
    /// A <see cref="Pipeline"/> that wraps another pipeline and only executes it
    /// if a condition is met.
    /// </summary>
    public class ConditionalPipeline : Pipeline
    {
        /// <summary>
        /// Create the pipeline.
        /// </summary>
        /// <param name="condition">A delegate that specifies the condition.</param>
        /// <param name="pipeline">The pipeline to wrap.</param>
        public ConditionalPipeline(ContextConfig condition, Pipeline pipeline)
            : base(pipeline.Name, GetModules(condition, pipeline))
        {
        }

        /// <summary>
        /// Create the pipeline.
        /// </summary>
        /// <param name="name">The name of this pipeline.</param>
        /// <param name="condition">A delegate that specifies the condition.</param>
        /// <param name="pipeline">The pipeline to wrap.</param>
        public ConditionalPipeline(string name, ContextConfig condition, Pipeline pipeline)
            : base(name, GetModules(condition, pipeline))
        {
        }

        private static IModuleList GetModules(ContextConfig condition, Pipeline pipeline) => new ModuleList
        {
            new If(condition, pipeline.ToArray())
        };
    }
}