using System;
using NuGet;

namespace Wyam.Core.Extensibility
{
    internal class Package
    {
        private readonly string _packageId;
        private readonly IVersionSpec _versionSpec;
        private readonly bool _allowPrereleaseVersions;
        private readonly bool _allowUnlisted;

        // The version string is either a simple version or an arithmetic range
        // e.g.
        //      1.0         --> 1.0 ≤ x
        //      (,1.0]      --> x ≤ 1.0
        //      (,1.0)      --> x &lt; 1.0
        //      [1.0]       --> x == 1.0
        //      (1.0,)      --> 1.0 &lt; x
        //      (1.0, 2.0)   --> 1.0 &lt; x &lt; 2.0
        //      [1.0, 2.0]   --> 1.0 ≤ x ≤ 2.0
        public Package(string packageId, string versionSpec = null, bool allowPrereleaseVersions = false, bool allowUnlisted = false)
        {
            if (packageId == null)
            {
                throw new ArgumentNullException("packageId");
            }
            if (string.IsNullOrWhiteSpace(packageId))
            {
                throw new ArgumentException("packageId");
            }

            _packageId = packageId;
            _versionSpec = string.IsNullOrWhiteSpace(versionSpec) ? null : VersionUtility.ParseVersionSpec(versionSpec);
            _allowPrereleaseVersions = allowPrereleaseVersions;
            _allowUnlisted = allowUnlisted;
        }

        public void InstallPackage(PackageManager packageManager)
        {
            IPackage package = packageManager.SourceRepository
                .FindPackage(_packageId, _versionSpec, _allowPrereleaseVersions, _allowUnlisted);
            packageManager.InstallPackage(package, false, _allowPrereleaseVersions);
        }
    }
}
