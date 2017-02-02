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
        /// Gets the directive name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the short directive name.
        /// </summary>
        string ShortName { get; }

        /// <summary>
        /// Gets a value indicating whether the directive supports multiple values.
        /// </summary>
        bool SupportsMultiple { get; }

        /// <summary>
        /// Gets a description of the directive.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Gets a string comparer that can be used to compare values of one directive to another for equality.
        /// </summary>
        IEqualityComparer<string> ValueComparer { get; }

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
