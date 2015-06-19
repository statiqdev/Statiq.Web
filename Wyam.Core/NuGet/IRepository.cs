using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Core.NuGet
{
    public interface IRepository
    {
        IRepository Install(string packageId, string versionSpec = null, bool allowPrereleaseVersions = false, bool allowUnlisted = false);
        IRepository Install(string packageId, bool allowPrereleaseVersions = false, bool allowUnlisted = false);
    }
}
