using System.Collections;
using System.Collections.Generic;
using Wyam.Common.Configuration;

namespace Wyam.Core.Configuration
{
    internal class RawAssemblyCollection : IRawAssemblyCollection
    {
        private readonly List<byte[]> _rawAssemblies = new List<byte[]>();

        public void Add(byte[] rawAssembly)
        {
            if (rawAssembly != null)
            {
                _rawAssemblies.Add(rawAssembly);
            }
        }

        public int Count => _rawAssemblies.Count;

        public IEnumerator<byte[]> GetEnumerator() => _rawAssemblies.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}