using System;
using Wyam.Configuration.Preprocessing;

namespace Wyam.Configuration.Directives
{
    internal class ReferenceDirective : IDirective
    {
        public string Name => "reference";

        public string ShortName => "r";

        public bool SupportsMultiple => true;

        public string Description => "Adds an assembly reference.";

        public void Process(Configurator configurator, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new Exception("Reference directive must have a value");
            }
            configurator.AssemblyLoader.AddReference(value.Trim().Trim('"'));
        }

        public string GetHelpText() => null;
    }
}
