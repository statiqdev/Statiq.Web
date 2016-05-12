using System.Collections.Generic;

namespace Wyam.Configuration.Preprocessing
{
    /// <summary>
    /// A directive that can be used in the configuration file and optionally on the
    /// command line.
    /// </summary>
    public interface IDirective
    {
        /// <summary>
        /// Gets the directive name(s) for use in configuration files.
        /// </summary>
        IEnumerable<string> DirectiveNames { get; }

        /// <summary>
        /// Indicates whether the directive should be supported in command
        /// line interfaces. If <c>true</c>, the first name will be used
        /// for the CLI option.
        /// </summary>
        bool SupportsCli { get; }

        /// <summary>
        /// Gets a description of the directive.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Processes the directive.
        /// </summary>
        /// <param name="configurator">The configurator.</param>
        /// <param name="value">The value of the directive.</param>
        void Process(Configurator configurator, string value);

        /// <summary>
        /// Gets the help text.
        /// </summary>
        string GetHelpText();
    }
}
