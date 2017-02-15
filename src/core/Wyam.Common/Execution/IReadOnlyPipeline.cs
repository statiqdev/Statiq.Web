using System.Collections.Generic;
using Wyam.Common.Modules;

namespace Wyam.Common.Execution
{
    public interface IReadOnlyPipeline : IReadOnlyModuleList
    {
        string Name { get; }
        bool ProcessDocumentsOnce { get; }
    }
}