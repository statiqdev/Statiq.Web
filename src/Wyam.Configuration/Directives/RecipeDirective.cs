using System;
using Wyam.Configuration.Preprocessing;

namespace Wyam.Configuration.Directives
{
    internal class RecipeDirective : IDirective
    {
        public string Name => "recipe";

        public string ShortName => "r";

        public bool SupportsMultiple => false;

        public string Description => "Specifies a recipe to use.";

        public void Process(Configurator configurator, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new Exception("Recipe directive must have a value");
            }
            configurator.Recipe = value.Trim();
        }

        public string GetHelpText() => null;
    }
}
