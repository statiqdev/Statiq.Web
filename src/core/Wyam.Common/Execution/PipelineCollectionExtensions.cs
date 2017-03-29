using System.Collections.Generic;
using Wyam.Common.Modules;

namespace Wyam.Common.Execution
{
    /// <summary>
    /// Extensions for working with pipeline collections.
    /// </summary>
    public static class PipelineCollectionExtensions
    {
        /// <summary>
        /// Adds a new unnamed pipeline to the collection.
        /// </summary>
        /// <param name="pipelines">The pipeline collection.</param>
        /// <param name="modules">The modules the new pipeline should contain.</param>
        /// <returns>The newly added pipeline.</returns>
        public static IPipeline Add(this IPipelineCollection pipelines, IModuleList modules) =>
            pipelines.Add(null, modules);

        /// <summary>
        /// Adds a new unnamed pipeline to the collection.
        /// </summary>
        /// <param name="pipelines">The pipeline collection.</param>
        /// <param name="modules">The modules the new pipeline should contain.</param>
        /// <returns>The newly added pipeline.</returns>
        public static IPipeline Add(this IPipelineCollection pipelines, params IModule[] modules) =>
            pipelines.Add(null, new ModuleList(modules));

        /// <summary>
        /// Adds a new named pipeline to the collection.
        /// </summary>
        /// <param name="pipelines">The pipeline collection.</param>
        /// <param name="name">The name of the new pipeline.</param>
        /// <param name="modules">The modules the new pipeline should contain.</param>
        /// <returns>The newly added pipeline.</returns>
        public static IPipeline Add(this IPipelineCollection pipelines, string name, params IModule[] modules) =>
            pipelines.Add(name, new ModuleList(modules));

        /// <summary>
        /// Inserts a new unnamed pipeline into the collection.
        /// </summary>
        /// <param name="pipelines">The pipeline collection.</param>
        /// <param name="index">The index at which to insert the new pipeline.</param>
        /// <param name="modules">The modules the new pipeline should contain.</param>
        /// <returns>The newly inserted pipeline.</returns>
        public static IPipeline Insert(this IPipelineCollection pipelines, int index, IModuleList modules) =>
            pipelines.Insert(index, null, modules);

        /// <summary>
        /// Inserts a new unnamed pipeline into the collection.
        /// </summary>
        /// <param name="pipelines">The pipeline collection.</param>
        /// <param name="index">The index at which to insert the new pipeline.</param>
        /// <param name="modules">The modules the new pipeline should contain.</param>
        /// <returns>The newly inserted pipeline.</returns>
        public static IPipeline Insert(this IPipelineCollection pipelines, int index, params IModule[] modules) =>
            pipelines.Insert(index, null, new ModuleList(modules));

        /// <summary>
        /// Inserts a new named pipeline into the collection.
        /// </summary>
        /// <param name="pipelines">The pipeline collection.</param>
        /// <param name="index">The index at which to insert the new pipeline.</param>
        /// <param name="name">The name of the new pipeline.</param>
        /// <param name="modules">The modules the new pipeline should contain.</param>
        /// <returns>The newly inserted pipeline.</returns>
        public static IPipeline Insert(this IPipelineCollection pipelines, int index, string name, params IModule[] modules) =>
            pipelines.Insert(index, name, new ModuleList(modules));

        /// <summary>
        /// Inserts a new unnamed pipeline before an existing named pipeline.
        /// </summary>
        /// <param name="pipelines">The pipeline collection.</param>
        /// <param name="target">The pipeline before which the new pipeline should be inserted.</param>
        /// <param name="modules">The modules the new pipeline should contain.</param>
        /// <returns>The newly inserted pipeline.</returns>
        public static IPipeline InsertBefore(this IPipelineCollection pipelines, string target, params IModule[] modules) =>
            InsertBefore(pipelines, target, null, new ModuleList(modules));

        /// <summary>
        /// Inserts a new unnamed pipeline before an existing named pipeline.
        /// </summary>
        /// <param name="pipelines">The pipeline collection.</param>
        /// <param name="target">The pipeline before which the new pipeline should be inserted.</param>
        /// <param name="modules">The modules the new pipeline should contain.</param>
        /// <returns>The newly inserted pipeline.</returns>
        public static IPipeline InsertBefore(this IPipelineCollection pipelines, string target, IModuleList modules) =>
            InsertBefore(pipelines, target, null, modules);

        /// <summary>
        /// Inserts a new named pipeline before an existing named pipeline.
        /// </summary>
        /// <param name="pipelines">The pipeline collection.</param>
        /// <param name="target">The pipeline before which the new pipeline should be inserted.</param>
        /// <param name="name">The name of the new pipeline.</param>
        /// <param name="modules">The modules the new pipeline should contain.</param>
        /// <returns>The newly inserted pipeline.</returns>
        public static IPipeline InsertBefore(this IPipelineCollection pipelines, string target, string name, params IModule[] modules) =>
            InsertBefore(pipelines, target, name, new ModuleList(modules));

