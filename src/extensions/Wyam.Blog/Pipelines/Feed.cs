using System;
using Wyam.Common.Configuration;
using Wyam.Common.Modules;
using Wyam.Core.Modules.Control;
using Wyam.Core.Modules.IO;
using Wyam.Feeds;

namespace Wyam.Blog.Pipelines
{
    /// <summary>
    /// Generates the blog RSS, Atom, and/or RDF feeds.
    /// </summary>
    public class Feed : RecipePipeline
    {
        /// <inheritdoc />
        public override string Name => nameof(Blog.Feed);

        /// <inheritdoc />
        public override ModuleList GetModules() => new ModuleList
        {
            new Documents(BlogPipelines.Posts),
            new GenerateFeeds()
                .WithRssPath(ctx => ctx.FilePath(BlogKeys.RssPath))
                .WithAtomPath(ctx => ctx.FilePath(BlogKeys.AtomPath))
                .WithRdfPath(ctx => ctx.FilePath(BlogKeys.RdfPath)),
            new WriteFiles()
        };
    }
}