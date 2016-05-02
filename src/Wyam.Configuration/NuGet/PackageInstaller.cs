using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.PackageManagement;
using NuGet.Packaging;
using NuGet.ProjectManagement;
using NuGet.Protocol.Core.Types;
using NuGet.Protocol.Core.v3;
using NuGet.Repositories;
using Wyam.Common.IO;
using Wyam.Common.Tracing;
using IFileSystem = Wyam.Common.IO.IFileSystem;
using IPackageFile = NuGet.Packaging.IPackageFile;

namespace Wyam.Configuration.NuGet
{
    internal class PackageInstaller
    {
        private readonly List<PackageSource> _packageSources = new List<PackageSource>
        {
            new PackageSource("https://api.nuget.org/v3/index.json")
        };
        private readonly List<Package> _packages = new List<Package>();
        private readonly IFileSystem _fileSystem;
        private DirectoryPath _packagesPath = "packages";

        public PackageInstaller(IFileSystem fileSystem, AssemblyLoader assemblyLoader)
        {
            _fileSystem = fileSystem;

            Settings = global::NuGet.Configuration.Settings.LoadDefaultSettings(_fileSystem.RootPath.FullPath, null, new MachineWideSettings());
            IPackageSourceProvider packageSourceProvider = new PackageSourceProvider(Settings);
            SourceRepositoryProvider = new WyamSourceRepositoryProvider(packageSourceProvider);
            PackageManager = new NuGetPackageManager(SourceRepositoryProvider, Settings, GetAbsolutePackagesPath().FullPath)
            {
                PackagesFolderNuGetProject = new WyamFolderNuGetProject(this, assemblyLoader, GetAbsolutePackagesPath().FullPath)
            };

            // Get the current framework
            string frameworkName = Assembly.GetExecutingAssembly().GetCustomAttributes(true)
                .OfType<System.Runtime.Versioning.TargetFrameworkAttribute>()
                .Select(x => x.FrameworkName)
                .FirstOrDefault();
            CurrentFramework = frameworkName == null
                ? NuGetFramework.AnyFramework
                : NuGetFramework.ParseFrameworkName(frameworkName, new DefaultFrameworkNameProvider());
        }

        internal NuGetLogger Logger { get; } = new NuGetLogger();

        internal INuGetProjectContext ProjectContext { get; } = new WyamProjectContext();

        private ISettings Settings { get; }

        private ISourceRepositoryProvider SourceRepositoryProvider { get; }

        internal NuGetPackageManager PackageManager { get; }

        internal NuGetFramework CurrentFramework { get; }
        
        // TODO: Add CLI and directive support for toggling global package source
        public bool UseGlobal { get; set; } = false;

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
            if (UseGlobal)
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
            bool allowPrereleaseVersions, bool allowUnlisted, bool exclusive) 
            => _packages.Add(new Package(packageId, packageSources, versionSpec, allowPrereleaseVersions, allowUnlisted, exclusive));

        // Based primarily on NuGet.CommandLine.Commands.UpdateCommand
        public void InstallPackages(bool updatePackages)
        {
            Trace.Verbose($"Installing packages to {GetAbsolutePackagesPath().FullPath} ({(UseGlobal ? string.Empty : "not ")}using global packages folder)");
            SourceRepository localSourceRepository = SourceRepositoryProvider.CreateRepository(new PackageSource(GetAbsolutePackagesPath().FullPath));
            List<SourceRepository> defaultSourceRepositories = _packageSources.Select(SourceRepositoryProvider.CreateRepository).ToList();

            // Install the packages
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
                package.InstallPackage(this, updatePackages, localSourceRepository, sourceRepositories);
            });
        }
    }
}
