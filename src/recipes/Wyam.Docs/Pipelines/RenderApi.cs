using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Execution;
using Wyam.Common.Modules;
using Wyam.Core.Modules.Control;
using Wyam.Core.Modules.IO;
using Wyam.Html;

namespace Wyam.Docs.Pipelines
{
    /// <summary>
    /// Renders and outputs the API pages using the API template layouts
    /// (this pipeline might take a bit of time).
    /// </summary>
    public class RenderApi : Pipeline
    {
        internal RenderApi()
            : base(GetModules())
        {
        }

        private static IModuleList GetModules() => new ModuleList
        {
            new If(ctx => ctx.Documents[Docs.Api].Any(),
                new Documents(Docs.Api),
                new Razor.Razor()
                    .WithLayout("/_ApiLayout.cshtml"),
                new Headings(),
                new HtmlInsert("div#infobar-headings", (doc, ctx) => ctx.GenerateInfobarHeadings(doc)),
                new WriteFiles())
                .WithoutUnmatchedDocuments()
        };
    }
}
