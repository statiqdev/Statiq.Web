using System.Collections.Generic;
using Wyam.Common.Modules;

namespace Wyam.Common.Execution
{
    public interface IReadOnlyPipeline : IReadOnlyModuleCollection
    {
        string Name { get; }
        bool ProcessDocumentsOnce { get; }
    }
}