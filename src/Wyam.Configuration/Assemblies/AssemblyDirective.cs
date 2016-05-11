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
        public override IEnumerable<string> DirectiveNames { get; } = new[] { "a", "assembly" };

        public class Settings
        {
            public IReadOnlyList<string> Assemblies = null;
        }

        protected override void Define(ArgumentSyntax syntax, Settings settings)
        {
            if (!syntax.DefineParameterList("assemblies", ref settings.Assemblies, "The assemblies to load by file or globbing pattern.").IsSpecified)
            {
                syntax.ReportError("at least one assembly file or globbing pattern must be specified.");
            }
        }

        protected override void Process(Configurator configurator, Settings settings)
        {
            foreach (string assembly in settings.Assemblies)
            {
                configurator.AssemblyLoader.AddPattern(assembly);
            }
        }
    }
}
