using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.IO;
using Wyam.Configuration.Preprocessing;

namespace Wyam.Configuration.NuGet
{
    internal class NuGetConfigDirective : ArgumentSyntaxDirective<NuGetConfigDirective.Settings>
    {
        public override IEnumerable<string> DirectiveNames { get; } = new[] { "nc", "nuget-config" };

        public class Settings
        {
            public DirectoryPath PackagesPath = null;
            public bool PackagesPathSpecified = false;
            public bool UseLocal = false;
            public bool UseLocalSpecified = false;
            public bool UpdatePackages = false;
            public bool UpdatePackagesSpecified = false;
        }

        protected override void Define(ArgumentSyntax syntax, Settings settings)
        {
            settings.UseLocalSpecified = syntax.DefineOption("use-local-packages", ref settings.UseLocal, "Toggles the use of a local NuGet packages folder.").IsSpecified;
            settings.UpdatePackagesSpecified = syntax.DefineOption("update-packages", ref settings.UpdatePackages, "Check the NuGet server for more recent versions of each package and update them if applicable.").IsSpecified;
            settings.PackagesPathSpecified = syntax.DefineParameter("packages-path", ref settings.PackagesPath, DirectoryPath.FromString, "The packages path to use (only if use-local is true).").IsSpecified;
        }

        protected override void Process(Configurator configurator, Settings settings)
        {
            if (settings.UseLocalSpecified)
            {
                configurator.PackageInstaller.UseLocal = settings.UseLocal;
            }
            if (settings.UpdatePackagesSpecified)
            {
                configurator.PackageInstaller.UpdatePackages = settings.UpdatePackages;
            }
            if (settings.PackagesPathSpecified)
            {
                configurator.PackageInstaller.PackagesPath = settings.PackagesPath;
            }

        }
    }
}
