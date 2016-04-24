using System.Collections.Generic;
using System.Linq;
using NuGet;
using Wyam.Common.IO;
using IFileSystem = Wyam.Common.IO.IFileSystem;

namespace Wyam.NuGet
{
    internal class Repository
    {
        private readonly List<Package> _packages = new List<Package>();
        private readonly IPackageRepository _packageRepository;

        public Repository(string packageSource)
        {
            _packageRepository = PackageRepositoryFactory.Default.CreateRepository(
                string.IsNullOrWhiteSpace(packageSource) ? "https://packages.nuget.org/api/v2" : packageSource);
        }

        public void Install(string packageId, bool allowPrereleaseVersions, bool allowUnlisted = false)
        {
            Install(packageId, null, allowPrereleaseVersions, allowUnlisted);
        }

        public void Install(string packageId, string versionSpec = null, bool allowPrereleaseVersions = false, bool allowUnlisted = false)
        {
            Package package = new Package(packageId, versionSpec, allowPrereleaseVersions, allowUnlisted);
            _packages.Add(package);
        }

        public void InstallPackages(DirectoryPath absolutePackagesPath, IFileSystem fileSystem, bool updatePackages)
        {
            PackageManager packageManager = new PackageManager(_packageRepository, absolutePackagesPath.FullPath);
            
            // Install the packages
            foreach (Package package in _packages)
            {
                IPackage installedPackage = package.InstallPackage(packageManager, updatePackages);

                if (installedPackage != null)
                {
                    // Add the content path(s) to the input paths if there are content files
                    // We need to use the directory name from an actual file to make sure we get the casing right
                    foreach (string contentSegment in installedPackage.GetContentFiles()
                        .Select(x => new DirectoryPath(x.Path).Segments[0])
                        .Distinct())
                    {
                        string installPath = packageManager.PathResolver.GetInstallPath(installedPackage);
                        fileSystem.InputPaths.Insert(0, new DirectoryPath(installPath).Combine(contentSegment));
                    }
                }
            }
        }
    }
}
