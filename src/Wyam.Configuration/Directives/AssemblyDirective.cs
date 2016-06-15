using System;
using Wyam.Configuration.Preprocessing;

namespace Wyam.Configuration.Directives
{
    internal class AssemblyDirective : IDirective
    {
        public string Name => "assembly";

        public string ShortName => "a";

        public bool SupportsMultiple => true;

        public string Description => "Adds a reference to an assembly by file name or globbing pattern.";

        public void Process(Configurator configurator, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new Exception("Assembly directive must have a value");
            }
            configurator.AssemblyLoader.AddPattern(value.Trim());
        }

        public string GetHelpText() => null;
    }
}
