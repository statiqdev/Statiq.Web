using System;
using System.Collections.Generic;
using System.CommandLine;
using Wyam.Configuration.Preprocessing;

namespace Wyam.Configuration.Directives
{
    internal class RecipeDirective : ArgumentSyntaxDirective<RecipeDirective.Settings>
    {
        public override string Name => "recipe";

        public override string ShortName => "r";

        public override bool SupportsMultiple => false;

        public override string Description => "Specifies a recipe to use.";

        public override IEqualityComparer<string> ValueComparer => StringComparer.OrdinalIgnoreCase;

        // Any changes to settings should also be made in Cake.Wyam
        public class Settings
        {
#pragma warning disable SA1401 // Fields should be private
            public bool IgnoreKnownPackages;
            public string Recipe;
#pragma warning restore SA1401 // Fields should be private
        }

        protected override void Define(ArgumentSyntax syntax, Settings settings)
        {
            syntax.DefineOption("i|ignore-known-packages", ref settings.IgnoreKnownPackages, "Ignores (does not add) packages for known recipes.");
            if (!syntax.DefineParameter("recipe", ref settings.Recipe, "The recipe to use.").IsSpecified)
            {
                syntax.ReportError("a recipe must be specified.");
            }
        }

        protected override void Process(Configurator configurator, Settings settings)
        {
            configurator.RecipeName = settings.Recipe;
            configurator.IgnoreKnownRecipePackages = settings.IgnoreKnownPackages;
        }
    }
}
