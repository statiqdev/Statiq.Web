using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.PackageManagement;
using NuGet.ProjectManagement;
using NuGet.Protocol.Core.Types;
using Wyam.Common.IO;
using Wyam.Common.Tracing;
using Wyam.Configuration.Assemblies;
using IFileSystem = Wyam.Common.IO.IFileSystem;

namespace Wyam.Configuration.NuGet
{
    public class PackageInstaller
    {
        private readonly List<PackageSource> _packageSources = new List<PackageSource>
        {
            new PackageSource("https://api.nuget.org/v3/index.json")
        };
        private readonly List<Package> _packages = new List<Package>();
        private readonly IFileSystem _fileSystem;
        private readonly WyamFolderNuGetProject _nuGetProject;
        private DirectoryPath _packagesPath = "packages";

        internal PackageInstaller(IFileSystem fileSystem, AssemblyLoader assemblyLoader)
        {
            _fileSystem = fileSystem;

            // Get the current framework
            string frameworkName = Assembly.GetExecutingAssembly().GetCustomAttributes(true)
                .OfType<System.Runtime.Versioning.TargetFrameworkAttribute>()
                .Select(x => x.FrameworkName)
                .FirstOrDefault();
            CurrentFramework = frameworkName == null
                ? NuGetFramework.AnyFramework
                : NuGetFramework.ParseFrameworkName(frameworkName, new DefaultFrameworkNameProvider());

            Settings = global::NuGet.Configuration.Settings.LoadDefaultSettings(_fileSystem.RootPath.FullPath, null, new MachineWideSettings());
            IPackageSourceProvider packageSourceProvider = new PackageSourceProvider(Settings);
            SourceRepositoryProvider = new WyamSourceRepositoryProvider(packageSourceProvider);
            _nuGetProject = new WyamFolderNuGetProject(fileSystem, assemblyLoader, CurrentFramework, GetAbsolutePackagesPath().FullPath);
            PackageManager = new NuGetPackageManager(SourceRepositoryProvider, Settings, GetAbsolutePackagesPath().FullPath)
            {
                PackagesFolderNuGetProject = _nuGetProject
            };
        }

        internal NuGetLogger Logger { get; } = new NuGetLogger();

        internal INuGetProjectContext ProjectContext { get; } = new WyamProjectContext();

        private ISettings Settings { get; }

        private ISourceRepositoryProvider SourceRepositoryProvider { get; }

        internal NuGetPackageManager PackageManager { get; }

        internal NuGetFramework CurrentFramework { get; }

        public bool UpdatePackages { get; set; }
        
        public bool UseLocal { get; set; }

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
            if (!UseLocal)
            {
                string globalPackagesFolder = SettingsUtility.GetGlobalPackagesFolder(Settings);
                if(!string.IsNullOrEmpty(globalPackagesFolder))
                {
                    packagesPath = packagesPath.Combine(new DirectoryPath(globalPackagesFolder)).Collapse();
                }
            }
            return packagesPath;
        }

        // Note that sources are searched first at index 0, then index 1, and so on until a match is found
        public void AddPackageSource(string packageSource) => _packageSources.Insert(0, new PackageSource(packageSource));

        public void AddPackage(string packageId, IReadOnlyList<string> packageSources, string versionSpec, 
            bool getLatest, bool allowPrereleaseVersions, bool allowUnlisted, bool exclusive) 
            => _packages.Add(new Package(packageId, packageSources, versionSpec, getLatest, allowPrereleaseVersions, allowUnlisted, exclusive));

        // Based primarily on NuGet.CommandLine.Commands.UpdateCommand
        internal void InstallPackages()
        {
            Trace.Verbose($"Installing packages to {GetAbsolutePackagesPath().FullPath} (using {(UseLocal ? "local" : "global")} packages folder)");
            SourceRepository localSourceRepository = SourceRepositoryProvider.CreateRepository(new PackageSource(GetAbsolutePackagesPath().FullPath));
            List<SourceRepository> defaultSourceRepositories = _packageSources.Select(SourceRepositoryProvider.CreateRepository).ToList();

            // Install the packages
            _nuGetProject.StartPackageInstall();
            Parallel.ForEach(_packages, package =>
            {
                List<SourceRepository> sourceRepositories = defaultSourceRepositories;
                if (package.PackageSources != null && package.PackageSources.Count > 0)
                {
                    sourceRepositories = package.PackageSources.Select(SourceRepositoryProvider.CreateRepository).ToList();
                    if (!package.Exclusive)
                    {
                        sourceRepositories = sourceRepositories.Concat(defaultSourceRepositories).ToList();
                    }
                }
                package.InstallPackage(this, UpdatePackages, localSourceRepository, sourceRepositories);
            });

            // Process the package (do this after all packages have been installed)
            _nuGetProject.ProcessAssembliesAndContent();
        }
    }
}
