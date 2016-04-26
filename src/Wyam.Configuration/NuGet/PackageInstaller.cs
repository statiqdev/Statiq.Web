using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.PackageManagement;
using NuGet.Packaging;
using NuGet.ProjectManagement;
using NuGet.Protocol.Core.Types;
using Wyam.Common.IO;
using Wyam.Common.Tracing;
using IFileSystem = Wyam.Common.IO.IFileSystem;
using IPackageFile = NuGet.Packaging.IPackageFile;

namespace Wyam.Configuration.NuGet
{
    internal class PackageInstaller
    {
        private readonly NuGetLogger _logger = new NuGetLogger();
        private readonly List<PackageSource> _packageSources = new List<PackageSource>
        {
            new PackageSource("https://packages.nuget.org/api/v2")
        };
        private readonly List<Package> _packages = new List<Package>();
        private readonly IFileSystem _fileSystem;
        private readonly ISettings _settings;
        private readonly IPackageSourceProvider _packageSourceProvider;
        private readonly ISourceRepositoryProvider _sourceRepositoryProvider;
        private DirectoryPath _packagesPath = "packages";

        public PackageInstaller(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
            _settings = Settings.LoadDefaultSettings(_fileSystem.RootPath.FullPath, null, new MachineWideSettings());
            _packageSourceProvider = new PackageSourceProvider(_settings);
            _sourceRepositoryProvider = new WyamSourceRepositoryProvider(_packageSourceProvider);
            FolderProject = new FolderNuGetProject(
                AbsolutePackagesPath.FullPath,
                new PackagePathResolver(AbsolutePackagesPath.FullPath));
            PackageManager = new NuGetPackageManager(_sourceRepositoryProvider, _settings, AbsolutePackagesPath.FullPath);
        }

        public FolderNuGetProject FolderProject { get; }

        public NuGetPackageManager PackageManager { get; }

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

        private DirectoryPath AbsolutePackagesPath => _fileSystem.RootPath.Combine(PackagesPath).Collapse();

        // Note that sources are searched first at index 0, then index 1, and so on until a match is found
        public void AddPackageSource(string packageSource) => _packageSources.Insert(0, new PackageSource(packageSource));

        public void AddPackage(string packageId, IReadOnlyList<string> packageSources, string versionSpec, bool allowPrereleaseVersions, bool allowUnlisted, bool exclusive) => _packages.Add(new Package(packageId, packageSources, versionSpec, allowPrereleaseVersions, allowUnlisted, exclusive));

        // Based primarily on NuGet.CommandLine.Commands.InstallCommand.InstallPackage()
        public void InstallPackages(bool updatePackages)
        {
            // TODO: Don't forget about updatePackages - not sure how to handle w/ v3

            List<SourceRepository> defaultSourceRepositories = _packageSources.Select(_sourceRepositoryProvider.CreateRepository).ToList();

            // Install the packages
            foreach (Package package in _packages)
            {
                IEnumerable<SourceRepository> sourcesRepositories = defaultSourceRepositories;
                if (package.PackageSources != null && package.PackageSources.Count > 0)
                {
                    sourcesRepositories = package.PackageSources.Select(_sourceRepositoryProvider.CreateRepository);
                }
                package.InstallPackage(this, sourcesRepositories);
            }





            // Install the packages
            /*
            foreach (Package package in _packages)
            {
                // Get the correct set of sources and install the package
                PackageManager packageManager = defaultPackageManager;
                if (package.PackageSources != null && package.PackageSources.Count > 0)
                {
                    IEnumerable<string> packageSources = package.Exclusive ? package.PackageSources : package.PackageSources.Concat(_packageSources);
                    packageManager = GetPackageManager(packageSources);
                }
                IPackage installedPackage = package.InstallPackage(packageManager, updatePackages);

                // Add the content path(s) to the input paths if there are content files
                // We need to use the directory name from an actual file to make sure we get the casing right
                if (installedPackage != null)
                {
                    foreach (string contentSegment in installedPackage.GetContentFiles().Select(x => new DirectoryPath(x.Path).Segments[0]).Distinct())
                    {
                        string installPath = packageManager.PathResolver.GetInstallPath(installedPackage);
                        _fileSystem.InputPaths.Insert(0, new DirectoryPath(installPath).Combine(contentSegment));
                    }
                }
            }
            */
        }

        /*
        private NuGetPackageManager GetPackageManager(IEnumerable<string> packageSources)
        {

            IPackageRepository packageRepository = new AggregateRepository(PackageRepositoryFactory.Default, packageSources, false);
            PackageManager packageManager = new PackageManager(packageRepository, AbsolutePackagesPath.FullPath)
            {
                Logger = _logger
            };
            return packageManager;
        }

        public IEnumerable<FilePath> GetCompatibleAssemblyPaths()
        {
            List<FilePath> assemblyPaths = new List<FilePath>();
            FrameworkReducer reducer = new FrameworkReducer();

            // TODO: If alternate versions of Wyam are developed (I.e., for DNX), this will need to be switched, or even better fetched from the current framework
            NuGetFramework targetFramework = new NuGetFramework(".NETFramework", Version.Parse("4.6"));

            // TODO: When we switch to the new v3 NuGet libraries, this will probably have to change since it doesn't copy all packages locally
            NuGetFrameworkFullComparer frameworkComparer = new NuGetFrameworkFullComparer();
            IPackageRepository packageRepository = PackageRepositoryFactory.Default.CreateRepository(AbsolutePackagesPath.FullPath);
            PackageManager packageManager = new PackageManager(packageRepository, AbsolutePackagesPath.FullPath);
            foreach (IPackage package in packageManager.LocalRepository.GetPackages())
            {
                // Get all packages along with their v3 framework
                List<KeyValuePair<IPackageFile, NuGetFramework>> filesAndFrameworks = package.GetLibFiles().Select(x => new KeyValuePair<IPackageFile, NuGetFramework>(x, x.TargetFramework == null ? null : new NuGetFramework(x.TargetFramework.Identifier, x.TargetFramework.Version, x.TargetFramework.Profile))).ToList();

                // Find the closest compatible framework
                NuGetFramework targetPackageFramework = reducer.GetNearest(targetFramework, filesAndFrameworks.Where(x => x.Value != null).Select(x => x.Value));

                // Restrict to compatible packages or those without a framework
                List<FilePath> packageAssemblyPaths = filesAndFrameworks.Where(x => x.Value == null || frameworkComparer.Equals(targetPackageFramework, x.Value)).Select(x => AbsolutePackagesPath.Combine(String.Format(CultureInfo.InvariantCulture, "{0}.{1}", package.Id, package.Version)).CombineFile(x.Key.Path)).Where(x => x.Extension == ".dll").ToList();

                // Add the assemblies from compatible packages
                foreach (FilePath packageAssemblyPath in packageAssemblyPaths)
                {
                    Trace.Verbose("Added assembly file {0} from package {1}.{2}", packageAssemblyPath.ToString(), package.Id, package.Version);
                }
                assemblyPaths.AddRange(packageAssemblyPaths);

                // Output a message if no assemblies were found in this package
                if (packageAssemblyPaths.Count == 0)
                {
                    Trace.Verbose("Could not find compatible framework for package {0}.{1} (this is normal for content-only packages)", package.Id, package.Version);
                }
            }
            return assemblyPaths;
        }
        */
    }
}
