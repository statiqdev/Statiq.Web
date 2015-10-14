using System.Collections.Generic;
using Wyam.Common.Modules;

namespace Wyam.Common.Pipelines
{
    public interface IPipeline : IList<IModule>
    {
        string Name { get; }
        bool ProcessDocumentsOnce { get; }
        void Add(params IModule[] items);
        void Insert(int index, params IModule[] items);
    }
}
