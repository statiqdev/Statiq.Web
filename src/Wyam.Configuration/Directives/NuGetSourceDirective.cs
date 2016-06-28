using System;
using Wyam.Configuration.Preprocessing;

namespace Wyam.Configuration.Directives
{
    internal class NuGetSourceDirective : IDirective
    {
        public string Name => "nuget-source";

        public string ShortName => "ns";

        public bool SupportsMultiple => true;

        public string Description => "Specifies an additional package source to use.";

        public void Process(Configurator configurator, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new Exception("NuGet source directive must have a value");
            }
            configurator.PackageInstaller.AddPackageSource(value.Trim().Trim('"'));
        }

        public string GetHelpText() => null;
    }
}