using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Execution;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Core.Modules.Control;
using Wyam.Core.Modules.IO;
using Wyam.Core.Modules.Metadata;

namespace Wyam.Docs.Pipelines
{
    /// <summary>
    /// Generates the index pages for blog posts.
    /// </summary>
    public class BlogIndexes : Pipeline
    {
        internal BlogIndexes()
            : base(GetModules())
        {
        }

        private static ModuleList GetModules() => new ModuleList
        {
            new If(ctx => ctx.Documents[Docs.BlogPosts].Any(),
                new ReadFiles("_BlogIndex.cshtml"),
                new Paginate(5,
                    new Documents(Docs.BlogPosts),
                    new OrderBy((doc, ctx) => doc.Get<DateTime>(DocsKeys.Published)).Descending()
                ),
                new Meta(Keys.Title, (doc, ctx) =>
                    doc.Get<int>(Keys.CurrentPage) == 1
                        ? "Blog"
                        : $"Blog (Page {doc[Keys.CurrentPage]})"),
                new Meta(Keys.RelativeFilePath, (doc, ctx) =>
                    doc.Get<int>(Keys.CurrentPage) == 1
                        ? "blog/index.html"
                        : $"blog/page{doc[Keys.CurrentPage]}.html"),
                new Razor.Razor()
                    .IgnorePrefix(null)
                    .WithLayout("/_BlogLayout.cshtml"),
                new WriteFiles()
            )
        };
    }
}
