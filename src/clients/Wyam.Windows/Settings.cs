using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Windows
{
    internal class Settings
    {
        public enum CommandEnum
        {
            None,
            SquirrelInstall,
            SquirrelUpdated,
            SquirrelObsolete,
            SquirrelUninstall,
            SquirrelFirstRun,
            Update,
            AddPath,
            RemovePath
        }

        public CommandEnum Command = CommandEnum.None;
        public string SquirrelVersion = null;

        public bool ParseArgs(string[] args, out bool hasErrors)
        {
            hasErrors = false;
            if (args == null || args.Length == 0)
            {
                return true;
            }

            bool squirrelOption = false;
            try
            {
                ArgumentSyntax.Parse(args, syntax =>
                {
                    syntax.HandleHelp = false;
                    syntax.HandleErrors = false;
                    if (DefineSquirrelOption(syntax, "squirrel-install", CommandEnum.SquirrelInstall, true)
                        || DefineSquirrelOption(syntax, "squirrel-updated", CommandEnum.SquirrelUpdated, true)
                        || DefineSquirrelOption(syntax, "squirrel-obsolete", CommandEnum.SquirrelObsolete, true)
                        || DefineSquirrelOption(syntax, "squirrel-uninstall", CommandEnum.SquirrelUninstall, true)
                        || DefineSquirrelOption(syntax, "squirrel-firstrun", CommandEnum.SquirrelFirstRun, false))
                    {
                        // If this is a Squirrel option, we don't need to continue processing args
                        squirrelOption = true;
                    }
                });
            }
            catch (Exception)
            {
                // Don't worry about errors from the first pass
            }
            if (squirrelOption)
            {
                return true;
            }

            // Second pass for non-Squirrel options
            ArgumentSyntax parsed = ArgumentSyntax.Parse(args, syntax =>
            {
                syntax.DefineCommand("update", ref Command, CommandEnum.Update, "Update to the latest version.");
                syntax.DefineCommand("add-path", ref Command, CommandEnum.AddPath, "Add the installation path to the PATH system environment variable.");
                syntax.DefineCommand("remove-path", ref Command, CommandEnum.RemovePath, "Remove the installation path from the PATH system environment variable.");
            });


            hasErrors = parsed.HasErrors;
            return !(parsed.IsHelpRequested() || hasErrors);
        }

        private bool DefineSquirrelOption(ArgumentSyntax syntax, string name, CommandEnum command, bool hasVersion)
        {
            Argument<string> option = syntax.DefineOption(name, ref SquirrelVersion, hasVersion, string.Empty);
            option.IsHidden = true;
            if (option.IsSpecified)
            {
                Command = command;
                return true;
            }
            return false;
        }
    }
}
