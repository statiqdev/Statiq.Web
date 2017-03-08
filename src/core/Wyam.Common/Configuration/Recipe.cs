using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Util;

namespace Wyam.Common.Configuration
{
    /// <summary>
    /// A utility class that can be used as the base for recipes. It is not necessary to
    /// derive from this class to create a recipe, implementing <see cref="IRecipe"/> is
    /// sufficient. However, this class does provide some helpful functionality such as
    /// using reflection to automatically iterate and add all <see cref="RecipePipeline"/>
    /// static properties.
    /// </summary>
    public abstract class Recipe : IRecipe
    {
        /// <summary>
        /// This will reflect over all static <see cref="RecipePipeline"/> properties in the
        /// derived class and will add their modules as pipelines to the engine. This operation
        /// depends on <see cref="SourceInfoAttribute"/> having been applied to all the properties
        /// in order to ensure proper ordering.
        /// </summary>
        /// <param name="engine">The engine.</param>
        public virtual void Apply(IEngine engine)
        {
            IEnumerable<Pipeline> pipelines = GetType()
                .GetProperties(BindingFlags.Static | BindingFlags.Public)
                .Where(x => typeof(Pipeline).IsAssignableFrom(x.PropertyType))
                .Select(x =>
                {
                    // Check for a source info attribute
                    SourceInfoAttribute sourceInfo = x.GetCustomAttributes(typeof(SourceInfoAttribute), false).SingleOrDefault() as SourceInfoAttribute;
                    if (sourceInfo == null)
                    {
                        throw new Exception($"{nameof(Pipeline)} property {x.Name} in {GetType().Name} requires a {nameof(SourceInfoAttribute)} for correct ordering.");
                    }

                    return new
                    {
                        Property = x,
                        SourceInfo = x.GetCustomAttributes(typeof(SourceInfoAttribute), false).SingleOrDefault() as SourceInfoAttribute
                    };
                })
                .OrderBy(x => x.SourceInfo.FilePath)
                .ThenBy(x => x.SourceInfo.LineNumber)
                .Select(x =>
                {
                    // Validate that the pipeline name matches the property name
                    Pipeline pipeline = (Pipeline)x.Property.GetValue(null, null);
                    if (x.Property.Name != pipeline.Name)
                    {
                        throw new Exception($"{nameof(Pipeline)} property {x.Property.Name} in {GetType().Name} does not match the name of the pipeline.");
                    }
                    return pipeline;
                });
            foreach (Pipeline pipeline in pipelines)
            {
                engine.Pipelines.Add(pipeline);
            }
        }

        /// <inheritdoc/>
        public virtual void Scaffold(IFile configFile, IDirectory inputDirectory)
        {
        }
    }
}
