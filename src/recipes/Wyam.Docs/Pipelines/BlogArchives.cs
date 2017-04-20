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
    /// Generates the date-based archive pages for blog posts.
    /// </summary>
    public class BlogArchives : Pipeline
    {
        internal BlogArchives()
            : base(GetModules())
        {
        }

        private static ModuleList GetModules() => new ModuleList
        {
            new If(ctx => ctx.Documents[Docs.BlogPosts].Any(),
                // Monthly archives
                new ReadFiles("_BlogIndex.cshtml"),
                new GroupBy((doc, ctx) => new DateTime(doc.Get<DateTime>(DocsKeys.Published).Year, doc.Get<DateTime>(DocsKeys.Published).Month, 1),
                    new Documents(Docs.BlogPosts)
                ),
                new Meta(Keys.Title, (doc, ctx) => doc.Get<DateTime>(Keys.GroupKey).ToString("MMMM, yyyy")),
                new Meta("Link", (doc, ctx) => doc.Get<DateTime>(Keys.GroupKey).ToString("yyyy/MM")),
                new Concat(
                    // Yearly archives
                    new ReadFiles("_BlogIndex.cshtml"),
                    new GroupBy((doc, ctx) => new DateTime(doc.Get<DateTime>(DocsKeys.Published).Year, 1, 1),
                        new Documents(Docs.BlogPosts)
                    ),
                    new Meta(Keys.Title, (doc, ctx) => doc.Get<DateTime>(Keys.GroupKey).ToString("yyyy")),
                    new Meta("Link", (doc, ctx) => doc.Get<DateTime>(Keys.GroupKey).ToString("yyyy"))
                ),
                new ForEach(
                    new Paginate(5,
                        new Documents((doc, ctx) => doc[Keys.GroupDocuments]),
                        new OrderBy((doc, ctx) => doc.Get<DateTime>(DocsKeys.Published)).Descending()
                    ),
                    new Meta(Keys.Title, (doc, ctx) =>
                        doc.Get<int>(Keys.CurrentPage) == 1
                            ? $"{doc[Keys.Title]}"
                            : $"{doc[Keys.Title]} (Page {doc[Keys.CurrentPage]})"),
                    new Meta(Keys.RelativeFilePath, (doc, ctx) =>
                        doc.Get<int>(Keys.CurrentPage) == 1
                            ? $"blog/archive/{doc["Link"]}/index.html"
                            : $"blog/archive/{doc["Link"]}/page{doc[Keys.CurrentPage]}.html"),
                    new Razor.Razor()
                        .IgnorePrefix(null)
                        .WithLayout("/_BlogLayout.cshtml"),
                    new WriteFiles()
                )
            )
        };
    }
}
