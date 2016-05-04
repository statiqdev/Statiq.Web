using System;
using System.Collections.Generic;
using Wyam.Configuration.Preprocessing;

namespace Wyam.Configuration.NuGet
{
    internal class NuGetSourceDirective : IDirective
    {
        public IEnumerable<string> DirectiveNames { get; } = new[] { "ns", "nuget-source" };

        public void Process(Configurator configurator, string value)
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
                configurator.PackageInstaller.AddPackageSource(source);
            }
        }
    }
}