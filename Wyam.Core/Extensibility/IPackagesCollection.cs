using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Core.Extensibility
{
    public interface IPackagesCollection
    {
        // Sets the path where NuGet packages will be downloaded and cached
        string Path { get; set; }

        IRepository AddRepository(string packageSource);
        IPackagesCollection Add(string packageId, string versionSpec = null, bool allowPrereleaseVersions = false, bool allowUnlisted = false);
    }
}
