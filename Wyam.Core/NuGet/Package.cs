using System;
using System.IO;
using System.Linq;
using NuGet;

namespace Wyam.Core.NuGet
{
    internal class Package
    {
        private readonly string _packageId;
        private readonly IVersionSpec _versionSpec;
        private readonly bool _allowPrereleaseVersions;
        private readonly bool _allowUnlisted;

        // The version string is either a simple version or an arithmetic range
        // e.g.
        //      1.0         --> 1.0 ≤ x
        //      (,1.0]      --> x ≤ 1.0
        //      (,1.0)      --> x &lt; 1.0
        //      [1.0]       --> x == 1.0
        //      (1.0,)      --> 1.0 &lt; x
        //      (1.0, 2.0)   --> 1.0 &lt; x &lt; 2.0
        //      [1.0, 2.0]   --> 1.0 ≤ x ≤ 2.0
        public Package(string packageId, string versionSpec = null, bool allowPrereleaseVersions = false, bool allowUnlisted = false)
        {
            if (packageId == null)
            {
                throw new ArgumentNullException("packageId");
            }
            if (string.IsNullOrWhiteSpace(packageId))
            {
                throw new ArgumentException("packageId");
            }

            _packageId = packageId;
            _versionSpec = string.IsNullOrWhiteSpace(versionSpec) ? null : VersionUtility.ParseVersionSpec(versionSpec);
            _allowPrereleaseVersions = allowPrereleaseVersions;
            _allowUnlisted = allowUnlisted;
        }

        public void InstallPackage(PackageManager packageManager, Engine engine)
        {
            engine.Trace.Verbose("Installing package {0}{1}...", 
                _packageId, _versionSpec == null ? string.Empty : " " + _versionSpec);

            // Find the local package
            IPackage localPackage = packageManager.LocalRepository.FindPackage(_packageId);

            // Find the source package
            IPackage sourcePackage = packageManager.SourceRepository
                .FindPackage(_packageId, _versionSpec, _allowPrereleaseVersions, _allowUnlisted);
            if (sourcePackage == null)
            {
                engine.Trace.Warning("Package {0} {1} could not be found.", 
                    _packageId, _versionSpec == null ? string.Empty : " " + _versionSpec);
                return;
            }

            // Check if we're up to date
            if (localPackage != null && _versionSpec.Satisfies(localPackage.Version) && localPackage.Version >= sourcePackage.Version)
            {
                engine.Trace.Verbose("Package {0}{1} is satisfied by version {2}, skipping.",
                    _packageId, _versionSpec == null ? string.Empty : " " + _versionSpec, localPackage.Version);
                return;
            }

            // Uninstall the old package (removing any content files)
            if (localPackage != null)
            {
                int indent = engine.Trace.Indent();
                engine.Trace.Verbose("Uninstalling package {0} {1}.", localPackage.Id, localPackage.Version);
                foreach (IPackageFile packageFile in localPackage.GetContentFiles())
                {
                    string filePath = Path.Combine(engine.InputFolder, packageFile.EffectivePath);
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }
                packageManager.UninstallPackage(localPackage);
                engine.Trace.IndentLevel = indent;
            }
            
            // Install it
            packageManager.InstallPackage(sourcePackage, false, _allowPrereleaseVersions);

            // Copy content files
            foreach (IPackageFile packageFile in sourcePackage.GetContentFiles())
            {
                string filePath = Path.Combine(engine.InputFolder, packageFile.EffectivePath);
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                using (var fileStream = File.Create(filePath))
                {
                    packageFile.GetStream().CopyTo(fileStream);
                }
            }

            engine.Trace.Verbose("Installed package {0} {1}.", sourcePackage.Id, sourcePackage.Version);
        }
    }
}
