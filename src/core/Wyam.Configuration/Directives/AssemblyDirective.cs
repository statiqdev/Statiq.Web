using System;
using System.Collections.Generic;
using Wyam.Configuration.Preprocessing;

namespace Wyam.Configuration.Directives
{
    internal class AssemblyDirective : IDirective
    {
        public string Name => "assembly";

        public string ShortName => "a";

        public bool SupportsMultiple => true;

        public string Description => "Adds an assembly reference by name, file name, or globbing pattern.";

        public IEqualityComparer<string> ValueComparer => StringComparer.Ordinal;

        public void Process(Configurator configurator, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new Exception("Assembly directive must have a value");
            }
            configurator.AssemblyLoader.Add(value.Trim().Trim('"'));
        }

        public string GetHelpText() => null;
    }
}
