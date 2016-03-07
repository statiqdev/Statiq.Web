using System.Collections.Generic;
using System.IO;
using NuGet;
using NuGet.Frameworks;
using Wyam.Common.IO;
using Wyam.Common.NuGet;
using IFileSystem = Wyam.Common.IO.IFileSystem;
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

        public void InstallPackages(string absolutePackagesPath, DirectoryPath contentPath, IFileSystem fileSystem, bool updatePackages)
        {

            // TODO: Use this code when ReadFiles (and other modules) support multiple input paths - make sure to test with clean (I.e., no packages) repo
            /*
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
                    using (Stream fileStream = file.Open(FileMode.Open, FileAccess.Write, FileShare.None))
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
            */

            // ***********************************************************
            // OLD CODE - remove when code above is ready
            PackageManager packageManager = new PackageManager(_packageRepository, absolutePackagesPath);
            packageManager.PackageInstalled += (sender, args) =>
            {
                // Copy all content files on install
                foreach (IPackageFile packageFile in args.Package.GetContentFiles())
                {
                    IFile filePath = fileSystem.GetInputFile(packageFile.EffectivePath);
                    IDirectory filePathDirectory = filePath.Directory;
                    if (!filePathDirectory.Exists)
                    {
                        filePathDirectory.Create();
                    }
                    if (!filePath.Exists)
                    {
                        using (var fileStream = filePath.OpenWrite())
                        {
                            packageFile.GetStream().CopyTo(fileStream);
                        }
                    }
                }
            };
            packageManager.PackageUninstalling += (sender, args) =>
            {
                // Remove all content files on uninstall
                foreach (IPackageFile packageFile in args.Package.GetContentFiles())
                {
                    IFile filePath = fileSystem.GetInputFile(packageFile.EffectivePath);
                    if (filePath.Exists)
                    {
                        filePath.Delete();
                    }
                }
            };
            foreach (Package package in _packages)
            {
                package.InstallPackage(packageManager, updatePackages);
            }
        }
    }
}
