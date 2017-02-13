using System.Collections.Generic;

namespace Wyam.Common.Modules
{
    /// <summary>
    /// A collection of optionally named modules.
    /// </summary>
    public interface IReadOnlyModuleCollection : IReadOnlyList<IModule>, IReadOnlyDictionary<string, IModule>
    {
        int IndexOf(string name);
        new int Count { get; }
        new IEnumerator<IModule> GetEnumerator();
    }
}