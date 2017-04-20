using System;
using Wyam.Common.Configuration;
using Wyam.Common.Execution;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Util;
using Wyam.Core.Modules.Control;
using Wyam.Core.Modules.IO;
using Wyam.Feeds;

namespace Wyam.BookSite.Pipelines
{
    /// <summary>
    /// Generates the blog RSS, Atom, and/or RDF feeds.
    /// </summary>
    public class Feed : Pipeline
    {
        internal Feed()
            : base(GetModules())
        {
        }

        private static ModuleList GetModules() => new ModuleList
        {
            new Documents(BookSite.Posts),
            new GenerateFeeds()
                .WithRssPath(ctx => ctx.FilePath(BookSiteKeys.RssPath))
                .WithAtomPath(ctx => ctx.FilePath(BookSiteKeys.AtomPath))
                .WithRdfPath(ctx => ctx.FilePath(BookSiteKeys.RdfPath)),
            new WriteFiles()
        };
    }
}