using Wyam.Common.Configuration;

namespace Wyam.Web.Pipelines
{
    /// <summary>
    /// Settings for the <see cref="ValidateLinks"/> pipeline.
    /// </summary>
    public class ValidateLinksSettings
    {
        /// <summary>
        /// The name of pipelines from which links should be validated.
        /// </summary>
        public string[] Pipelines { get; set; }

        /// <summary>
        /// A delegate to indicate whether absolute links should be validated.
        /// </summary>
        public ContextConfig ValidateAbsoluteLinks { get; set; }

        /// <summary>
        /// A delegate to indicate whether relative links should be validated.
        /// </summary>
        public ContextConfig ValidateRelativeLinks { get; set; }

        /// <summary>
        /// A delegate to indicate whether links should validate as errors.
        /// </summary>
        public ContextConfig ValidateLinksAsError { get; set; }
    }
}