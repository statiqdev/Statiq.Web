using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Execution;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Util;
using Wyam.Core.Modules.Control;
using Wyam.Core.Modules.IO;
using Wyam.Feeds;

namespace Wyam.Docs.Pipelines
{
    /// <summary>
    /// Generates the blog RSS, Atom, and/or RDF feeds.
    /// </summary>
    public class BlogFeed : Pipeline
    {
        internal BlogFeed()
            : base(GetModules())
        {
        }

        private static IModuleList GetModules() => new ModuleList
        {
            new If(ctx => ctx.Documents[Docs.BlogPosts].Any(),
                new Documents(Docs.BlogPosts),
                new GenerateFeeds()
                    .WithRssPath(ctx => ctx.FilePath(DocsKeys.BlogRssPath))
                    .WithAtomPath(ctx => ctx.FilePath(DocsKeys.BlogAtomPath))
                    .WithRdfPath(ctx => ctx.FilePath(DocsKeys.BlogRdfPath)),
                new WriteFiles()
            )
        };
    }
}
