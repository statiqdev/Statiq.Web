using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.PackageManagement;
using NuGet.Protocol.Core.Types;
using Wyam.Common.IO;
using Wyam.Configuration.Assemblies;
using Wyam.Core.Execution;
using IFileSystem = Wyam.Common.IO.IFileSystem;
using Trace = Wyam.Common.Tracing.Trace;

namespace Wyam.Configuration.NuGet
{
    public class PackageInstaller
    {
        private readonly Dictionary<string, Package> _packages = new Dictionary<string, Package>();
        private readonly NuGetLogger _logger = new NuGetLogger();
        private readonly IFileSystem _fileSystem;
        private readonly AssemblyLoader _assemblyLoader;
        private readonly NuGetFramework _currentFramework;
        private readonly ISettings _settings;
        private readonly SourceRepositoryProvider _sourceRepositories;

        private DirectoryPath _packagesPath = "packages";

        internal PackageInstaller(IFileSystem fileSystem, AssemblyLoader assemblyLoader)
        {
            _fileSystem = fileSystem;
            _assemblyLoader = assemblyLoader;
            _currentFramework = GetCurrentFramework();
            _settings = Settings.LoadDefaultSettings(fileSystem.RootPath.FullPath, null, new MachineWideSettings());
            _sourceRepositories = new SourceRepositoryProvider(_settings);
        }

        private NuGetFramework GetCurrentFramework()
        {
            string frameworkName = Assembly.GetExecutingAssembly().GetCustomAttributes(true)
                .OfType<System.Runtime.Versioning.TargetFrameworkAttribute>()
                .Select(x => x.FrameworkName)
                .FirstOrDefault();
            return frameworkName == null
                ? NuGetFramework.AnyFramework
                : NuGetFramework.ParseFrameworkName(frameworkName, new DefaultFrameworkNameProvider());
        }
        
        /// <summary>
        /// Adds the specified package source. Sources added this way will be searched before any global sources.
        /// </summary>
        /// <param name="packageSource">The package source to add.</param>
        public void AddPackageSource(string packageSource) => _sourceRepositories.AddDefaultRepository(packageSource);

        /// <summary>
        /// Adds a package.
        /// </summary>
        /// <param name="packageId">The package identifier.</param>
        /// <param name="packageSources">The package sources.</param>
        /// <param name="versionRange">The version range.</param>
        /// <param name="getLatest">If set to <c>true</c>, the latest version of the package will always be downloaded.</param>
        /// <param name="allowPrereleaseVersions">If set to <c>true</c>, allow prerelease versions.</param>
        /// <param name="allowUnlisted">If set to <c>true</c>, allow unlisted versions.</param>
        /// <param name="exclusive">If set to <c>true</c>, only use the package sources defined for this package.</param>
        public void AddPackage(string packageId, IEnumerable<string> packageSources = null, string versionRange = null,
            bool getLatest = false, bool allowPrereleaseVersions = false, bool allowUnlisted = false, bool exclusive = false)
        {
            if (_packages.ContainsKey(packageId))
            {
                throw new ArgumentException($"A package with the ID {packageId} has already been added");
            }

            // If this package is a known Wyam extension and no version is specified, set to the current version
            if (KnownExtension.Values.Values.Any(x => x.PackageId == packageId) && versionRange == null)
            {
                versionRange = $"[{Engine.Version}]";
                Trace.Verbose($"Added known extension package {packageId} without version, setting version to {versionRange}");
            }

            _packages.Add(packageId, new Package(_currentFramework, packageId,
                packageSources?.Select(_sourceRepositories.CreateRepository).ToList(),
                versionRange, getLatest, allowPrereleaseVersions, allowUnlisted, exclusive));
        }

        /// <summary>
        /// Determines whether the specified package has already been added.
        /// </summary>
        /// <param name="packageId">The package identifier.</param>
        public bool ContainsPackage(string packageId) => _packages.ContainsKey(packageId);

        public ICollection<string> PackageIds => _packages.Keys;

        public bool UpdatePackages { get; set; }

        public bool UseLocalPackagesFolder { get; set; }

        public bool UseGlobalPackageSources { get; set; }

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

