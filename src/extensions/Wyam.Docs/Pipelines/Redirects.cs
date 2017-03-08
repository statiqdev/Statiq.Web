using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Execution;
using Wyam.Common.Modules;
using Wyam.Core.Modules.Contents;
using Wyam.Core.Modules.Control;
using Wyam.Core.Modules.Extensibility;
using Wyam.Core.Modules.IO;

namespace Wyam.Docs.Pipelines
{
    /// <summary>
    /// Generates any redirect placeholders and files.
    /// </summary>
    public class Redirects : Pipeline
    {
        internal Redirects()
            : base(GetModules())
        {
        }

        private static ModuleList GetModules() => new ModuleList
        {
            new Documents(Docs.RenderPages),
            new Concat(
                new Documents(Docs.RenderBlogPosts)
            ),
            new Execute(ctx =>
            {
                Redirect redirect = new Redirect()
                    .WithMetaRefreshPages(ctx.Bool(DocsKeys.MetaRefreshRedirects));
                if (ctx.Bool(DocsKeys.NetlifyRedirects))
                {
                    redirect.WithAdditionalOutput("_redirects", redirects =>
                        string.Join(Environment.NewLine, redirects.Select(r => $"/{r.Key} {r.Value}")));
                }
                return redirect;
            }),
            new WriteFiles()
        };
    }
}
