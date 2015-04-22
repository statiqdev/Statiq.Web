using System.Collections.Generic;

namespace Wyam.Core.Extensibility
{
    internal class PackagesCollection : IPackagesCollection
    {
        // The first repository is the default NuGet feed
        private readonly List<Repository> _repositories = new List<Repository>()
        {
            new Repository(null)
        };

        public IRepository AddRepository(string packageSource)
        {
            Repository repository = new Repository(packageSource);
            _repositories.Add(repository);
            return repository;
        }

        public IPackagesCollection AddPackage(string packageId, string versionSpec = null, 
            bool allowPrereleaseVersions = false, bool allowUnlisted = false)
        {
            _repositories[0].AddPackage(packageId, versionSpec, allowPrereleaseVersions, allowUnlisted);
            return this;
        }

        public IEnumerable<Repository> Repositories
        {
            get { return _repositories; }
        }
    }
}
