using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common;
using Wyam.Common.Configuration;
using Wyam.Common.IO;
using Wyam.Common.Modules;
using Wyam.Core.Execution;

namespace Wyam.Core.Configuration
{
    internal class AssemblyCollection : IAssemblyCollection
    {
        private readonly ConcurrentDictionary<string, Assembly> _assemblies = new ConcurrentDictionary<string, Assembly>();

        public AssemblyCollection()
        {
            // This is the default set of assemblies that should get loaded during configuration and in other dynamic modules
            Add(Assembly.GetAssembly(typeof(object))); // System
            Add(Assembly.GetAssembly(typeof(System.Collections.Generic.List<>))); // System.Collections.Generic 
            Add(Assembly.GetAssembly(typeof(System.Linq.ImmutableArrayExtensions))); // System.Linq
            Add(Assembly.GetAssembly(typeof(System.Dynamic.DynamicObject))); // System.Core (needed for dynamic)
            Add(Assembly.GetAssembly(typeof(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo))); // Microsoft.CSharp (needed for dynamic)
            Add(Assembly.GetAssembly(typeof(System.IO.Stream))); // System.IO
            Add(Assembly.GetAssembly(typeof(System.Diagnostics.TraceSource))); // System.Diagnostics
            Add(Assembly.GetAssembly(typeof(Engine))); // Wyam.Core
            Add(Assembly.GetAssembly(typeof(IModule))); // Wyam.Common
        }

        public bool Add(Assembly assembly) => _assemblies.TryAdd(assembly.FullName, assembly);

        public bool ContainsFullName(string fullName) => _assemblies.ContainsKey(fullName);

        public bool TryGetAssembly(string fullName, out Assembly assembly)
            => _assemblies.TryGetValue(fullName, out assembly);

        public int Count => _assemblies.Count;

        public IEnumerator<Assembly> GetEnumerator() => _assemblies.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
