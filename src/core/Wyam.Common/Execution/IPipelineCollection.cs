using System.Collections.Generic;
using Wyam.Common.Modules;

namespace Wyam.Common.Execution
{
    /// <summary>
    /// A collection of pipelines.
    /// </summary>
    public interface IPipelineCollection : IReadOnlyDictionary<string, IPipeline>, IReadOnlyList<IPipeline>
    {
        /// <summary>
        /// Adds a new named pipeline to the collection.
        /// </summary>
        /// <param name="name">The name of the pipeline to add.</param>
        /// <param name="modules">The modules the pipeline should contain.</param>
        /// <returns>The added pipeline.</returns>
        IPipeline Add(string name, IModuleList modules);

        /// <summary>
        /// Adds an existing pipeline to the collection.
        /// </summary>
        /// <param name="pipeline">The pipeline to add.</param>
        /// <returns>The added pipeline.</returns>
        IPipeline Add(IPipeline pipeline);

        /// <summary>
        /// Inserts a new pipeline into the collection.
        /// </summary>
        /// <param name="index">The index at which to insert the new pipeline.</param>
        /// <param name="name">The name of the pipeline to insert.</param>
        /// <param name="modules">The modules the pipeline should contain.</param>
        /// <returns>The inserted pipeline.</returns>
        IPipeline Insert(int index, string name, IModuleList modules);

        /// <summary>
        /// Inserts an existing pipeline into the collection.
        /// </summary>
        /// <param name="index">The index at which to insert the pipeline.</param>
        /// <param name="pipeline">The pipeline to insert.</param>
        /// <returns>The inserted pipeline.</returns>
        IPipeline Insert(int index, IPipeline pipeline);

        /// <summary>
        /// Removes a pipeline from the collection by name.
        /// </summary>
        /// <param name="name">The name of the pipeline to remove.</param>
        /// <returns><c>true</c> if the pipeline was found and remove, otherwise <c>false</c>.</returns>
        bool Remove(string name);

        /// <summary>
        /// Removes a pipeline from the collection by index.
        /// </summary>
        /// <param name="index">The index of the pipeline to remove.</param>
        void RemoveAt(int index);

        /// <summary>
        /// Gets the index of a named pipeline.
        /// </summary>
        /// <param name="name">The name of the pipeline.</param>
        /// <returns>The index of the pipeline.</returns>
        int IndexOf(string name);

        /// <summary>
        /// Gets the number of pipeline in the collection.
        /// </summary>
        new int Count { get; }

        /// <inheritdoc />
        new IEnumerator<IPipeline> GetEnumerator();
    }
}
