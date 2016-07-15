using System;
using Wyam.Configuration.Preprocessing;

namespace Wyam.Configuration.Directives
{
    internal class AssembliesDirective : IDirective
    {
        public string Name => "assemblies";

        public string ShortName => "a";

        public bool SupportsMultiple => true;

        public string Description => "Adds references to multiple assemblies using a globbing pattern.";

        public void Process(Configurator configurator, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new Exception("Assemblies directive must have a value");
            }
            configurator.AssemblyLoader.AddPattern(value.Trim().Trim('"'));
        }

        public string GetHelpText() => null;
    }
}