        /// <summary>
        /// Inserts a new named pipeline before an existing named pipeline.
        /// </summary>
        /// <param name="pipelines">The pipeline collection.</param>
        /// <param name="target">The pipeline before which the new pipeline should be inserted.</param>
        /// <param name="name">The name of the new pipeline.</param>
        /// <param name="modules">The modules the new pipeline should contain.</param>
        /// <returns>The newly inserted pipeline.</returns>
        public static IPipeline InsertBefore(this IPipelineCollection pipelines, string target, string name, IModuleList modules)
        {
            int index = pipelines.IndexOf(target);
            if (index < 0)
            {
                throw new KeyNotFoundException($"Target pipeline {target} was not found");
            }
            return pipelines.Insert(index, name, modules);
        }

        /// <summary>
        /// Inserts an existing pipeline before an existing named pipeline.
        /// </summary>
        /// <param name="pipelines">The pipeline collection.</param>
        /// <param name="target">The pipeline before which the specified pipeline should be inserted.</param>
        /// <param name="pipeline">The pipeline to insert.</param>
        /// <returns>The inserted pipeline.</returns>
        public static IPipeline InsertBefore(this IPipelineCollection pipelines, string target, IPipeline pipeline)
        {
            int index = pipelines.IndexOf(target);
            if (index < 0)
            {
                throw new KeyNotFoundException($"Target pipeline {target} was not found");
            }
            return pipelines.Insert(index, pipeline);
        }

        /// <summary>
        /// Inserts a new unnamed pipeline after an existing named pipeline.
        /// </summary>
        /// <param name="pipelines">The pipeline collection.</param>
        /// <param name="target">The pipeline after which the new pipeline should be inserted.</param>
        /// <param name="modules">The modules the new pipeline should contain.</param>
        /// <returns>The newly inserted pipeline.</returns>
        public static IPipeline InsertAfter(this IPipelineCollection pipelines, string target, params IModule[] modules) =>
            InsertAfter(pipelines, target, null, new ModuleList(modules));

        /// <summary>
        /// Inserts a new unnamed pipeline after an existing named pipeline.
        /// </summary>
        /// <param name="pipelines">The pipeline collection.</param>
        /// <param name="target">The pipeline after which the new pipeline should be inserted.</param>
        /// <param name="modules">The modules the new pipeline should contain.</param>
        /// <returns>The newly inserted pipeline.</returns>
        public static IPipeline InsertAfter(this IPipelineCollection pipelines, string target, IModuleList modules) =>
            InsertAfter(pipelines, target, null, modules);

        /// <summary>
        /// Inserts a new named pipeline after an existing named pipeline.
        /// </summary>
        /// <param name="pipelines">The pipeline collection.</param>
        /// <param name="target">The pipeline after which the new pipeline should be inserted.</param>
        /// <param name="name">The name of the new pipeline.</param>
        /// <param name="modules">The modules the new pipeline should contain.</param>
        /// <returns>The newly inserted pipeline.</returns>
        public static IPipeline InsertAfter(this IPipelineCollection pipelines, string target, string name, params IModule[] modules) =>
            InsertAfter(pipelines, target, name, new ModuleList(modules));

        /// <summary>
        /// Inserts a new named pipeline after an existing named pipeline.
        /// </summary>
        /// <param name="pipelines">The pipeline collection.</param>
        /// <param name="target">The pipeline after which the new pipeline should be inserted.</param>
        /// <param name="name">The name of the new pipeline.</param>
        /// <param name="modules">The modules the new pipeline should contain.</param>
        /// <returns>The newly inserted pipeline.</returns>
        public static IPipeline InsertAfter(this IPipelineCollection pipelines, string target, string name, IModuleList modules)
        {
            int index = pipelines.IndexOf(target);
            if (index < 0)
            {
                throw new KeyNotFoundException($"Target pipeline {target} was not found");
            }
            return index + 1 < pipelines.Count
                ? pipelines.Insert(index + 1, name, modules)
                : pipelines.Add(name, modules);
        }

        /// <summary>
        /// Inserts an existing pipeline after an existing named pipeline.
        /// </summary>
        /// <param name="pipelines">The pipeline collection.</param>
        /// <param name="target">The pipeline after which the specified pipeline should be inserted.</param>
        /// <param name="pipeline">The pipeline to insert.</param>
        /// <returns>The inserted pipeline.</returns>
        public static IPipeline InsertAfter(this IPipelineCollection pipelines, string target, IPipeline pipeline)
        {
            int index = pipelines.IndexOf(target);
            if (index < 0)
            {
                throw new KeyNotFoundException($"Target pipeline {target} was not found");
            }
            return index + 1 < pipelines.Count
                ? pipelines.Insert(index + 1, pipeline)
                : pipelines.Add(pipeline);
        }
    }
}