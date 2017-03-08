using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Modules;

namespace Wyam.Common.Configuration
{
    /// <summary>
    /// A utility base class for encapsulating modules for a specific pipeline in a recipe. The
    /// primary benefit of this class is that it implements a standard
    /// method for getting modules that can be used by <see cref="Recipe"/>. Instances of this class
    /// are typically assigned to read-only properties in a <see cref="RecipePipeline"/> class and by
    /// convention the class name should match the property name (an exception will be thrown during
    /// recipe application if not).
    /// </summary>
    public abstract class RecipePipeline
    {
        /// <summary>
        /// Gets the modules that comprise the pipeline.
        /// </summary>
        /// <returns>The modules that comprise the pipeline.</returns>
        public abstract ModuleList GetModules();

        /// <summary>
        /// Gets the name of the pipeline. If not overridden, this is the same as
        /// the class name.
        /// </summary>
        public virtual string Name => GetType().Name;

        /// <summary>
        /// Provides the name for the pipeline when converted to a string.
        /// </summary>
        /// <param name="pipeline">The current instance.</param>
        public static implicit operator string(RecipePipeline pipeline) => pipeline?.Name;

        /// <inheritdoc />
        public override string ToString() => Name;

        /// <inheritdoc />
        public override int GetHashCode() => Name.GetHashCode();
    }
}
