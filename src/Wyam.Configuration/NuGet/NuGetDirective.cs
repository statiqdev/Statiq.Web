using System;
using System.Collections.Generic;
using System.CommandLine;
using Wyam.Configuration.Preprocessing;

namespace Wyam.Configuration.NuGet
{
    internal class NuGetDirective : ArgumentSyntaxDirective<NuGetDirective.Settings>
    {
        public override IEnumerable<string> DirectiveNames { get; } = new[] { "n", "nuget" };

        public class Settings
        {
            public bool Prerelease = false;
            public bool Unlisted = false;
            public bool Exclusive = false;
            public string Version = null;
            public IReadOnlyList<string> Sources = null;
            public IReadOnlyList<string> Packages = null;
        }

        protected override void Define(ArgumentSyntax syntax, Settings settings)
        {
            syntax.DefineOption("p|prerelease", ref settings.Prerelease, "Specifies that prerelease packages are allowed.");
            syntax.DefineOption("u|unlisted", ref settings.Unlisted, "Specifies that unlisted packages are allowed.");
            syntax.DefineOption("v|version", ref settings.Version, "Specifies the version of the package to use.");
            syntax.DefineOptionList("s|source", ref settings.Sources, "Specifies the package source(s) to get the package from.");
            if (syntax.DefineOption("e|exclusive", ref settings.Exclusive, "Indicates that only the specified package source(s) should be used to find the package.").IsSpecified
                && settings.Sources == null)
            {
                syntax.ReportError("exclusive can only be used if sources are specified.");
            }
            if (!syntax.DefineParameterList("package", ref settings.Packages, "The package(s) to install.").IsSpecified)
            {
                syntax.ReportError("at least one package must be specified.");
            }
        }

        protected override void Process(Configurator configurator, Settings settings)
        {
            // Add the package to the repository (it'll actually get fetched later)
            foreach (string package in settings.Packages)
            {
                configurator.PackageInstaller.AddPackage(package, 
                    settings.Sources, settings.Version, settings.Prerelease, settings.Unlisted, settings.Exclusive);
            }
        }
    }
}