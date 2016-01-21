using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.NuGet;
using Wyam.Common.Pipelines;
using Wyam.Common.Tracing;

namespace Wyam.Common.Configuration
{
    public interface IConfig
    {
        IAssemblyCollection Assemblies { get; }
        IPackagesCollection Packages { get; }
    }
}
