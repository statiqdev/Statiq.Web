using System;
using System.Collections.Generic;
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
            Update
        }

        public CommandEnum Command = CommandEnum.None;

        public bool ParseArgs(string[] args, out bool hasErrors)
        {
            System.CommandLine.ArgumentSyntax parsed = System.CommandLine.ArgumentSyntax.Parse(args, syntax =>
            {
                syntax.DefineCommand("update", ref Command, CommandEnum.Update, "Update to the latest version.");
            });

            hasErrors = parsed.HasErrors;
            return !(parsed.IsHelpRequested() || hasErrors);
        }
    }
}
