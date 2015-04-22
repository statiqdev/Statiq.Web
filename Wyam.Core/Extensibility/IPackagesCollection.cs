using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Core.Extensibility
{
    public interface IPackagesCollection
    {
        IRepository AddRepository(string packageSource);
        IPackagesCollection AddPackage(string packageId, string versionSpec = null, bool allowPrereleaseVersions = false, bool allowUnlisted = false);
    }
}
