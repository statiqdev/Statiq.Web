using System.Collections.Generic;
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

        public IRepository Add(string packageId, string versionSpec = null, bool allowPrereleaseVersions = false, bool allowUnlisted = false)
        {
            Package package = new Package(packageId, versionSpec, allowPrereleaseVersions, allowUnlisted);
            _packages.Add(package);
            return this;
        }

        public void InstallPackages(string path, Engine engine)
        {
            PackageManager packageManager = new PackageManager(_packageRepository, path);
            foreach (Package package in _packages)
            {
                package.InstallPackage(packageManager, engine);
            }
        }
    }
}
