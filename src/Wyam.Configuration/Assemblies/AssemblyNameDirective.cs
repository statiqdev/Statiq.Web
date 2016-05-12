using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Configuration.Preprocessing;

namespace Wyam.Configuration.Assemblies
{
    internal class AssemblyNameDirective : ArgumentSyntaxDirective<AssemblyNameDirective.Settings>
    {
        public override IEnumerable<string> DirectiveNames { get; } = new[] { "assembly-name", "an" };

        public override bool SupportsCli => true;

        public override string Description => "Adds a reference to an assembly by name.";

        // Any changes to settings should also be made in Cake.Wyam
        public class Settings
        {
            public string Assembly;
        }

        protected override void Define(ArgumentSyntax syntax, Settings settings)
        {
            if (!syntax.DefineParameter("assembly", ref settings.Assembly, "The assembly to load by name.").IsSpecified)
            {
                syntax.ReportError("an assembly name must be specified.");
            }
        }

        protected override void Process(Configurator configurator, Settings settings)
        {
            configurator.AssemblyLoader.AddName(settings.Assembly);
        }
    }
}
