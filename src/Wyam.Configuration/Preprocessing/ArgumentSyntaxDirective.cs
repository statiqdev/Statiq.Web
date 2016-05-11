using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;

namespace Wyam.Configuration.Preprocessing
{
    internal abstract class ArgumentSyntaxDirective<TSettings> : IDirective
        where TSettings : new()
    {
        public abstract IEnumerable<string> DirectiveNames { get; }

        public void Process(Configurator configurator, string value)
        {
            Process(configurator, value, false);
        }

        public string GetHelpText()
        {
            return string.Join(Environment.NewLine, Process(null, string.Empty, true)
                .Split(new[] { Environment.NewLine }, StringSplitOptions.None).Skip(1));
        }

        private string Process(Configurator configurator, string value, bool getHelpText)
        {
            IReadOnlyList<string> sources = null;

            // Parse the directive value
            IEnumerable<string> arguments = ArgumentSplitter.Split(value);
            ArgumentSyntax parsed = null;
            string helpText = null;
            TSettings settings = new TSettings();
            try
            {
                parsed = ArgumentSyntax.Parse(arguments, syntax =>
                {
                    syntax.HandleErrors = !getHelpText;
                    Define(syntax, settings);
                    if (getHelpText)
                    {
                        helpText = syntax.GetHelpText();
                    }
                });
            }
            catch (Exception)
            {
                if (!getHelpText)
                {
                    throw;
                }
            }
            if (getHelpText)
            {
                return helpText;
            }
            if (parsed.HasErrors)
            {
                throw new Exception(parsed.GetHelpText());
            }

            Process(configurator, settings);
            return null;
        }

        protected abstract void Define(ArgumentSyntax syntax, TSettings settings);

        protected abstract void Process(Configurator configurator, TSettings settings);
    }
}