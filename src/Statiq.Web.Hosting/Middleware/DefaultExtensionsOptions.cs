using System.Collections.Generic;

namespace Statiq.Web.Hosting.Middleware
{
    /// <summary>
    /// Extensions to use when no extension is provided in the URL.
    /// </summary>
    internal class DefaultExtensionsOptions
    {
        /// <summary>
        /// Gets or sets the extensions.
        /// </summary>
        /// <value>
        /// The extensions.
        /// </value>
        public IList<string> Extensions { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultExtensionsOptions"/> class.
        /// </summary>
        public DefaultExtensionsOptions() => Extensions = new List<string>
        {
            ".htm",
            ".html"
        };
    }
}