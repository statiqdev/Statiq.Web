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

            
           
            // TODO: The line below doesn't work because GetInstalledPackagesAsync() doesn't actually get packages on disk
            List<PackageReference> postInstalledPackages = PackageManager.PackagesFolderNuGetProject
                .GetInstalledPackagesAsync(CancellationToken.None).Result.ToList();

            // TODO: Once we have the packages, need to add assemblies and add include paths for content


            
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
