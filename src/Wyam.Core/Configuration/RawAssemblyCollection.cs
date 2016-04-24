using System.Collections;
using System.Collections.Generic;

namespace Wyam.Core.Configuration
{
    internal class RawAssemblyCollection : IRawAssemblyCollection
    {
        private readonly List<byte[]> _rawAssemblies = new List<byte[]>();

        public void Add(byte[] rawAssembly)
        {
            if (rawAssembly != null && rawAssembly.Length > 0)
            {
                _rawAssemblies.Add(rawAssembly);
            }
        }

        public IEnumerator<byte[]> GetEnumerator() => _rawAssemblies.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}