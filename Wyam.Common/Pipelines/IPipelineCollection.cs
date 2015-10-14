using System.Collections.Generic;
using Wyam.Common.Modules;

namespace Wyam.Common.Pipelines
{
    public interface IPipelineCollection : IReadOnlyDictionary<string, IPipeline>
    {
        IPipeline Add(params IModule[] modules);
        IPipeline Add(string name, params IModule[] modules);
        IPipeline Add(string name, bool processDocumentsOnce, params IModule[] modules);
        IPipeline Add(bool processDocumentsOnce, params IModule[] modules);
    }
}
