using System.Collections.Generic;
using Wyam.Common.Modules;

namespace Wyam.Common.Execution
{
    public interface IPipeline : IModuleCollection
    {
        string Name { get; }
        bool ProcessDocumentsOnce { get; }
    }
}
