using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Wyam.Razor
{
    internal class NamespaceCollection : IEnumerable<string>
    {
        private readonly string[] _namespaces;

        public NamespaceCollection(IEnumerable<string> namespaces)
        {
            _namespaces = namespaces.ToArray();
        }

        public IEnumerator<string> GetEnumerator()
        {
            IEnumerable<string> namespaces = _namespaces;
            return namespaces.GetEnumerator();
        }

        public override int GetHashCode()
        {
            return _namespaces.Length;
        }

        public override bool Equals(object obj)
        {
            var other = obj as NamespaceCollection;
            return other != null && _namespaces.SequenceEqual(other._namespaces);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}