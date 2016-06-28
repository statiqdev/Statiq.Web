using System;
using Wyam.Configuration.Preprocessing;

namespace Wyam.Configuration.Directives
{
    internal class AssemblyNameDirective : IDirective
    {
        public string Name => "assembly-name";

        public string ShortName => "an";

        public bool SupportsMultiple => true;

        public string Description => "Adds a reference to an assembly by name.";

        public void Process(Configurator configurator, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new Exception("Assembly name directive must have a value");
            }
            configurator.AssemblyLoader.AddName(value.Trim().Trim('"'));
        }

        public string GetHelpText() => null;
    }
}
