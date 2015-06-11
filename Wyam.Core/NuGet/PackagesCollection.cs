using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using NuGet;
using NuGet.Frameworks;

namespace Wyam.Core.NuGet
{
    internal class PackagesCollection : IPackagesCollection
    {
        private readonly Engine _engine;
        private string _path = "packages";

        public PackagesCollection(Engine engine)
        {
            _engine = engine;
        }

        // The first repository is the default NuGet feed
        private readonly List<Repository> _repositories = new List<Repository>()
        {
            new Repository(null)
        };
        
        public string Path
        {
            get { return System.IO.Path.Combine(_engine.RootFolder, _path); }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("Path");
                }
                _path = value;
            }
        }

        public IRepository Repository(string packageSource)
        {
            Repository repository = new Repository(packageSource);
            _repositories.Add(repository);
            return repository;
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
                repository.InstallPackages(Path, _engine, updatePackages);
            }
        }

        public IEnumerable<string> GetCompatibleAssemblyPaths()
        {
            List<string> assemblyPaths = new List<string>();
            FrameworkReducer reducer = new FrameworkReducer();
            NuGetFramework targetFramework = new NuGetFramework(".NETFramework", Version.Parse("4.5"));  // If alternate versions of Wyam are developed (I.e., for DNX), this will need to be switched
            NuGetFrameworkFullComparer frameworkComparer = new NuGetFrameworkFullComparer();
            IPackageRepository packageRepository = PackageRepositoryFactory.Default.CreateRepository(Path);
            PackageManager packageManager = new PackageManager(packageRepository, Path);
            foreach (IPackage package in packageManager.LocalRepository.GetPackages())
            {
                List<KeyValuePair<IPackageFile, NuGetFramework>> filesAndFrameworks = package.GetLibFiles()
                    .Select(x => new KeyValuePair<IPackageFile, NuGetFramework>(x, 
                        new NuGetFramework(x.TargetFramework.Identifier, x.TargetFramework.Version, x.TargetFramework.Profile)))
                    .ToList();
                NuGetFramework targetPackageFramework = reducer.GetNearest(targetFramework, filesAndFrameworks.Select(x => x.Value));
                if (targetPackageFramework != null)
                {
                    assemblyPaths.AddRange(filesAndFrameworks
                        .Where(x => frameworkComparer.Equals(targetPackageFramework, x.Value))
                        .Select(x => System.IO.Path.Combine(Path, String.Format(CultureInfo.InvariantCulture, "{0}.{1}", package.Id, package.Version), x.Key.Path))
                        .Where(x => System.IO.Path.GetExtension(x) == ".dll"));
                }
            }
            return assemblyPaths;
        }
    }
}
