using System;
using Wyam.Common.Configuration;
using Wyam.Feeds;

namespace Wyam.Web.Pipelines
{
    /// <summary>
    /// Settings for the <see cref="Feeds"/> pipeline.
    /// </summary>
    public class FeedsSettings
    {
        /// <summary>
        /// The name of pipelines from which feed items should be taken.
        /// </summary>
        public string[] Pipelines { get; set; }

        /// <summary>
        /// A delegate that returns the RSS path to use.
        /// </summary>
        public ContextConfig RssPath { get; set; }

        /// <summary>
        /// A delegate that returns the Atom path to use.
        /// </summary>
        public ContextConfig AtomPath { get; set; }

        /// <summary>
        /// A delegate that returns the RDF path to use.
        /// </summary>
        public ContextConfig RdfPath { get; set; }

        /// <summary>
        /// Allows customization of the <see cref="GenerateFeeds"/> module for
        /// setting metadata fields, etc.
        /// </summary>
        public Func<GenerateFeeds, GenerateFeeds> Customization { get; set; } = x => x;
    }
}