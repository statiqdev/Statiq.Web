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
using Wyam.Core.Modules.Metadata;
using Wyam.Html;

namespace Wyam.Docs.Pipelines
{
    /// <summary>
    /// Renders and outputs the document content pages using the template layouts.
    /// </summary>
    public class RenderPages : Pipeline
    {
        internal RenderPages()
            : base(GetModules())
        {
        }

        private static IModuleList GetModules() => new ModuleList
        {
            new If(ctx => ctx.Documents[Docs.Pages].Any(),
                new Documents(Docs.Pages),
                new Flatten(),
                // Hide the sidebar for root pages if there's no children
                new Meta(DocsKeys.NoSidebar, (doc, ctx) => doc.Get(DocsKeys.NoSidebar,
                    (doc.DocumentList(Keys.Children)?.Count ?? 0) == 0)
                    && doc.Document(Keys.Parent) == null),
                new Title(),
                new Razor.Razor()
                    .WithLayout("/_Layout.cshtml"),
                new Headings(),
                new HtmlInsert("div#infobar-headings", (doc, ctx) => ctx.GenerateInfobarHeadings(doc)),
                new WriteFiles()
            )
        };
    }
}
