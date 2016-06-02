using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.PackageManagement;
using NuGet.Protocol.Core.Types;
using Wyam.Common.IO;
using Wyam.Common.Tracing;
using Wyam.Configuration.Assemblies;
using IFileSystem = Wyam.Common.IO.IFileSystem;

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
            bool getLatest = false, bool allowPrereleaseVersions = true, bool allowUnlisted = false, bool exclusive = false)
        {
            if (_packages.ContainsKey(packageId))
            {
                throw new ArgumentException($"A package with the ID {packageId} has already been added");
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
            Trace.Verbose($"Installing packages to {packagesPath.FullPath} (using {(UseLocalPackagesFolder ? "local" : "global")} packages folder)");

            // Add the global default sources if requested
            if (UseGlobalPackageSources)
            {
                _sourceRepositories.AddGlobalDefaults();
            }

            // Get the local repository
            SourceRepository localRepository = _sourceRepositories.CreateRepository(packagesPath.FullPath);

            // Install the packages
            WyamFolderNuGetProject nuGetProject = new WyamFolderNuGetProject(_fileSystem, _assemblyLoader, _currentFramework, packagesPath.FullPath);
            NuGetPackageManager packageManager = new NuGetPackageManager(_sourceRepositories, _settings, packagesPath.FullPath)
            {
                PackagesFolderNuGetProject = nuGetProject
            };
            Task.WaitAll(_packages.Values.Select(x => x.Install(localRepository, _sourceRepositories.GetDefaultRepositories(), UpdatePackages, packageManager, _logger)).ToArray());

            // Process the package (do this after all packages have been installed)
            nuGetProject.ProcessAssembliesAndContent();
        }
    }
}
