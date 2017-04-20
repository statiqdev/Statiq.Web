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

namespace Wyam.Docs.Pipelines
{
    /// <summary>
    /// Generates the category pages for blog posts.
    /// </summary>
    public class BlogCategories : Pipeline
    {
        internal BlogCategories()
            : base(GetModules())
        {
        }

        private static IModuleList GetModules() => new ModuleList
        {
            new If(ctx => ctx.Documents[Docs.BlogPosts].Any(),
                new ReadFiles("_BlogIndex.cshtml"),
                new GroupBy((doc, ctx) => doc[DocsKeys.Category],
                    new Documents(Docs.BlogPosts)
                ),
                new ForEach(
                    new Paginate(5,
                        new Documents((doc, ctx) => doc[Keys.GroupDocuments]),
                        new OrderBy((doc, ctx) => doc.Get<DateTime>(DocsKeys.Published)).Descending()
                    ),
                    new Meta(Keys.Title, (doc, ctx) =>
                        doc.Get<int>(Keys.CurrentPage) == 1
                            ? $"{doc[Keys.GroupKey]}"
                            : $"{doc[Keys.GroupKey]} (Page {doc[Keys.CurrentPage]})"),
                    new Meta(Keys.RelativeFilePath, (doc, ctx) =>
                    {
                        string category = doc.String(Keys.GroupKey).ToLower().Replace(" ", "-").Replace("'", string.Empty);
                        return doc.Get<int>(Keys.CurrentPage) == 1
                            ? $"blog/{category}/index.html"
                            : $"blog/{category}/page{doc[Keys.CurrentPage]}.html";
                    }),
                    new Razor.Razor()
                        .IgnorePrefix(null)
                        .WithLayout("/_BlogLayout.cshtml"),
                    new WriteFiles()
                )
            )
        };
    }
}
