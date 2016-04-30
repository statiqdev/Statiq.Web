using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.ProjectManagement;
using NuGet.Protocol.Core.Types;
using Wyam.Common.IO;
using Wyam.Common.Tracing;

namespace Wyam.Configuration.NuGet
{
    // This primarily exists to intercept package installations and store their paths
    internal class WyamFolderNuGetProject : FolderNuGetProject
    {
        private readonly PackageInstaller _installer;
        private readonly AssemblyLoader _assemblyLoader;

        public WyamFolderNuGetProject(PackageInstaller installer, AssemblyLoader assemblyLoader, string root) : base(root)
        {
            _installer = installer;
            _assemblyLoader = assemblyLoader;
        }

        public override Task<bool> InstallPackageAsync(PackageIdentity packageIdentity, DownloadResourceResult downloadResourceResult,
            INuGetProjectContext nuGetProjectContext, CancellationToken token)
        {
            return base
                .InstallPackageAsync(packageIdentity, downloadResourceResult, nuGetProjectContext, token)
                .ContinueWith(x => ProcessAssembliesAndContent(x, packageIdentity), token);
        }

        private bool ProcessAssembliesAndContent(Task<bool> antecedent, PackageIdentity packageIdentity)
        {
            DirectoryPath installedPath = new DirectoryPath(GetInstalledPath(packageIdentity));
            string packageFilePath = GetInstalledPackageFilePath(packageIdentity);
            PackageArchiveReader archiveReader = new PackageArchiveReader(packageFilePath, null, null);
            List<FrameworkSpecificGroup> referenceItems = archiveReader.GetReferenceItems().ToList();

            // Reduce to the most compatible framework (see MSBuildNuGetProjectSystemUtility)
            // Add all reference items to the assembly list
            FrameworkReducer reducer = new FrameworkReducer();
            NuGetFramework mostCompatibleFramework = reducer.GetNearest(_installer.CurrentFramework,
                referenceItems.Select(x => x.TargetFramework));
            if (mostCompatibleFramework != null)
            {
                FrameworkSpecificGroup mostCompatibleGroup =
                    referenceItems.FirstOrDefault(x => x.TargetFramework.Equals(mostCompatibleFramework));
                if (IsValid(mostCompatibleGroup))
                {
                    foreach (FilePath itemPath in mostCompatibleGroup.Items
                        .Select(x => new FilePath(x))
                        .Where(x => x.FileName.Extension == ".dll" || x.FileName.Extension == ".exe"))
                    {
                        FilePath assemblyPath = installedPath.CombineFile(itemPath);
                        _assemblyLoader.AddFile(assemblyPath);
                        Trace.Verbose($"Added NuGet reference {assemblyPath} for loading");
                    }
                }
            }

            // TODO: Add content directories to the include paths here

            return antecedent.Result;
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
    }
}
