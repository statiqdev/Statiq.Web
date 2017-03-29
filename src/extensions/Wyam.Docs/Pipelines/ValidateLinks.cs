using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Util;
using Wyam.Core.Modules.Control;
using Wyam.Core.Modules.Extensibility;

namespace Wyam.Docs.Pipelines
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
                ctx => ctx.Bool(DocsKeys.ValidateAbsoluteLinks) || ctx.Bool(DocsKeys.ValidateRelativeLinks),
                new Documents(Docs.RenderPages),
                new Concat(
                    new Documents(Docs.RenderBlogPosts)),
                new Concat(
                    new Documents(Docs.RenderApi)),
                new Concat(
                    new Documents(Docs.Resources)),
                new Where((doc, ctx) =>
                {
                    FilePath destinationPath = doc.FilePath(Keys.DestinationFilePath);
                    return destinationPath != null
                           && (destinationPath.Extension == ".html" || destinationPath.Extension == ".htm");
                }),
                new Execute(ctx =>
                    new Html.ValidateLinks()
                        .ValidateAbsoluteLinks(ctx.Bool(DocsKeys.ValidateAbsoluteLinks))
                        .ValidateRelativeLinks(ctx.Bool(DocsKeys.ValidateRelativeLinks))
                        .AsError(ctx.Bool(DocsKeys.ValidateLinksAsError))))
        };
    }
}
