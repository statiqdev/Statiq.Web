using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NuGet.Protocol.Core.v3;
using Wyam.Configuration.Preprocessing;
using ArgumentSyntax = System.CommandLine.ArgumentSyntax;

namespace Wyam.Configuration.NuGet
{
    internal class NuGetSourceDirective : ArgumentSyntaxDirective<NuGetSourceDirective.Settings>
    {
        public override IEnumerable<string> DirectiveNames { get; } = new[] { "ns", "nuget-source" };

        public class Settings
        {
            public IReadOnlyList<string> Sources = null;
        }

        protected override void Define(ArgumentSyntax syntax, Settings settings)
        {
            if (!syntax.DefineParameterList("sources", ref settings.Sources, "The package source(s) to add.").IsSpecified)
            {
                syntax.ReportError("package source(s) must be specified");
            }
        }

        protected override void Process(Configurator configurator, Settings settings)
        {
            foreach (string source in settings.Sources)
            {
                configurator.PackageInstaller.AddPackageSource(source);
            }
        }
    }
}