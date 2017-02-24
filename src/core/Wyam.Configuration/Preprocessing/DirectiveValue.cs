namespace Wyam.Configuration.Preprocessing
{
    /// <summary>
    /// Represents the use of a directive.
    /// </summary>
    public class DirectiveValue
    {
        /// <summary>
        /// Gets the line where the directive was specified
        /// (or <c>null</c> if created outside the configuration file).
        /// </summary>
        ///
        public int? Line { get; }

        /// <summary>
        /// Gets the directive name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the directive value.
        /// </summary>
        public string Value { get; }

        public DirectiveValue(int line, string name, string value)
        {
            Line = line;
            Name = name;
            Value = value;
        }

        public DirectiveValue(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}