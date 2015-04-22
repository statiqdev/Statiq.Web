using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Core.Extensibility;

namespace Wyam.Core.Configuration
{
    public class PreConfigGlobals
    {
        private readonly Configurator _config;
        private readonly IPackagesCollection _packages;

        internal PreConfigGlobals(Configurator config, IPackagesCollection packages)
        {
            _config = config;
            _packages = packages;
        }

        public string PackagePath
        {
            get { return _config.PackagePath; }
            set { _config.PackagePath = value; }
        }

        public IPackagesCollection Packages
        {
            get { return _packages; }
        }
    }
}
