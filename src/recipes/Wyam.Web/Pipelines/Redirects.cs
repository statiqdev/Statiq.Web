using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Configuration;
using Wyam.Common.Execution;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Util;
using Wyam.Core.Modules.Contents;
using Wyam.Core.Modules.Control;
using Wyam.Core.Modules.Extensibility;
using Wyam.Core.Modules.IO;

namespace Wyam.Web.Pipelines
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
        /// <param name="metaRefreshRedirects">A delegate specifying whether META-REFRESH redirects should be generated.</param>
        /// <param name="netlifyRedirects">A delegate specifying whether Netlify-style redirects file should be generated.</param>
        public Redirects(string name, string[] pipelines, ContextConfig metaRefreshRedirects, ContextConfig netlifyRedirects)
            : base(name, GetModules(pipelines, metaRefreshRedirects, netlifyRedirects))
        {
        }

        private static IModuleList GetModules(string[] pipelines, ContextConfig metaRefreshRedirects, ContextConfig netlifyRedirects) => new ModuleList
        {
            new Documents()
                .FromPipelines(pipelines),
            new Execute(ctx =>
            {
                Redirect redirect = new Redirect()
                    .WithMetaRefreshPages(metaRefreshRedirects.Invoke<bool>(ctx));
                if (netlifyRedirects.Invoke<bool>(ctx))
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
