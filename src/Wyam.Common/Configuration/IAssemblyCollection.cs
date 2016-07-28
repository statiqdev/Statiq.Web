using System.Collections.Generic;
using System.Reflection;

namespace Wyam.Common.Configuration
{
    public interface IAssemblyCollection : IReadOnlyCollection<Assembly>
    {
        bool Add(Assembly assembly);
        bool ContainsFullName(string fullName);
        bool TryGetAssembly(string fullName, out Assembly assembly);
    }
}