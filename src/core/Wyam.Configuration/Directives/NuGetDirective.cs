using System;
using System.Collections.Generic;
using System.CommandLine;

namespace Wyam.Configuration.Directives
{
    internal class NuGetDirective : ArgumentSyntaxDirective<NuGetDirective.Settings>
    {
        public override string Name => "nuget";

        public override string ShortName => "n";

        public override bool SupportsMultiple => true;

        public override string Description => "Adds a NuGet package (downloading and installing it if needed).";

        public override IEqualityComparer<string> ValueComparer => StringComparer.Ordinal;

        // Any changes to settings should also be made in Cake.Wyam
        public class Settings
        {
#pragma warning disable SA1401 // Fields should be private
            public bool Prerelease;
            public bool Unlisted;
            public bool Exclusive;
            public string VersionRange;
            public bool Latest;
            public IReadOnlyList<string> Sources;
            public string Package;
#pragma warning restore SA1401 // Fields should be private
        }

        protected override void Define(ArgumentSyntax syntax, Settings settings)
        {
            syntax.DefineOption("p|prerelease", ref settings.Prerelease, "Specifies that prerelease packages are allowed.");
            syntax.DefineOption("u|unlisted", ref settings.Unlisted, "Specifies that unlisted packages are allowed.");
            syntax.DefineOption("v|version", ref settings.VersionRange, "Specifies the version range of the package to use.");
            if (syntax.DefineOption("l|latest", ref settings.Latest, "Specifies that the latest available version of the package should be used (this will always trigger a request to the sources).").IsSpecified
                && !string.IsNullOrEmpty(settings.VersionRange))
            {
                syntax.ReportError("latest cannot be specified if a version range is specified.");
            }
            syntax.DefineOptionList("s|source", ref settings.Sources, "Specifies the package source(s) to get the package from.");
            if (syntax.DefineOption("e|exclusive", ref settings.Exclusive, "Indicates that only the specified package source(s) should be used to find the package.").IsSpecified
                && settings.Sources == null)
            {
                syntax.ReportError("exclusive can only be used if sources are specified.");
            }
            if (!syntax.DefineParameter("package", ref settings.Package, "The package to install.").IsSpecified)
            {
                syntax.ReportError("a package must be specified.");
            }
        }

        protected override void Process(Configurator configurator, Settings settings)
        {
            // Add the package to the repository (it'll actually get fetched later)
            configurator.PackageInstaller.AddPackage(
                settings.Package,
                settings.Sources,
                settings.VersionRange,
                settings.Latest,
                settings.Prerelease,
                settings.Unlisted,
                settings.Exclusive);
        }
    }
}