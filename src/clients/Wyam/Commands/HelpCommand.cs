using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Configuration.Preprocessing;

namespace Wyam.Commands
{
    internal class HelpCommand : Command
    {
        private bool _directives;

        public override string Description => "Displays various help messages.";

        protected override bool GlobalArguments => false;

        protected override void ParseOptions(ArgumentSyntax syntax)
        {
            syntax.DefineOption("directives", ref _directives, "Displays help for all preprocessor directives.");
        }

        protected override ExitCode RunCommand(Preprocessor preprocessor)
        {
            // Directives
            if (_directives)
            {
                Console.WriteLine("Available preprocessor directives:");
                foreach (IDirective directive in preprocessor.Directives)
                {
                    Console.WriteLine();
                    Console.WriteLine("#" + directive.Name + (string.IsNullOrEmpty(directive.ShortName) ? string.Empty : ", #" + directive.ShortName));
                    string helpText = directive.GetHelpText();
                    if (!string.IsNullOrEmpty(helpText))
                    {
                        Console.WriteLine(helpText);
                    }
                    Console.WriteLine($"{directive.Description}");
                }
            }

            return ExitCode.Normal;
        }
    }
}
