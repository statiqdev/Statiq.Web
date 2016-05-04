using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuGet.Configuration;
using NuGet.Protocol.Core.Types;

namespace Wyam.Configuration.NuGet
{
    // Ported from NuGet.CommandLine.CommandLineSourceRepositoryProvider
    internal class WyamSourceRepositoryProvider : ISourceRepositoryProvider
    {
        private readonly IPackageSourceProvider _packageSourceProvider;
        private readonly List<Lazy<INuGetResourceProvider>> _resourceProviders;
        private readonly List<SourceRepository> _repositories = new List<SourceRepository>();

        // There should only be one instance of the source repository for each package source.
        private static readonly ConcurrentDictionary<PackageSource, SourceRepository> _cachedSources
            = new ConcurrentDictionary<PackageSource, SourceRepository>();

        public WyamSourceRepositoryProvider(IPackageSourceProvider packageSourceProvider)
        {
            _packageSourceProvider = packageSourceProvider;

            _resourceProviders = new List<Lazy<INuGetResourceProvider>>();
            _resourceProviders.AddRange(global::NuGet.Protocol.Core.v2.FactoryExtensionsV2.GetCoreV2(Repository.Provider));
            _resourceProviders.AddRange(global::NuGet.Protocol.Core.v3.FactoryExtensionsV2.GetCoreV3(Repository.Provider));

            // Create repositories
            _repositories = _packageSourceProvider.LoadPackageSources()
                .Where(s => s.IsEnabled)
                .Select(CreateRepository)
                .ToList();
        }

        /// <summary>
        /// Retrieve repositories that have been cached.
        /// </summary>
        public IEnumerable<SourceRepository> GetRepositories() => _repositories;

        /// <summary>
        /// Create a repository and add it to the cache.
        /// </summary>
        public SourceRepository CreateRepository(PackageSource source) => 
            _cachedSources.GetOrAdd(source, new SourceRepository(source, _resourceProviders));

        public IPackageSourceProvider PackageSourceProvider => _packageSourceProvider;
    }
}
