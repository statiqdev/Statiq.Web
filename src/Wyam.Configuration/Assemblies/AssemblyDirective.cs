using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Configuration.Preprocessing;

namespace Wyam.Configuration.Assemblies
{
    internal class AssemblyDirective : IDirective
    {
        public IEnumerable<string> DirectiveNames { get; } = new [] { "a", "assembly" };

        public void Process(Configurator configurator, string value)
        {
            IReadOnlyList<string> assemblies = null;

            // Parse the directive value
            IEnumerable<string> arguments = ArgumentSplitter.Split(value);
            System.CommandLine.ArgumentSyntax parsed = System.CommandLine.ArgumentSyntax.Parse(arguments, syntax =>
            {
                if (!syntax.DefineParameterList("assemblies", ref assemblies, "The assemblies to load by file or globbing pattern.").IsSpecified)
                {
                    syntax.ReportError("at least one assembly file or globbing pattern must be specified.");
                }
            });
            if (parsed.HasErrors)
            {
                throw new Exception(parsed.GetHelpText());
            }

            // Add the assemblies
            foreach (string assembly in assemblies)
            {
                configurator.AssemblyLoader.AddPattern(assembly);
            }
        }
    }
}
