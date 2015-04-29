using System.Collections.Generic;
using NuGet;

namespace Wyam.Core.NuGet
{
    internal class Repository : IRepository
    {
        private readonly List<Package> _packages = new List<Package>(); 
        private readonly string _packageSource;
        private IPackageRepository _packageRepository;

        public Repository(string packageSource)
        {
            _packageSource = string.IsNullOrWhiteSpace(packageSource) ? "https://packages.nuget.org/api/v2" : packageSource;
        }

        public IRepository Add(string packageId, string versionSpec = null, bool allowPrereleaseVersions = false, bool allowUnlisted = false)
        {
            Package package = new Package(packageId, versionSpec, allowPrereleaseVersions, allowUnlisted);
            _packages.Add(package);
            return this;
        }

        public IPackageRepository GetRepository()
        {
            if (_packageRepository == null)
            {
                _packageRepository = PackageRepositoryFactory.Default.CreateRepository(_packageSource);
            }
            return _packageRepository;
        }

        public void InstallPackages(string path)
        {
            IPackageRepository repository = GetRepository();
            PackageManager packageManager = new PackageManager(repository, path);
            foreach (Package package in _packages)
            {
                package.InstallPackage(packageManager);
            }
        }
    }
}
