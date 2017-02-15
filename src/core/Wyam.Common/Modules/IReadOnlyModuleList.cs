using System.Collections.Generic;

namespace Wyam.Common.Modules
{
    /// <summary>
    /// A collection of optionally named modules.
    /// </summary>
    public interface IReadOnlyModuleList : IReadOnlyList<IModule>
    {
        int IndexOf(string name);
        bool Contains(string name);
        bool TryGetValue(string name, out IModule value);
        IModule this[string name] { get; }
        IEnumerable<KeyValuePair<string, IModule>> AsKeyValuePairs();
    }
}