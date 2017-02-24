using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ConcurrentCollections;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.ProjectManagement;
using NuGet.Protocol.Core.Types;
using Wyam.Common.IO;
using Wyam.Common.Tracing;
using Wyam.Common.Util;
using Wyam.Configuration.Assemblies;

namespace Wyam.Configuration.NuGet
{
    // This primarily exists to intercept package installations and store their paths
    internal class WyamFolderNuGetProject : FolderNuGetProject
    {
        private readonly IFileSystem _fileSystem;
        private readonly AssemblyLoader _assemblyLoader;
        private readonly NuGetFramework _currentFramework;
        private readonly InstalledPackagesCache _installedPackages;

        public WyamFolderNuGetProject(IFileSystem fileSystem, AssemblyLoader assemblyLoader, NuGetFramework currentFramework, InstalledPackagesCache installedPackages, string root)
            : base(root)
        {
            _fileSystem = fileSystem;
            _assemblyLoader = assemblyLoader;
            _currentFramework = currentFramework;
            _installedPackages = installedPackages;
        }

        // This gets called for every package install, including dependencies, and is our only chance to handle dependency PackageIdentity instances
        public override Task<bool> InstallPackageAsync(PackageIdentity packageIdentity, DownloadResourceResult downloadResourceResult,
            INuGetProjectContext nuGetProjectContext, CancellationToken token)
        {
            _installedPackages.AddPackage(packageIdentity, _currentFramework);
            Trace.Verbose($"Installing package or dependency {packageIdentity.Id} {(packageIdentity.HasVersion ? packageIdentity.Version.ToNormalizedString() : string.Empty)}");
            return base.InstallPackageAsync(packageIdentity, downloadResourceResult, nuGetProjectContext, token);
        }

        public void ProcessAssembliesAndContent()
        {
            List<DirectoryPath> contentPaths = _installedPackages
                .GetInstalledPackagesAndDependencies()
                .Distinct()
                .AsParallel()
                .SelectMany(packageIdentity =>
                {
                    DirectoryPath installedPath = new DirectoryPath(GetInstalledPath(packageIdentity));
                    string packageFilePath = GetInstalledPackageFilePath(packageIdentity);
                    PackageArchiveReader archiveReader = new PackageArchiveReader(packageFilePath, null, null);
                    AddReferencedAssemblies(installedPath, archiveReader);
                    return GetContentDirectories(installedPath, archiveReader);
                })
                .ToList();
            foreach (DirectoryPath contentPath in contentPaths)
            {
                _fileSystem.InputPaths.Insert(0, contentPath);
                Trace.Verbose($"Added content path {contentPath} to included paths");
            }
        }

        // Add all reference items to the assembly list
        private void AddReferencedAssemblies(DirectoryPath installedPath, PackageArchiveReader archiveReader)
        {
            FrameworkSpecificGroup referenceGroup = GetMostCompatibleGroup(_currentFramework, archiveReader.GetReferenceItems().ToList());
            if (referenceGroup != null)
            {
                foreach (FilePath itemPath in referenceGroup.Items
                    .Select(x => new FilePath(x))
                    .Where(x => x.FileName.Extension == ".dll" || x.FileName.Extension == ".exe"))
                {
                    FilePath assemblyPath = installedPath.CombineFile(itemPath);
                    _assemblyLoader.Add(assemblyPath.FullPath);
                    Trace.Verbose($"Added NuGet reference {assemblyPath} for loading");
                }
            }
        }

        // Add content directories to the input paths
        private IEnumerable<DirectoryPath> GetContentDirectories(DirectoryPath installedPath, PackageArchiveReader archiveReader)
        {
            FrameworkSpecificGroup contentGroup = GetMostCompatibleGroup(_currentFramework, archiveReader.GetContentItems().ToList());
            if (contentGroup != null)
            {
                // We need to use the directory name from an actual file to make sure we get the casing right
                foreach (string contentSegment in contentGroup.Items
                    .Select(x => new FilePath(x).Segments[0])
                    .Distinct())
                {
                    yield return installedPath.Combine(contentSegment);
                }
            }
        }

        // Probably going to hell for using a region
        // The following methods are originally from the internal MSBuildNuGetProjectSystemUtility class
        #region MSBuildNuGetProjectSystemUtility

        private static FrameworkSpecificGroup GetMostCompatibleGroup(NuGetFramework projectTargetFramework,
            IEnumerable<FrameworkSpecificGroup> itemGroups)
        {
            var reducer = new FrameworkReducer();
            var mostCompatibleFramework
                = reducer.GetNearest(projectTargetFramework, itemGroups.Select(i => i.TargetFramework));
            if (mostCompatibleFramework != null)
            {
                var mostCompatibleGroup
                    = itemGroups.FirstOrDefault(i => i.TargetFramework.Equals(mostCompatibleFramework));

                if (IsValid(mostCompatibleGroup))
                {
                    return mostCompatibleGroup;
                }
            }

            return null;
        }

        private static bool IsValid(FrameworkSpecificGroup frameworkSpecificGroup)
        {
            if (frameworkSpecificGroup != null)
            {
                return (frameworkSpecificGroup.HasEmptyFolder
                     || frameworkSpecificGroup.Items.Any()
                     || !frameworkSpecificGroup.TargetFramework.Equals(NuGetFramework.AnyFramework));
            }

            return false;
        }

        #endregion MSBuildNuGetProjectSystemUtility
    }
}
