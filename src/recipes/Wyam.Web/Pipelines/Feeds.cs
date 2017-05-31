using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Configuration;
using Wyam.Common.Execution;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Util;
using Wyam.Core.Modules.Control;
using Wyam.Core.Modules.IO;
using Wyam.Feeds;
using Wyam.Html;

namespace Wyam.Web.Pipelines
{
    /// <summary>
    /// Generates RSS, Atom, and/or RDF feeds.
    /// </summary>
    public class Feeds : Pipeline
    {
        /// <summary>
        /// Creates the pipeline.
        /// </summary>
        /// <param name="name">The name of this pipeline.</param>
        /// <param name="settings">The settings for the pipeline.</param>
        public Feeds(string name, FeedsSettings settings)
            : base(name, GetModules(settings))
        {
        }

        private static IModuleList GetModules(FeedsSettings settings) => new ModuleList
        {
            new Documents()
                .FromPipelines(settings.Pipelines),
            settings.Customization(
                new GenerateFeeds()
                    .WithRssPath(settings.RssPath)
                    .WithAtomPath(settings.AtomPath)
                    .WithRdfPath(settings.RdfPath)
                    .WithItemDescription((doc, ctx) => doc.String(HtmlKeys.Excerpt))
                    .WithItemContent((doc, ctx) => doc.Content)),
            new WriteFiles()
        };
    }
}
