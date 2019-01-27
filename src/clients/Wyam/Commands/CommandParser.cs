using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using Wyam.Common.Tracing;
using Wyam.Configuration.Preprocessing;

namespace Wyam.Commands
{
    internal static class CommandParser
    {
        // Any changes to commands should also be made in Cake.Wyam
        private static readonly Command[] Commands =
        {
            new BuildCommand(),
            new NewCommand(),
            new PreviewCommand(),
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
            List<string> arguments = args == null ? new List<string>() : new List<string>(args);
            if (arguments.Count == 0)
            {
                arguments.Add(commands[0].Item1);
            }
            else if (arguments[0] != "-?"
                && !arguments[0].Equals("-h", StringComparison.OrdinalIgnoreCase)
                && !arguments[0].Equals("--help", StringComparison.OrdinalIgnoreCase)
                && commands.All(x => !x.Item1.Equals(arguments[0], StringComparison.OrdinalIgnoreCase)))
            {
                arguments.Insert(0, commands[0].Item1);
            }
            else if (arguments.Count == 1 && arguments[0].Equals("help", StringComparison.OrdinalIgnoreCase))
            {
                // Special case for the help command without any additional arguments, output global help instead
                arguments[0] = "--help";
            }

            // If the first arg is a command, convert it to lowercase
            // TODO: Add feature to upstream System.CommandLine to ignore case of commands, options, and arguments then remove this
            if (commands.Any(x => x.Item1.Equals(arguments[0], StringComparison.OrdinalIgnoreCase)))
            {
                arguments[0] = arguments[0].ToLowerInvariant();
            }

            // Parse the command line arguments
            Command command = null;
            ArgumentSyntax parsed = ArgumentSyntax.Parse(arguments, syntax =>
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

            if (parsed.IsHelpRequested() || hasErrors)
            {
                return null;
            }
            Trace.Information($"**{commands.First(x => x.Item2 == command).Item1.ToUpperInvariant()}**");
            return command;
        }
    }
}