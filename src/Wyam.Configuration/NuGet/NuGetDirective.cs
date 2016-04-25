using System;
using System.Collections.Generic;
using Wyam.Configuration.Preprocessing;

namespace Wyam.Configuration.NuGet
{
    internal class NuGetDirective : IDirective
    {
        private readonly PackageInstaller _packageInstaller;

        public NuGetDirective(PackageInstaller packageInstaller)
        {
            _packageInstaller = packageInstaller;
        }

        public void Process(string value)
        {
            bool prerelease = false;
            bool unlisted = false;
            bool exclusive = false;
            string version = null;
            IReadOnlyList<string> sources = null;
            IReadOnlyList<string> packages = null;

            // Parse the directive value
            IEnumerable<string> arguments = ArgumentSplitter.Split(value);
            System.CommandLine.ArgumentSyntax parsed = System.CommandLine.ArgumentSyntax.Parse(arguments, syntax =>
            {
                syntax.DefineOption("p|prerelease", ref prerelease, "Specifies that prerelease packages are allowed.");
                syntax.DefineOption("u|unlisted", ref unlisted, "Specifies that unlisted packages are allowed.");
                syntax.DefineOption("v|version", ref version, "Specifies the version specification to use for the package.");
                syntax.DefineOptionList("s|source", ref sources, "Specifies the package source(s) to get the package from.");
                if (syntax.DefineOption("e|exclusive", ref exclusive, "Indicates that only the specified package source(s) should be used to find the package.").IsSpecified
                    && sources == null)
                {
                    syntax.ReportError("exclusive can only be used if sources are specified.");
                }
                if (!syntax.DefineParameterList("package", ref packages, "The package(s) to install.").IsSpecified)
                {
                    syntax.ReportError("at least one package must be specified.");
                }
            });
            if (parsed.HasErrors)
            {
                throw new Exception(parsed.GetHelpText());
            }

            // Add the package to the repository (it'll actually get fetched later)
            foreach (string package in packages)
            {
                _packageInstaller.AddPackage(package, sources, version, prerelease, unlisted, exclusive);
            }
        }
    }
}