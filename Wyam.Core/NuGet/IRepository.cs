using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Core.NuGet
{
    public interface IRepository
    {
        IRepository Add(string packageId, string versionSpec = null, bool allowPrereleaseVersions = false, bool allowUnlisted = false);
    }
}
