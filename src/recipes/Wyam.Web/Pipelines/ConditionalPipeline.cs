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
        /// An <see cref="If"/> module that checks the condition.
        /// The other modules in the pipeline are all children of this module.
        /// </summary>
        public const string Condition = nameof(Condition);

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
            {
                Condition,
                new If(condition, pipeline.ToArray())
            }
        };
    }
}