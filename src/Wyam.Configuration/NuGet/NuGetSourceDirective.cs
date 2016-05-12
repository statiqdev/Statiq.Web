using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NuGet.Protocol.Core.v3;
using Wyam.Configuration.Preprocessing;
using ArgumentSyntax = System.CommandLine.ArgumentSyntax;

namespace Wyam.Configuration.NuGet
{
    internal class NuGetSourceDirective : ArgumentSyntaxDirective<NuGetSourceDirective.Settings>
    {
        public override IEnumerable<string> DirectiveNames { get; } = new[] { "nuget-source", "ns" };

        public override bool SupportsCli => true;

        public override string Description => "Specifies an additional package source to use.";

        // Any changes to settings should also be made in Cake.Wyam
        public class Settings
        {
            public string Source;
        }

        protected override void Define(ArgumentSyntax syntax, Settings settings)
        {
            if (!syntax.DefineParameter("source", ref settings.Source, "The package source to add.").IsSpecified)
            {
                syntax.ReportError("a package source must be specified");
            }
        }

        protected override void Process(Configurator configurator, Settings settings)
        {
            configurator.PackageInstaller.AddPackageSource(settings.Source);
        }
    }
}