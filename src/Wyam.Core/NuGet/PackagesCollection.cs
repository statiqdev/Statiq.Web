using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using NuGet;
using NuGet.Frameworks;
using Wyam.Common.IO;
using Wyam.Common.NuGet;
using Wyam.Common.Tracing;

namespace Wyam.Core.NuGet
{
    internal class PackagesCollection : IPackagesCollection
    {
        private DirectoryPath _packagesPath = "packages";
        private DirectoryPath _contentPath = "content";
        private readonly IConfigurableFileSystem _fileSystem;

        public PackagesCollection(IConfigurableFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        // The first repository is the default NuGet feed
        private readonly List<Repository> _repositories = new List<Repository>()
        {
            new Repository(null)
        };

        public DirectoryPath PackagesPath
        {
            get { return _packagesPath; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(PackagesPath));
                }
                _packagesPath = value;
            }
        }

        private string AbsolutePackagesPath => _fileSystem.RootPath.Combine(PackagesPath).Collapse().FullPath;

        public DirectoryPath ContentPath
        {
            get { return _contentPath; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(ContentPath));
                }
                _contentPath = value;
            }
        }

        public IRepository Repository(string packageSource)
        {
            Repository repository = new Repository(packageSource);
            _repositories.Add(repository);
            return repository;
        }

        public IRepository Install(string packageId, bool allowPrereleaseVersions, bool allowUnlisted = false)
        {
            return Install(packageId, null, allowPrereleaseVersions, allowUnlisted);
        }

        public IRepository Install(string packageId, string versionSpec = null,
            bool allowPrereleaseVersions = false, bool allowUnlisted = false)
        {
            _repositories[0].Install(packageId, versionSpec, allowPrereleaseVersions, allowUnlisted);
            return this;
        }

        public void InstallPackages(bool updatePackages)
        {
            foreach (Repository repository in _repositories)
            {
                repository.InstallPackages(AbsolutePackagesPath, ContentPath, _fileSystem, updatePackages);
            }
        }

        // TODO: Return IEnumerable<DirectoryPath> and remove calls to System.IO
        public IEnumerable<string> GetCompatibleAssemblyPaths()
        {
            List<string> assemblyPaths = new List<string>();
            FrameworkReducer reducer = new FrameworkReducer();
            NuGetFramework targetFramework = new NuGetFramework(".NETFramework", Version.Parse("4.6"));  // If alternate versions of Wyam are developed (I.e., for DNX), this will need to be switched
            NuGetFrameworkFullComparer frameworkComparer = new NuGetFrameworkFullComparer();
            IPackageRepository packageRepository = PackageRepositoryFactory.Default.CreateRepository(AbsolutePackagesPath);
            PackageManager packageManager = new PackageManager(packageRepository, AbsolutePackagesPath);
            foreach (IPackage package in packageManager.LocalRepository.GetPackages())
            {
                List<KeyValuePair<IPackageFile, NuGetFramework>> filesAndFrameworks = package.GetLibFiles()
                    .Where(x => x.TargetFramework != null)
                    .Select(x => new KeyValuePair<IPackageFile, NuGetFramework>(x,
                        new NuGetFramework(x.TargetFramework.Identifier, x.TargetFramework.Version, x.TargetFramework.Profile)))
                    .ToList();
                NuGetFramework targetPackageFramework = reducer.GetNearest(targetFramework, filesAndFrameworks.Select(x => x.Value));
                if (targetPackageFramework != null)
                {
                    List<string> packageAssemblyPaths = filesAndFrameworks
                        .Where(x => frameworkComparer.Equals(targetPackageFramework, x.Value))
                        .Select(x => System.IO.Path.Combine(AbsolutePackagesPath, String.Format(CultureInfo.InvariantCulture, "{0}.{1}", package.Id, package.Version), x.Key.Path))
                        .Where(x => System.IO.Path.GetExtension(x) == ".dll")
                        .ToList();
                    foreach (string packageAssemblyPath in packageAssemblyPaths)
                    {
                        Trace.Verbose("Added assembly file {0} from package {1}.{2}", packageAssemblyPath, package.Id, package.Version);
                    }
                    assemblyPaths.AddRange(filesAndFrameworks
                        .Where(x => frameworkComparer.Equals(targetPackageFramework, x.Value))
                        .Select(x => System.IO.Path.Combine(AbsolutePackagesPath, String.Format(CultureInfo.InvariantCulture, "{0}.{1}", package.Id, package.Version), x.Key.Path))
                        .Where(x => System.IO.Path.GetExtension(x) == ".dll"));
                }
                else
                {
                    Trace.Verbose("Could not find compatible framework for package {0}.{1} (this is normal for content-only packages)", package.Id, package.Version);
                }
            }
            return assemblyPaths;
        }
    }
}
