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
        private readonly Engine _engine;
        private readonly IPackagesCollection _packages;
        private readonly IAssemblyCollection _assemblies;

        internal SetupGlobals(Engine engine, IPackagesCollection packages, IAssemblyCollection assemblies)
        {
            _engine = engine;
            _packages = packages;
            _assemblies = assemblies;
        }

        public string RootFolder
        {
            set { _engine.RootFolder = value; }
        }

        public string InputFolder
        {
            set { _engine.InputFolder = value; }
        }

        public string OutputFolder
        {
            set { _engine.OutputFolder = value; }
        }

        public IPackagesCollection Packages
        {
            get { return _packages; }
        }

        public IAssemblyCollection Assemblies
        {
            get { return _assemblies; }
        }
    }
}
