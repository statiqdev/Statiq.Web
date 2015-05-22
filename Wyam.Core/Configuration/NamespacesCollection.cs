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
        private readonly List<string> _ns = new List<string>();

        public INamespacesCollection Add(string ns)
        {
            _ns.Add(ns);
            return this;
        }

        public List<string> Ns
        {
            get { return _ns; }
        }
    }
}
