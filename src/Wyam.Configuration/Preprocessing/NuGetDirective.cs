using System;
using System.Collections.Generic;
using Wyam.NuGet;

namespace Wyam.Configuration.Preprocessing
{
    internal class NuGetDirective : IDirective
    {
        private readonly PackagesCollection _packagesCollection;

        public NuGetDirective(PackagesCollection packagesCollection)
        {
            _packagesCollection = packagesCollection;
        }

        public void Process(string value)
        {
            bool prerelease = false;
            bool unlisted = false;
            string version = null;
            string repository = null;
            string package = null;

            // Parse the directive value
            IEnumerable<string> arguments = ArgumentSplitter.Split(value);
            System.CommandLine.ArgumentSyntax parsed = System.CommandLine.ArgumentSyntax.Parse(arguments, syntax =>
            {
                syntax.DefineOption("p|prerelease", ref prerelease, "Specifies that prerelease packages are allowed.");
                syntax.DefineOption("u|unlisted", ref unlisted, "Specifies that unlisted packages are allowed.");
                syntax.DefineOption("v|version", ref version, "Specifies the version specification to use for the package.");
                syntax.DefineOption("r|repository", ref repository, "Specifies the repository to get the package from.");
                if (!syntax.DefineParameter("package", ref package, "The package to fetch.").IsSpecified)
                {
                    syntax.ReportError("package must be specified");
                }
            });
            if (parsed.HasErrors)
            {
                throw new Exception(parsed.GetHelpText());
            }

            // Add the package to the repository (it'll actually get fetched later)
            IRepository repo = repository == null 
                ? (IRepository)_packagesCollection 
                : _packagesCollection.GetRepository(repository);
            repo.AddPackage(package, version, prerelease, unlisted);
        }
    }
}