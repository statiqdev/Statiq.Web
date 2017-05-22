using Wyam.Common.Configuration;

namespace Wyam.Web.Pipelines
{
    /// <summary>
    /// Settings for the <see cref="Redirects"/> pipeline.
    /// </summary>
    public class RedirectsSettings
    {
        /// <summary>
        /// The name of pipelines for which redirects should be calculated.
        /// </summary>
        public string[] Pipelines { get; set; }

        /// <summary>
        /// A delegate specifying whether META-REFRESH redirects should be generated.
        /// </summary>
        public ContextConfig MetaRefreshRedirects { get; set; }

        /// <summary>
        /// A delegate specifying whether Netlify-style redirects file should be generated.
        /// </summary>
        public ContextConfig NetlifyRedirects { get; set; }
    }
}