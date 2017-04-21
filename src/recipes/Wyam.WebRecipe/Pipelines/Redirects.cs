using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Execution;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Util;
using Wyam.Core.Modules.Contents;
using Wyam.Core.Modules.Control;
using Wyam.Core.Modules.Extensibility;
using Wyam.Core.Modules.IO;

namespace Wyam.WebRecipe.Pipelines
{
    /// <summary>
    /// Generates any redirect placeholders and files.
    /// </summary>
    public class Redirects : Pipeline
    {
        /// <summary>
        /// Creates the pipeline.
        /// </summary>
        /// <param name="name">The name of this pipeline.</param>
        /// <param name="pipelines">The name of pipelines for which redirects should be calculated.</param>
        public Redirects(string name, string[] pipelines)
            : base(name, GetModules(pipelines))
        {
        }

        private static IModuleList GetModules(string[] pipelines) => new ModuleList
        {
            new Documents()
                .FromPipelines(pipelines),
            new Execute(ctx =>
            {
                Redirect redirect = new Redirect()
                    .WithMetaRefreshPages(ctx.Bool(WebRecipeKeys.MetaRefreshRedirects));
                if (ctx.Bool(WebRecipeKeys.NetlifyRedirects))
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
