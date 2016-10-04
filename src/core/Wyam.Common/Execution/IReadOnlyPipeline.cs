using System.Collections.Generic;
using Wyam.Common.Modules;

namespace Wyam.Common.Execution
{
    public interface IReadOnlyPipeline : IReadOnlyList<IModule>
    {
        string Name { get; }
        bool ProcessDocumentsOnce { get; }
    }
}