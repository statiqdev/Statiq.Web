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
    /// Renders and outputs the blog posts using the template layouts.
    /// </summary>
    public class RenderBlogPosts : Pipeline
    {
        internal RenderBlogPosts()
            : base(GetModules())
        {
        }

        private static ModuleList GetModules() => new ModuleList
        {
            // Render the blog after the indexes and archive so the layout doesn't show up when including whole page (I.e., first post)
            new If(ctx => ctx.Documents[Docs.BlogPosts].Any(),
                new Documents(Docs.BlogPosts),
                new Razor.Razor()
                    .WithLayout("/_BlogPost.cshtml"),
                new Headings(),
                new HtmlInsert("div#infobar-headings", (doc, ctx) => ctx.GenerateInfobarHeadings(doc)),
                new WriteFiles()
            )
        };
    }
}
