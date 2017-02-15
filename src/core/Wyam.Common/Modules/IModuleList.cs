using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Execution;

namespace Wyam.Common.Modules
{
    /// <summary>
    /// A collection of optionally named modules. Implementations should "unwrap" <see cref="NamedModule"/>
    /// objects to obtain the module name.
    /// </summary>
    public interface IModuleList : IList<IModule>
    {
        void Add(string name, IModule module);
        void Add(params IModule[] modules);
        void Insert(int index, string name, IModule module);
        void Insert(int index, params IModule[] modules);
        bool Remove(string name);
        int IndexOf(string name);
        bool Contains(string name);
        bool TryGetValue(string name, out IModule value);
        IModule this[string name] { get; }
        IEnumerable<KeyValuePair<string, IModule>> AsKeyValuePairs();
    }
}
