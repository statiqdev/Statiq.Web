using System;
using System.Collections.Generic;
using System.CommandLine;
using Wyam.Configuration.Preprocessing;

namespace Wyam.Configuration.NuGet
{
    internal class NuGetDirective : ArgumentSyntaxDirective<NuGetDirective.Settings>
    {
        public override IEnumerable<string> DirectiveNames { get; } = new[] { "nuget", "n" };

        public override bool SupportsCli => true;
        
        public override string Description => "Adds a NuGet package (downloading and installing it if needed).";

        // Any changes to settings should also be made in Cake.Wyam
        public class Settings
        {
            public bool Prerelease;
            public bool Unlisted;
            public bool Exclusive;
            public string Version;
            public IReadOnlyList<string> Sources;
            public string Package;
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
            if (!syntax.DefineParameter("package", ref settings.Package, "The package to install.").IsSpecified)
            {
                syntax.ReportError("a package must be specified.");
            }
        }

        protected override void Process(Configurator configurator, Settings settings)
        {
            // Add the package to the repository (it'll actually get fetched later)
            configurator.PackageInstaller.AddPackage(settings.Package, 
                settings.Sources, settings.Version, settings.Prerelease, settings.Unlisted, settings.Exclusive);
        }
    }
}