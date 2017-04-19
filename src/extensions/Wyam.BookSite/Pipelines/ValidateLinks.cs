using System;
using Wyam.Common.Configuration;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Util;
using Wyam.Core.Modules.Control;
using Wyam.Core.Modules.Extensibility;

namespace Wyam.BookSite.Pipelines
{
    /// <summary>
    /// Validates links.
    /// </summary>
    public class ValidateLinks : Pipeline
    {
        internal ValidateLinks()
            : base(GetModules())
        {
        }

        private static ModuleList GetModules() => new ModuleList
        {
            new If(
                ctx => ctx.Bool(BookSiteKeys.ValidateAbsoluteLinks) || ctx.Bool(BookSiteKeys.ValidateRelativeLinks),
                new Documents(BookSite.RenderPages),
                new Concat(new Documents(BookSite.Posts)),
                new Concat(new Documents(BookSite.Resources)),
                new Where((doc, ctx) =>
                {
                    FilePath destinationPath = doc.FilePath(Keys.DestinationFilePath);
                    return destinationPath != null && (destinationPath.Extension == ".html" || destinationPath.Extension == ".htm");
                }),
                new Execute(ctx =>
                    new Html.ValidateLinks()
                        .ValidateAbsoluteLinks(ctx.Bool(BookSiteKeys.ValidateAbsoluteLinks))
                        .ValidateRelativeLinks(ctx.Bool(BookSiteKeys.ValidateRelativeLinks))
                        .AsError(ctx.Bool(BookSiteKeys.ValidateLinksAsError))))
        };
    }
}