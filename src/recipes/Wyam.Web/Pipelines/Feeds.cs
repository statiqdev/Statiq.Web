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
        /// <param name="pipelines">The name of pipelines from which feed items should be taken.</param>
        /// <param name="rssPath">A delegate that returns the RSS path to use.</param>
        /// <param name="atomPath">A delegate that returns the Atom path to use.</param>
        /// <param name="rdfPath">A delegate that returns the RDF path to use.</param>
        public Feeds(string name, string[] pipelines, ContextConfig rssPath, ContextConfig atomPath, ContextConfig rdfPath)
            : base(name, GetModules(pipelines, rssPath, atomPath, rdfPath))
        {
        }

        private static IModuleList GetModules(string[] pipelines, ContextConfig rssPath, ContextConfig atomPath, ContextConfig rdfPath) => new ModuleList
        {
            new Documents()
                .FromPipelines(pipelines),
            new GenerateFeeds()
                .WithRssPath(rssPath)
                .WithAtomPath(atomPath)
                .WithRdfPath(rdfPath),
            new WriteFiles()
        };
    }
}
