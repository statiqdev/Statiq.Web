using System.Collections.Generic;
using System.Reflection;

namespace Wyam.Core.Configuration
{
    public interface IAssemblyCollection : IEnumerable<Assembly>
    {
        bool Add(Assembly assembly);
        bool ContainsFullName(string fullName);
        bool TryGetAssembly(string fullName, out Assembly assembly);
    }
}