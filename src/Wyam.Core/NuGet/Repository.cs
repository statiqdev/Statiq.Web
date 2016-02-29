using System.Collections.Generic;
using System.IO;
using NuGet;
using NuGet.Frameworks;
using Wyam.Common.IO;
using Wyam.Common.NuGet;
using Wyam.Core.IO;
using Path = System.IO.Path;

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
        
        public void InstallPackages(string absolutePackagesPath, DirectoryPath contentPath, FileSystem fileSystem, bool updatePackages)
        {
            PackageManager packageManager = new PackageManager(_packageRepository, absolutePackagesPath);

            // On package install...
            packageManager.PackageInstalled += (sender, args) =>
            {
                IDirectory packageContentDirectory = fileSystem.GetRootDirectory(contentPath.Combine(args.Package.Id));

                // Copy all content files on install and add to input paths
                bool firstFile = true;
                foreach (IPackageFile packageFile in args.Package.GetContentFiles())
                {
                    if (firstFile)
                    {
                        // This package does have content files, so create the directory and add an input path
                        packageContentDirectory.Create();
                        fileSystem.InputPaths.Insert(0, packageContentDirectory.Path);
                        firstFile = false;
                    }

                    IFile file = packageContentDirectory.GetFile(packageFile.EffectivePath);
                    file.Directory.Create();
                    using (var fileStream = file.Open(FileMode.Create))
                    {
                        packageFile.GetStream().CopyTo(fileStream);
                    }
                }
            };

            // On package uninstall...
            packageManager.PackageUninstalling += (sender, args) =>
            {
                IDirectory packageContentDirectory = fileSystem.GetRootDirectory(contentPath.Combine(args.Package.Id));
                packageContentDirectory.Delete(true);
            };

            // Install the packages
            foreach (Package package in _packages)
            {
                package.InstallPackage(packageManager, updatePackages);
            }
        }
    }
}
