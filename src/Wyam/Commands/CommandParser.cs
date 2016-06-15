using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using Wyam.Configuration.Preprocessing;

namespace Wyam.Commands
{
    internal static class CommandParser
    {
        // Any changes to commands should also be made in Cake.Wyam
        private static readonly Command[] Commands =
        {
            new BuildCommand(),
            new HelpCommand()
        };

        /// <summary>
        /// Parses the command line arguments.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="preprocessor">The preprocessor.</param>
        /// <param name="hasErrors">If set to <c>true</c>, the command line had errors.</param>
        /// <returns>The resulting command.</returns>
        public static Command Parse(string[] args, Preprocessor preprocessor, out bool hasErrors)
        {
            // Construct a mapping of command names to commands
            List<Tuple<string, Command>> commands = Commands
                .Select(x =>
                {
                    string commandName = x.GetType().Name.ToLowerInvariant();
                    commandName = commandName.EndsWith("command")
                        ? commandName.Substring(0, commandName.Length - 7)
                        : commandName;
                    return Tuple.Create(commandName, x);
                })
                .ToList();

            // If the first argument is not a valid command, set it to the first command as a default
            // Make sure to allow the default help flags to handle help output
            if (args == null || args.Length == 0)
            {
                args = new[] {commands[0].Item1};
            }
            else if (args[0] != "-?" && args[0] != "-h" && args[0] != "--help"
                && commands.All(x => x.Item1 != args[0]))
            {
                args = new[] { commands[0].Item1 }.Concat(args).ToArray();
            }
            else if (args.Length == 1 && args[0] == "help")
            {
                // Special case for the help command without any additional arguments, output global help instead
                args = new[] {"--help"};
            }

            // Parse the command line arguments
            Command command = null;
            ArgumentSyntax parsed = ArgumentSyntax.Parse(args, syntax =>
            {
                // Add all commands
                foreach (Tuple<string, Command> cmd in commands)
                {
                    syntax.DefineCommand(cmd.Item1, ref command, cmd.Item2, cmd.Item2.Description);
                    cmd.Item2.Parse(syntax, preprocessor);
                }
            });

            hasErrors = parsed.HasErrors;

            // Output help text for any directive that got used for this command
            if (parsed.IsHelpRequested() && command?.SupportedDirectives != null)
            {
                foreach (IDirective directive in preprocessor.Directives
                    .Where(x => command.SupportedDirectives.Contains(x.Name, StringComparer.OrdinalIgnoreCase)))
                {
                    string helpText = directive.GetHelpText();
                    if (!string.IsNullOrEmpty(helpText))
                    {
                        Console.WriteLine($"--{directive.Name} usage:");
                        Console.WriteLine();
                        Console.WriteLine(directive.GetHelpText());
                        Console.WriteLine();
                    }
                }
            }

            return parsed.IsHelpRequested() || hasErrors ? null : command;
        }
    }
}