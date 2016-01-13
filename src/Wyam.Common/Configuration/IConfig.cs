using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.NuGet;

namespace Wyam.Common.Configuration
{
    public interface IConfig
    {
        IAssemblyCollection Assemblies { get; }
        IPackagesCollection Packages { get; }
    }
}
