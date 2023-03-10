using System.Collections.Generic;

namespace Statiq.Web.Hosting.Middleware
{
    /// <summary>
    /// Custom headers to add to responses.
    /// </summary>
    internal class CustomHeadersOptions
    {
        /// <summary>
        /// Gets or sets the extensions.
        /// </summary>
        /// <value>
        /// The extensions.
        /// </value>
        public IReadOnlyDictionary<string, string> CustomHeaders { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomHeadersOptions"/> class.
        /// </summary>
        public CustomHeadersOptions() =>
            CustomHeaders = new Dictionary<string, string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomHeadersOptions"/> class.
        /// </summary>
        public CustomHeadersOptions(IReadOnlyDictionary<string, string> customHeaders) =>
            CustomHeaders = customHeaders ?? new Dictionary<string, string>();
    }
}