using System;
using NuGet;
using Wyam.Common.Tracing;

namespace Wyam.NuGet
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

        public IPackage InstallPackage(PackageManager packageManager, bool updatePackages)
        {
            using (Trace.WithIndent().Verbose("Installing package {0}{1} from {2}",
                _packageId, _versionSpec == null ? string.Empty : " " + _versionSpec, packageManager.SourceRepository.Source))
            {
                // Find the local package
                IPackage localPackage = packageManager.LocalRepository.FindPackage(_packageId);

                // Check if we're up to date
                if (localPackage != null && _versionSpec.Satisfies(localPackage.Version) && !updatePackages)
                {
                    Trace.Verbose("Package {0}{1} is satisfied by version {2}, skipping",
                        _packageId, _versionSpec == null ? string.Empty : " " + _versionSpec, localPackage.Version);
                    return localPackage;
                }

                // Find the source package
                IPackage sourcePackage = packageManager.SourceRepository
                    .FindPackage(_packageId, _versionSpec, _allowPrereleaseVersions, _allowUnlisted);
                if (sourcePackage == null)
                {
                    Trace.Warning("Package {0} {1} could not be found at {2}",
                        _packageId, _versionSpec == null ? string.Empty : " " + _versionSpec, packageManager.SourceRepository.Source);
                    return null;
                }

                // Check if we're up to date
                if (localPackage != null && localPackage.Version >= sourcePackage.Version)
                {
                    Trace.Verbose("Package {0}{1} is up to date with version {2}, skipping",
                        _packageId, _versionSpec == null ? string.Empty : " " + _versionSpec, localPackage.Version);
                    return localPackage;
                }

                // Uninstall the old package
                if (localPackage != null)
                {
                    packageManager.UninstallPackage(localPackage, true);
                    Trace.Verbose("Uninstalled package {0} {1}", localPackage.Id, localPackage.Version);
                }
            
                // Install it
                packageManager.InstallPackage(sourcePackage, false, _allowPrereleaseVersions);
                Trace.Verbose("Installed package {0} {1}", sourcePackage.Id, sourcePackage.Version);
                return sourcePackage;
            }
        }
    }
}
