using System.Collections.Generic;
using Wyam.Common.Modules;

namespace Wyam.Common.Execution
{
    public interface IPipeline : IModuleList
    {
        string Name { get; }
        bool ProcessDocumentsOnce { get; set; }
        IPipeline WithProcessDocumentsOnce(bool processDocumentsOnce = true);
    }
}
