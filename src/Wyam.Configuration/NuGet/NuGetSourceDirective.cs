using System;
using System.Collections.Generic;
using Wyam.Configuration.Preprocessing;

namespace Wyam.Configuration.NuGet
{
    internal class NuGetSourceDirective : IDirective
    {
        private readonly PackageInstaller _packageInstaller;

        public NuGetSourceDirective(PackageInstaller packageInstaller)
        {
            _packageInstaller = packageInstaller;
        }

        public void Process(string value)
        {
            IReadOnlyList<string> sources = null;

            // Parse the directive value
            IEnumerable<string> arguments = ArgumentSplitter.Split(value);
            System.CommandLine.ArgumentSyntax parsed = System.CommandLine.ArgumentSyntax.Parse(arguments, syntax =>
            {
                if (!syntax.DefineParameterList("sources", ref sources, "The package source(s) to add.").IsSpecified)
                {
                    syntax.ReportError("package source(s) must be specified");
                }
            });
            if (parsed.HasErrors)
            {
                throw new Exception(parsed.GetHelpText());
            }

            foreach (string source in sources)
            {
                _packageInstaller.AddPackageSource(source);
            }
        }
    }
}