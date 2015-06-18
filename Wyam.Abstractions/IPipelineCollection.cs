using System.Collections.Generic;

namespace Wyam.Abstractions
{
    public interface IPipelineCollection : IReadOnlyDictionary<string, IPipeline>
    {
        IPipeline Add(params IModule[] modules);
        IPipeline Add(string name, params IModule[] modules);
    }
}
