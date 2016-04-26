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

namespace Wyam.Configuration.NuGet
{
    internal class Package
    {
        private readonly string _packageId;
        private readonly VersionRange _versionRange;
        private readonly bool _allowPrereleaseVersions;
        private readonly bool _allowUnlisted;

        public IReadOnlyList<PackageSource> PackageSources { get; }

        // Indicates that only the specified package sources should be used to find the package
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
        public Package(string packageId, IReadOnlyList<string> packageSources, string versionRange, 
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
            _versionRange = string.IsNullOrWhiteSpace(versionRange) ? null : VersionRange.Parse(versionRange);
            _allowPrereleaseVersions = allowPrereleaseVersions;
            _allowUnlisted = allowUnlisted;
            Exclusive = exclusive;
        }

        public void InstallPackage(PackageInstaller installer, List<PackageReference> installedPackages, bool updatePackages, List<SourceRepository> sourceRepositories)
        {
            string versionRange = _versionRange == null ? string.Empty : " " + _versionRange;
            Trace.Verbose($"Installing package {_packageId}{versionRange} (with dependencies)");
            ResolutionContext resolutionContext = new ResolutionContext(
                DependencyBehavior.Highest, _allowPrereleaseVersions, _allowUnlisted, VersionConstraints.None);
            PackageReference installedPackage =
                installedPackages.FirstOrDefault(x => x.PackageIdentity.Id == _packageId);

            // Check if we're up to date
            if (installedPackage != null && _versionRange != null 
                && _versionRange.Satisfies(installedPackage.PackageIdentity.Version) && !updatePackages)
            {
                Trace.Verbose($"Package {_packageId}{versionRange} is satisfied by version {installedPackage.PackageIdentity.Version}, skipping");
                return;
            }

            // Find the source package
            NuGetVersion latestVersion = NuGetPackageManager.GetLatestVersionAsync(_packageId, installer.CurrentFramework, resolutionContext,
                sourceRepositories, installer.Logger, CancellationToken.None).Result;
            if (latestVersion == null)
            {
                Trace.Critical($"Package {_packageId}{versionRange} could not be found");
                return;
            }

            // Check if we're up to date
            if (installedPackage != null && installedPackage.PackageIdentity.Version >= latestVersion)
            {
                Trace.Verbose($"Package {_packageId}{versionRange} is up to date with version {installedPackage.PackageIdentity.Version} (latest is {latestVersion}, skipping");
                return;
            }

            // Uninstall the old package
            if (installedPackage != null)
            {
                installer.PackageManager.UninstallPackageAsync(installer.PackageManager.PackagesFolderNuGetProject, _packageId, 
                    new UninstallationContext(), installer.ProjectContext, CancellationToken.None).Wait();
                Trace.Verbose($"Uninstalled package {_packageId} {installedPackage.PackageIdentity.Version}");
            }

            // Install the new package
            installer.PackageManager.InstallPackageAsync(installer.PackageManager.PackagesFolderNuGetProject,
                _packageId, resolutionContext, installer.ProjectContext, sourceRepositories,
                Array.Empty<SourceRepository>(), CancellationToken.None).Wait();
            Trace.Verbose($"Installed package {_packageId} {latestVersion}");
        }

        private void Foo(Func<Task> func)
        {
            Task task = Task.Run(async () => { await func(); });
            task.Wait();
        }
    }
}
