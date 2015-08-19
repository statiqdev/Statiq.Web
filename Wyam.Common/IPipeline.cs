using System.Collections.Generic;

namespace Wyam.Common
{
    public interface IPipeline : IList<IModule>
    {
        string Name { get; }
        bool ProcessDocumentsOnce { get; }
        void Add(params IModule[] items);
        void Insert(int index, params IModule[] items);
    }
}
