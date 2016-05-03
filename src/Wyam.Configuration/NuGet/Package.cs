using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.PackageManagement;
using NuGet.Packaging;
using NuGet.ProjectManagement;
using NuGet.Protocol.Core.Types;
using NuGet.Resolver;
using NuGet.Versioning;
using Wyam.Common.Tracing;
using System.Threading.Tasks;
using NuGet.Packaging.Core;

namespace Wyam.Configuration.NuGet
{
    internal class Package
    {
        private readonly string _packageId;
        private readonly NuGetVersion _version;
        private readonly bool _allowPrereleaseVersions;
        private readonly bool _allowUnlisted;

        public IReadOnlyList<PackageSource> PackageSources { get; }

        // Indicates that only the specified package sources should be used to find the package
        public bool Exclusive { get; }

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
            PackageSources = packageSources?.Select(x => new PackageSource(x)).ToList();
            _version = string.IsNullOrWhiteSpace(version) ? null : NuGetVersion.Parse(version);
            _allowPrereleaseVersions = allowPrereleaseVersions;
            _allowUnlisted = allowUnlisted;
            Exclusive = exclusive;
        }

        public void InstallPackage(PackageInstaller installer, bool updatePackages, SourceRepository localSourceRepository, List<SourceRepository> sourceRepositories)
        {
            string versionString = _version == null ? string.Empty : " " + _version;
            Trace.Verbose($"Installing package {_packageId}{versionString} (with dependencies)");
            ResolutionContext resolutionContext = new ResolutionContext(
                DependencyBehavior.Lowest, _allowPrereleaseVersions, _allowUnlisted, VersionConstraints.None);
            
            // Get the installed version
            NuGetVersion installedVersion = NuGetPackageManager.GetLatestVersionAsync(_packageId, installer.CurrentFramework, resolutionContext,
                localSourceRepository, installer.Logger, CancellationToken.None).Result;

            // Does the installed version match the requested version
            NuGetVersion matchingVersion = installedVersion;
            if (installedVersion != null 
                && (_version == null || installedVersion == _version) 
                && !updatePackages)
            {
                Trace.Verbose($"Package {_packageId}{versionString} is satisfied by version {installedVersion}");
            }
            else if (_version != null)
            {
                matchingVersion = _version;
            }
            else
            {
                // Get the latest version
                matchingVersion = NuGetPackageManager.GetLatestVersionAsync(_packageId, installer.CurrentFramework, resolutionContext,
                    sourceRepositories, installer.Logger, CancellationToken.None).Result;
            }

            // Install the requested version (do even if we're up to date to ensure dependencies are installed)
            installer.PackageManager.InstallPackageAsync(installer.PackageManager.PackagesFolderNuGetProject,
                new PackageIdentity(_packageId, matchingVersion), resolutionContext, installer.ProjectContext, sourceRepositories,
                Array.Empty<SourceRepository>(), CancellationToken.None).Wait();
            Trace.Verbose($"Installed package {_packageId} {matchingVersion}");
        }
    }
}
