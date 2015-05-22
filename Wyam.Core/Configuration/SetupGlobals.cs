using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Abstractions;
using Wyam.Core.NuGet;

namespace Wyam.Core.Configuration
{
    public class SetupGlobals
    {
        private readonly IPackagesCollection _packages;
        private readonly IAssemblyCollection _assemblies;
        private readonly INamespacesCollection _namespaces;

        internal SetupGlobals(IPackagesCollection packages, IAssemblyCollection assemblies, INamespacesCollection namespaces)
        {
            _packages = packages;
            _assemblies = assemblies;
            _namespaces = namespaces;
        }

        public IPackagesCollection Packages
        {
            get { return _packages; }
        }

        public IAssemblyCollection Assemblies
        {
            get { return _assemblies; }
        }

        public INamespacesCollection Namespaces
        {
            get { return _namespaces; }
        }
    }
}
