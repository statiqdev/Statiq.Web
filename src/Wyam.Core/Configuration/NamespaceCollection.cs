using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Configuration;
using Wyam.Common.Util;

namespace Wyam.Core.Configuration
{
    internal class NamespaceCollection : INamespacesCollection
    {
        private readonly ConcurrentHashSet<string> _namespaces = new ConcurrentHashSet<string>();

        public NamespaceCollection()
        {
            // This is the default set of namespaces that should brought in scope during configuration and in other dynamic modules
            _namespaces.AddRange(new []
            {
                "System",
                "System.Collections.Generic",
                "System.Linq",
                "Wyam.Core.Execution",
                "Wyam.Core.Configuration",
                "Wyam.Core.Documents"  // For custom document type support
            });
        }

        public bool Add(string ns) => _namespaces.Add(ns);

        public void AddRange(IEnumerable<string> namespaces) => _namespaces.AddRange(namespaces);

        public int Count => _namespaces.Count;

        public IEnumerator<string> GetEnumerator() => _namespaces.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
