using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Core.Extensibility
{
    public interface IRepository
    {
        IRepository AddPackage(string packageId, string versionSpec = null, bool allowPrereleaseVersions = false, bool allowUnlisted = false);
    }
}
