using System.Collections.Generic;

namespace Wyam.Common
{
    public interface IPipelineCollection : IReadOnlyDictionary<string, IPipeline>
    {
        IPipeline Add(params IModule[] modules);
        IPipeline Add(string name, params IModule[] modules);
    }
}
