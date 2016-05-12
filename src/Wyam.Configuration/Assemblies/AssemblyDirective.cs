using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Configuration.Preprocessing;

namespace Wyam.Configuration.Assemblies
{
    internal class AssemblyDirective : ArgumentSyntaxDirective<AssemblyDirective.Settings>
    {
        public override IEnumerable<string> DirectiveNames { get; } = new[] { "assembly", "a" };

        public override bool SupportsCli => true;

        public override string Description => "Adds a reference to an assembly by file name or globbing pattern.";

        // Any changes to settings should also be made in Cake.Wyam
        public class Settings
        {
            public string Assembly;
        }

        protected override void Define(ArgumentSyntax syntax, Settings settings)
        {
            if (!syntax.DefineParameter("assembly", ref settings.Assembly, "The assembly to load by file or globbing pattern.").IsSpecified)
            {
                syntax.ReportError("an assembly file or globbing pattern must be specified.");
            }
        }

        protected override void Process(Configurator configurator, Settings settings)
        {
            configurator.AssemblyLoader.AddPattern(settings.Assembly);
        }
    }
}
