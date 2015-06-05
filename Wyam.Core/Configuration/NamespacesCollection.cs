using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Abstractions;

namespace Wyam.Core.Configuration
{
    internal class NamespacesCollection : INamespacesCollection
    {
        private readonly List<string> _namespaces = new List<string>();

        public INamespacesCollection Using(string ns)
        {
            _namespaces.Add(ns);
            return this;
        }

        public List<string> Namespaces
        {
            get { return _namespaces; }
        }
    }
}
