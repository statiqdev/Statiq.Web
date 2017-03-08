using System;
using Wyam.Common.Configuration;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Core.Modules.Control;
using Wyam.Core.Modules.Extensibility;

namespace Wyam.Blog.Pipelines
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
                ctx => ctx.Bool(BlogKeys.ValidateAbsoluteLinks) || ctx.Bool(BlogKeys.ValidateRelativeLinks),
                new Documents(Blog.RenderPages),
                new Concat(new Documents(Blog.Posts)),
                new Concat(new Documents(Blog.Resources)),
                new Where((doc, ctx) =>
                {
                    FilePath destinationPath = doc.FilePath(Keys.DestinationFilePath);
                    return destinationPath != null && (destinationPath.Extension == ".html" || destinationPath.Extension == ".htm");
                }),
                new Execute(ctx =>
                    new Html.ValidateLinks()
                        .ValidateAbsoluteLinks(ctx.Bool(BlogKeys.ValidateAbsoluteLinks))
                        .ValidateRelativeLinks(ctx.Bool(BlogKeys.ValidateRelativeLinks))
                        .AsError(ctx.Bool(BlogKeys.ValidateLinksAsError))))
        };
    }
}