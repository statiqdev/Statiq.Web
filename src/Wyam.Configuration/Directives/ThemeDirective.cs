using System;
using System.CommandLine;
using Wyam.Configuration.Preprocessing;

namespace Wyam.Configuration.Directives
{
    internal class ThemeDirective : ArgumentSyntaxDirective<ThemeDirective.Settings>
    {
        public override string Name => "theme";

        public override string ShortName => "t";

        public override bool SupportsMultiple => false;

        public override string Description => "Specifies a theme to use.";

        // Any changes to settings should also be made in Cake.Wyam
        public class Settings
        {
            public bool IgnoreKnownPackages;
            public string Theme;
        }

        protected override void Define(ArgumentSyntax syntax, Settings settings)
        {
            syntax.DefineOption("i|ignore-known-packages", ref settings.IgnoreKnownPackages, "Ignores (does not add) packages for known themes.");
            if (!syntax.DefineParameter("theme", ref settings.Theme, "The theme to use.").IsSpecified)
            {
                syntax.ReportError("a theme must be specified.");
            }
        }

        protected override void Process(Configurator configurator, Settings settings)
        {
            configurator.Theme = settings.Theme;
            configurator.IgnoreKnownThemePackages = settings.IgnoreKnownPackages;
        }
    }
}
