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
    public interface IModuleCollection : IList<IModule>, IReadOnlyDictionary<string, IModule>
    {
        void Add(string name, IModule module);
        void Insert(int index, string name, IModule module);
        void Add(params IModule[] modules);
        void Insert(int index, params IModule[] modules);
        int IndexOf(string name);
        new int Count { get; }
        new IEnumerator<IModule> GetEnumerator();
    }
}