        private DirectoryPath GetAbsolutePackagesPath()
        {
            DirectoryPath packagesPath = _fileSystem.RootPath.Combine(PackagesPath).Collapse();
            if (!UseLocalPackagesFolder)
            {
                string globalPackagesFolder = SettingsUtility.GetGlobalPackagesFolder(_settings);
                if (!string.IsNullOrEmpty(globalPackagesFolder))
                {
                    packagesPath = packagesPath.Combine(new DirectoryPath(globalPackagesFolder)).Collapse();
                }
            }
            return packagesPath;
        }
        
        internal void InstallPackages()
        {
            DirectoryPath packagesPath = GetAbsolutePackagesPath();
            Trace.Information($"Installing packages to {packagesPath.FullPath} (using {(UseLocalPackagesFolder ? "local" : "global")} packages folder)");

            try
            {
                // Add the global default sources if requested
                if (UseGlobalPackageSources)
                {
                    _sourceRepositories.AddGlobalDefaults();
                }

                // Get the local repository
                SourceRepository localRepository = _sourceRepositories.CreateRepository(packagesPath.FullPath);

                // Cache the packages in a packages file
                string packagesFilePath = _fileSystem.RootPath.CombineFile(new FilePath("packages.xml")).FullPath;
                Trace.Verbose($"Writing packages file to {packagesFilePath}");
                using (InstalledPackagesCache installedPackages = new InstalledPackagesCache(packagesFilePath, UpdatePackages))
                { 
                    // Get the package manager and repositories
                    WyamFolderNuGetProject nuGetProject = new WyamFolderNuGetProject(_fileSystem, _assemblyLoader, _currentFramework, installedPackages, packagesPath.FullPath);
                    NuGetPackageManager packageManager = new NuGetPackageManager(_sourceRepositories, _settings, packagesPath.FullPath)
                    {
                        PackagesFolderNuGetProject = nuGetProject
                    };
                    IReadOnlyList<SourceRepository> remoteRepositories = _sourceRepositories.GetDefaultRepositories();

                    // Resolve all the versions
                    IReadOnlyList<SourceRepository> installationRepositories = remoteRepositories;
                    try
                    {
                        ResolveVersions(localRepository, remoteRepositories);
                    }
                    catch (Exception ex)
                    {
                        Trace.Verbose($"Exception while resolving package versions: {ex.Message}");
                        Trace.Warning("Error while resolving package versions, attempting without remote repositories");
                        installationRepositories = new[] {localRepository};
                        ResolveVersions(localRepository, Array.Empty<SourceRepository>());
                    }

                    // Install the packages (doing this synchronously since doing it in parallel triggers file lock errors in NuGet on a clean system)
                    try
                    {
                        InstallPackages(packageManager, localRepository, installationRepositories, installedPackages);
                    }
                    catch (Exception ex)
                    {
                        Trace.Verbose($"Exception while installing packages: {(ex is AggregateException ? string.Join("; ", ((AggregateException)ex).InnerExceptions.Select(x => x.Message)) : ex.Message)}");
                        Trace.Warning("Error while installing packages, attempting without remote repositories");
                        InstallPackages(packageManager, localRepository, Array.Empty<SourceRepository>(), installedPackages);
                    }

                    // Process the package (do this after all packages have been installed)
                    nuGetProject.ProcessAssembliesAndContent();
                }
            }
            catch (Exception ex)
            {
                Trace.Verbose($"Unexpected exception while installing packages: {(ex is AggregateException ? string.Join("; ", ((AggregateException)ex).InnerExceptions.Select(x => x.Message)) : ex.Message)}");
                Trace.Warning("Error while installing packages, attempting to continue anyway");
            }
        }

        private void ResolveVersions(SourceRepository localRepository, IReadOnlyList<SourceRepository> remoteRepositories) =>
            Task.WaitAll(_packages.Values.Select(x => x.ResolveVersion(localRepository, remoteRepositories, UpdatePackages, _logger)).ToArray());

        private void InstallPackages(NuGetPackageManager packageManager, SourceRepository localRepository, IReadOnlyList<SourceRepository> installationRepositories, InstalledPackagesCache installedPackages)
        {
            foreach (Package package in _packages.Values)
            {
                package.Install(localRepository, installationRepositories, installedPackages, packageManager).Wait();
            }
        }
    }
}
