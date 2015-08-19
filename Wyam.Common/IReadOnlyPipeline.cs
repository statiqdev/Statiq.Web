using System.Collections.Generic;

namespace Wyam.Common
{
    public interface IReadOnlyPipeline : IReadOnlyList<IModule>
    {
        string Name { get; }
        bool ProcessDocumentsOnce { get; }
    }
}