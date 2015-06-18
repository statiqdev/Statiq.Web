using System.Collections.Generic;

namespace Wyam.Abstractions
{
    public interface IReadOnlyPipeline : IReadOnlyList<IModule>
    {
        string Name { get; }
    }
}