using System.Collections.Generic;
using System.IO;
using NuGet;
using NuGet.Frameworks;

namespace Wyam.Core.NuGet
{
    internal class Repository : IRepository
    {
        private readonly List<Package> _packages = new List<Package>(); 
        private readonly IPackageRepository _packageRepository;

        public Repository(string packageSource)
        {
            _packageRepository = PackageRepositoryFactory.Default.CreateRepository(
                string.IsNullOrWhiteSpace(packageSource) ? "https://packages.nuget.org/api/v2" : packageSource);
        }

        public IRepository Install(string packageId, bool allowPrereleaseVersions, bool allowUnlisted = false)
        {
            return Install(packageId, null, allowPrereleaseVersions, allowUnlisted);
        }

        public IRepository Install(string packageId, string versionSpec = null, bool allowPrereleaseVersions = false, bool allowUnlisted = false)
        {
            Package package = new Package(packageId, versionSpec, allowPrereleaseVersions, allowUnlisted);
            _packages.Add(package);
            return this;
        }

        public void InstallPackages(string path, Engine engine, bool updatePackages)
        {
            PackageManager packageManager = new PackageManager(_packageRepository, path);
            packageManager.PackageInstalled += (sender, args) =>
            {
                // Copy all content files on install
                foreach (IPackageFile packageFile in args.Package.GetContentFiles())
                {
                    string filePath = Path.Combine(engine.InputFolder, packageFile.EffectivePath);
                    string filePathDirectory = Path.GetDirectoryName(filePath);
                    if (!Directory.Exists(filePathDirectory))
                    {
                        Directory.CreateDirectory(filePathDirectory);
                    }
                    using (var fileStream = File.Create(filePath))
                    {
                        packageFile.GetStream().CopyTo(fileStream);
                    }
                }
            };
            packageManager.PackageUninstalling += (sender, args) =>
            {
                // Remove all content files on uninstall
                foreach (IPackageFile packageFile in args.Package.GetContentFiles())
                {
                    string filePath = Path.Combine(engine.InputFolder, packageFile.EffectivePath);
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }
            };
            foreach (Package package in _packages)
            {
                package.InstallPackage(packageManager, engine, updatePackages);
            }
        }
    }
}
