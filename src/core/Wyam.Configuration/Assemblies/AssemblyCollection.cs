using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using Wyam.Common.Modules;
using Wyam.Core.Execution;

namespace Wyam.Configuration.Assemblies
{
    internal class AssemblyCollection : IReadOnlyCollection<Assembly>
    {
        private readonly ConcurrentDictionary<string, Assembly> _assemblies = new ConcurrentDictionary<string, Assembly>();

        public bool Add(Assembly assembly) => _assemblies.TryAdd(assembly.FullName, assembly);

        public bool ContainsFullName(string fullName) => _assemblies.ContainsKey(fullName);

        public bool TryGetAssembly(string fullName, out Assembly assembly)
            => _assemblies.TryGetValue(fullName, out assembly);

        public int Count => _assemblies.Count;

        public IEnumerator<Assembly> GetEnumerator() => _assemblies.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}