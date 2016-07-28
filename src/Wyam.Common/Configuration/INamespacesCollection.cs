using System.Collections.Generic;

namespace Wyam.Common.Configuration
{
    public interface INamespacesCollection : IReadOnlyCollection<string>
    {
        bool Add(string ns);
        void AddRange(IEnumerable<string> namespaces);
    }
}