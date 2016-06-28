using System;
using Wyam.Configuration.Preprocessing;

namespace Wyam.Configuration.Directives
{
    internal class ThemeDirective : IDirective
    {
        public string Name => "theme";

        public string ShortName => "t";

        public bool SupportsMultiple => false;

        public string Description => "Specifies a theme to use.";

        public void Process(Configurator configurator, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new Exception("Theme directive must have a value");
            }
            configurator.Theme = value.Trim().Trim('"');
        }

        public string GetHelpText() => null;
    }
}
