using System;
using System.Collections.Generic;
using System.IO;

namespace Wyam.Core.NuGet
{
    internal class PackagesCollection : IPackagesCollection
    {
        private readonly Engine _engine;
        private string _path;

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
            get { return _path; }
            set { _path = System.IO.Path.Combine(_engine.RootFolder, value); }
        }

        public IRepository AddRepository(string packageSource)
        {
            Repository repository = new Repository(packageSource);
            _repositories.Add(repository);
            return repository;
        }

        public IPackagesCollection Add(string packageId, string versionSpec = null, 
            bool allowPrereleaseVersions = false, bool allowUnlisted = false)
        {
            _repositories[0].Add(packageId, versionSpec, allowPrereleaseVersions, allowUnlisted);
            return this;
        }

        public void InstallPackages()
        {
            // Default package path
            if (string.IsNullOrWhiteSpace(Path))
            {
                Path = System.IO.Path.Combine(_engine.RootFolder, "packages");
            }

            // Iterate repositories
            foreach (Repository repository in _repositories)
            {
                repository.InstallPackages(Path);
            }
        }
    }
}
