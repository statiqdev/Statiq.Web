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
        /// <param name="settings">The settings for the pipeline.</param>
        public Redirects(string name, RedirectsSettings settings)
            : base(name, GetModules(settings))
        {
        }

        private static IModuleList GetModules(RedirectsSettings settings) => new ModuleList
        {
            new Documents()
                .FromPipelines(settings.Pipelines),
            new Execute(ctx =>
            {
                Redirect redirect = new Redirect()
                    .WithMetaRefreshPages(settings.MetaRefreshRedirects.Invoke<bool>(ctx));
                if (settings.NetlifyRedirects.Invoke<bool>(ctx))
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
