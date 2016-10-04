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
        private readonly FrameworkReducer _reducer = new FrameworkReducer();
        private readonly ConcurrentHashSet<PackageIdentity> _packageIdentities = new ConcurrentHashSet<PackageIdentity>();
        private readonly IFileSystem _fileSystem;
        private readonly AssemblyLoader _assemblyLoader;
        private readonly NuGetFramework _currentFramework;

        public WyamFolderNuGetProject(IFileSystem fileSystem, AssemblyLoader assemblyLoader, NuGetFramework currentFramework, string root) : base(root)
        {
            _fileSystem = fileSystem;
            _assemblyLoader = assemblyLoader;
            _currentFramework = currentFramework;
        }

        // This gets called for every package install, including dependencies, and is our only chance to handle dependency PackageIdentity instances
        public override Task<bool> InstallPackageAsync(PackageIdentity packageIdentity, DownloadResourceResult downloadResourceResult,
            INuGetProjectContext nuGetProjectContext, CancellationToken token)
        {
            _packageIdentities.Add(packageIdentity);
            return base.InstallPackageAsync(packageIdentity, downloadResourceResult, nuGetProjectContext, token);
        }

        public void ProcessAssembliesAndContent()
        {
            Parallel.ForEach(_packageIdentities, packageIdentity =>
            {
                DirectoryPath installedPath = new DirectoryPath(GetInstalledPath(packageIdentity));
                string packageFilePath = GetInstalledPackageFilePath(packageIdentity);
                PackageArchiveReader archiveReader = new PackageArchiveReader(packageFilePath, null, null);
                AddReferencedAssemblies(installedPath, archiveReader);
                IncludeContentDirectories(installedPath, archiveReader);
                Trace.Verbose($"Finished processing NuGet package at {installedPath}");
            });
        }

        // Add all reference items to the assembly list
        private void AddReferencedAssemblies(DirectoryPath installedPath, PackageArchiveReader archiveReader)
        {
            FrameworkSpecificGroup referenceGroup = GetMostCompatibleGroup(_reducer,
                _currentFramework, archiveReader.GetReferenceItems().ToList());
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
        private void IncludeContentDirectories(DirectoryPath installedPath, PackageArchiveReader archiveReader)
        {
            FrameworkSpecificGroup contentGroup = GetMostCompatibleGroup(_reducer,
                _currentFramework, archiveReader.GetContentItems().ToList());
            if (contentGroup != null)
            {
                // We need to use the directory name from an actual file to make sure we get the casing right
                foreach (string contentSegment in contentGroup.Items
                    .Select(x => new FilePath(x).Segments[0])
                    .Distinct())
                {
                    DirectoryPath contentPath = installedPath.Combine(contentSegment);
                    _fileSystem.InputPaths.Insert(0, contentPath);
                    Trace.Verbose($"Added content path {contentPath} to included paths");
                }
            }
        }

        // Probably going to hell for using a region
        // The following methods are originally from the internal MSBuildNuGetProjectSystemUtility class
        #region MSBuildNuGetProjectSystemUtility  

        private static FrameworkSpecificGroup GetMostCompatibleGroup(FrameworkReducer reducer, NuGetFramework projectTargetFramework,
            ICollection<FrameworkSpecificGroup> itemGroups)
        {
            NuGetFramework mostCompatibleFramework
                = reducer.GetNearest(projectTargetFramework, itemGroups.Select(i => i.TargetFramework));
            if (mostCompatibleFramework != null)
            {
                FrameworkSpecificGroup mostCompatibleGroup = itemGroups
                    .FirstOrDefault(i => i.TargetFramework.Equals(mostCompatibleFramework));

                if (IsValid(mostCompatibleGroup))
                {
                    // Normalize() is called outside GetMostCompatibleGroup() in MSBuildNuGetProjectSystemUtility but I combined it
                    return Normalize(mostCompatibleGroup);
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

        private static FrameworkSpecificGroup Normalize(FrameworkSpecificGroup group)
        {
            // Default to returning the same group
            FrameworkSpecificGroup result = group;

            // If the group is null or it does not contain any items besides _._ then this is a no-op.
            // If it does have items create a new normalized group to replace it with.
            if (group?.Items.Any() == true)
            {
                // Filter out invalid files
                IEnumerable<string> normalizedItems = GetValidPackageItems(group.Items)
                    .Select(PathUtility.ReplaceAltDirSeparatorWithDirSeparator);

                // Create a new group
                result = new FrameworkSpecificGroup(group.TargetFramework, normalizedItems);
            }

            return result;
        }

        private static IEnumerable<string> GetValidPackageItems(IEnumerable<string> items)
        {
            // Assume nupkg and nuspec as the save mode for identifying valid package files
            return items?.Where(i => PackageHelper.IsPackageFile(i, PackageSaveMode.Defaultv3)) ?? Enumerable.Empty<string>();
        }

        #endregion MSBuildNuGetProjectSystemUtility
    }
}
