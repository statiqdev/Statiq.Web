using System.Collections.Generic;
using Wyam.Common.Modules;

namespace Wyam.Common.Pipelines
{
    public interface IReadOnlyPipeline : IReadOnlyList<IModule>
    {
        string Name { get; }
        bool ProcessDocumentsOnce { get; }
    }
}