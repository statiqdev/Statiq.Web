using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Wyam.Razor
{
    internal class DynamicAssemblyCollection : IEnumerable<byte[]>
    {
        private readonly IReadOnlyCollection<byte[]> _assemblies;

        public DynamicAssemblyCollection(IEnumerable<byte[]> assemblies)
        {
            _assemblies = assemblies.ToArray();
        }

        public IEnumerator<byte[]> GetEnumerator()
        {
            IEnumerable<byte[]> assemblies = _assemblies;
            return assemblies.GetEnumerator();
        }

        public override int GetHashCode()
        {
            return _assemblies.Count;
        }

        public override bool Equals(object obj)
        {
            var other = obj as DynamicAssemblyCollection;
            return other != null && _assemblies.SequenceEqual(other._assemblies);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}