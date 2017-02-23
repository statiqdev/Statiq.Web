using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using Wyam.Common.Tracing;

namespace Wyam.Configuration.NuGet
{
    internal class InstalledPackagesCache : IDisposable
    {
        private List<CachedPackage> _installedPackages = new List<CachedPackage>(); // The packages installed during this session
        private readonly string _fullPath;
        private readonly CachedPackage[] _cachedPackages;

        private CachedPackageEntry _currentlyInstallingPackage = null;

        public InstalledPackagesCache(string fullPath, bool updatePackages)
        {
            if (fullPath == null)
            {
                throw new ArgumentNullException(nameof(fullPath));
            }
            _fullPath = fullPath;
            _cachedPackages = ReadCacheFile(fullPath, updatePackages);
        }

        private static CachedPackage[] ReadCacheFile(string fullPath, bool updatePackages)
        {
            if (!updatePackages && File.Exists(fullPath))
            {
                try
                {
                    XDocument packagesDocument = XDocument.Load(fullPath);
                    if (packagesDocument.Root == null)
                    {
                        Trace.Warning("No root element in packages file");
                    }
                    else if (packagesDocument.Root.Name != CachedPackage.PackagesElementName)
                    {
                        Trace.Warning($@"Packages file root element should be named ""{CachedPackage.PackagesElementName}"" but is actually named ""{packagesDocument.Root.Name}""");
                    }
                    else
                    {
                        return packagesDocument.Root.Elements(CachedPackage.PackageElementName).Select(x => new CachedPackage(x)).ToArray();
                    }
                }
                catch (Exception ex)
                {
                    Trace.Warning($"Error while reading packages file: {ex.Message}");
                }
            }
            return Array.Empty<CachedPackage>();
        }

        /// <summary>
        /// Gets all top-level installed packages from the incoming cache file.
        /// </summary>
        public IEnumerable<PackageReference> GetCachedPackages() => _cachedPackages.Select(x => x.PackageReference);

        /// <summary>
        /// Gets all installed packages from this session and their dependencies.
        /// </summary>
        public IEnumerable<PackageIdentity> GetInstalledPackagesAndDependencies() => 
            _installedPackages.Select(x => x.PackageReference.PackageIdentity)
                .Concat(_installedPackages.SelectMany(x => x.Dependencies.Select(dep => dep.PackageIdentity)));

        /// <summary>
        /// Verifies that a package has been previously installed and if so,
        /// adds it back to the outgoing cache file along with all it's
        /// previously calculated dependencies.
        /// </summary>
        /// <param name="packageIdentity">The identity of the package to verify.</param>
        /// <returns><c>true</c> if the package was previously installed and cached, <c>false</c> otherwise</returns>
        public bool VerifyPackage(PackageIdentity packageIdentity)
        {
            CachedPackage cached = _cachedPackages.FirstOrDefault(x =>
                x.PackageReference.PackageIdentity.Id.Equals(packageIdentity.Id, StringComparison.OrdinalIgnoreCase)
                && x.PackageReference.PackageIdentity.Version.Equals(packageIdentity.Version));
            if (cached != null)
            {
                _installedPackages.Add(cached);
                return true;
            }
            return false;
        }
        
        public IDisposable AddPackage(PackageIdentity identity, NuGetFramework targetFramework)
        {
            if (_currentlyInstallingPackage != null && _currentlyInstallingPackage.CurrentlyInstalling)
            {
                // We're currently installing a package, so add the dependency to that one
                _currentlyInstallingPackage.CachedPackage.AddDependency(
                    new CachedPackage(identity, targetFramework));
                return _currentlyInstallingPackage;
            }

            // This is a new top-level installation, so add to the root
            CachedPackage cachedPackage = new CachedPackage(identity, targetFramework);
            _installedPackages.Add(cachedPackage);
            _currentlyInstallingPackage = new CachedPackageEntry(cachedPackage);
            return _currentlyInstallingPackage;
        }

        public void Dispose() => 
            new XDocument(
                new XElement(CachedPackage.PackagesElementName,
                    _installedPackages.Select(x => (object)x.Element).ToArray())).Save(_fullPath);

        private class CachedPackageEntry : IDisposable
        {
            public CachedPackage CachedPackage { get; }

            public bool CurrentlyInstalling { get; private set; }

            public CachedPackageEntry(CachedPackage cachedPackage)
            {
                CachedPackage = cachedPackage;
                CurrentlyInstalling = true;
            }

            public void Dispose()
            {
                CurrentlyInstalling = false;
            }
        }
    }
}
