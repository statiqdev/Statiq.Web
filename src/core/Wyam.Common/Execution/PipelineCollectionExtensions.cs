using System.Collections.Generic;
using Wyam.Common.Modules;

namespace Wyam.Common.Execution
{
    public static class PipelineCollectionExtensions
    {
        public static IPipeline Add(this IPipelineCollection pipelines, ModuleList modules) => 
            pipelines.Add(null, modules);

        public static IPipeline Add(this IPipelineCollection pipelines, params IModule[] modules) => 
            pipelines.Add(null, new ModuleList(modules));

        public static IPipeline Add(this IPipelineCollection pipelines, string name, params IModule[] modules) =>
            pipelines.Add(name, new ModuleList(modules));

        public static IPipeline Insert(this IPipelineCollection pipelines, int index, ModuleList modules) =>
            pipelines.Insert(index, null, modules);

        public static IPipeline Insert(this IPipelineCollection pipelines, int index, params IModule[] modules) =>
            pipelines.Insert(index, null, new ModuleList(modules));

        public static IPipeline Insert(this IPipelineCollection pipelines, int index, string name, params IModule[] modules) =>
            pipelines.Insert(index, name, new ModuleList(modules));

        public static IPipeline InsertBefore(this IPipelineCollection pipelines, string target, params IModule[] modules) =>
            InsertBefore(pipelines, target, null, new ModuleList(modules));

        public static IPipeline InsertBefore(this IPipelineCollection pipelines, string target, string name, params IModule[] modules) =>
            InsertBefore(pipelines, target, name, new ModuleList(modules));

        public static IPipeline InsertBefore(this IPipelineCollection pipelines, string target, ModuleList modules) =>
            InsertBefore(pipelines, target, null, modules);

        public static IPipeline InsertBefore(this IPipelineCollection pipelines, string target, string name, ModuleList modules)
        {
            int index = pipelines.IndexOf(target);
            if (index < 0)
            {
                throw new KeyNotFoundException($"Target pipeline {name} was not found");
            }
            return pipelines.Insert(index, name, modules);
        }

        public static IPipeline InsertAfter(this IPipelineCollection pipelines, string target, params IModule[] modules) =>
            InsertAfter(pipelines, target, null, new ModuleList(modules));

        public static IPipeline InsertAfter(this IPipelineCollection pipelines, string target, string name, params IModule[] modules) =>
            InsertAfter(pipelines, target, name, new ModuleList(modules));

        public static IPipeline InsertAfter(this IPipelineCollection pipelines, string target, ModuleList modules) =>
            InsertAfter(pipelines, target, null, modules);

        public static IPipeline InsertAfter(this IPipelineCollection pipelines, string target, string name, ModuleList modules)
        {
            int index = pipelines.IndexOf(target);
            if (index < 0)
            {
                throw new KeyNotFoundException($"Target pipeline {name} was not found");
            }
            return index + 1 < pipelines.Count 
                ? pipelines.Insert(index + 1, name, modules) 
                : pipelines.Add(name, modules);
        }
    }
}