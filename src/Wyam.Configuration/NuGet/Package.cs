using System;
using System.Collections.Generic;
using System.Linq;
using NuGet.Configuration;
using NuGet.PackageManagement;
using NuGet.ProjectManagement;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using Wyam.Common.Tracing;

namespace Wyam.Configuration.NuGet
{
    internal class Package
    {
        private readonly string _packageId;
        private readonly NuGetVersion _version;
        private readonly bool _allowPrereleaseVersions;
        private readonly bool _allowUnlisted;

        public IReadOnlyList<PackageSource> PackageSources { get; }
        public bool Exclusive { get; }

        // The version string is either a simple version or an arithmetic range
        // e.g.
        //      1.0         --> 1.0 ≤ x
        //      (,1.0]      --> x ≤ 1.0
        //      (,1.0)      --> x &lt; 1.0
        //      [1.0]       --> x == 1.0
        //      (1.0,)      --> 1.0 &lt; x
        //      (1.0, 2.0)   --> 1.0 &lt; x &lt; 2.0
        //      [1.0, 2.0]   --> 1.0 ≤ x ≤ 2.0
        public Package(string packageId, IReadOnlyList<string> packageSources, string version, 
            bool allowPrereleaseVersions, bool allowUnlisted, bool exclusive)
        {
            if (packageId == null)
            {
                throw new ArgumentNullException(nameof(packageId));
            }
            if (string.IsNullOrWhiteSpace(packageId))
            {
                throw new ArgumentException(nameof(packageId));
            }

            _packageId = packageId;
            PackageSources = packageSources.Select(x => new PackageSource(x)).ToList();
            _version = string.IsNullOrWhiteSpace(version) ? null : NuGetVersion.Parse(version);
            _allowPrereleaseVersions = allowPrereleaseVersions;
            _allowUnlisted = allowUnlisted;
            Exclusive = exclusive;
        }

        public void InstallPackage(PackageInstaller installer, IEnumerable<SourceRepository> sourceRepositories)
        {
            
        }

        /*
        public IPackage InstallPackage(PackageManager packageManager, bool updatePackages)
        {
            string versionSpec = _versionSpec == null ? string.Empty : " " + _versionSpec;
            using (Trace.WithIndent().Verbose($"Installing package {_packageId}{versionSpec} (and dependencies)"))
            {
                // Find the local package
                IPackage localPackage = packageManager.LocalRepository.FindPackage(_packageId);

                // Check if we're up to date
                if (localPackage != null && _versionSpec.Satisfies(localPackage.Version) && !updatePackages)
                {
                    Trace.Verbose($"Package {_packageId}{versionSpec} is satisfied by version {localPackage.Version}, skipping");
                    return localPackage;
                }

                // Find the source package
                IPackage sourcePackage = packageManager.SourceRepository
                    .FindPackage(_packageId, _versionSpec, _allowPrereleaseVersions, _allowUnlisted);
                if (sourcePackage == null)
                {
                    Trace.Critical($"Package {_packageId}{versionSpec} could not be found");
                    return null;
                }

                // Check if we're up to date
                if (localPackage != null && localPackage.Version >= sourcePackage.Version)
                {
                    Trace.Verbose($"Package {_packageId}{versionSpec} is up to date with version {localPackage.Version}, skipping");
                    return localPackage;
                }

                // Uninstall the old package
                if (localPackage != null)
                {
                    packageManager.UninstallPackage(localPackage, true);
                    Trace.Verbose($"Uninstalled package {localPackage.Id} {localPackage.Version}");
                }
            
                // Install it
                packageManager.InstallPackage(sourcePackage, false, _allowPrereleaseVersions);
                Trace.Verbose($"Installed package {sourcePackage.Id} {sourcePackage.Version}");
                return sourcePackage;
            }
        }
        */
    }
}
