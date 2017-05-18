using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Configuration;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Util;
using Wyam.Core.Modules.Control;
using Wyam.Core.Modules.Extensibility;

namespace Wyam.Web.Pipelines
{
    /// <summary>
    /// Validates links.
    /// </summary>
    public class ValidateLinks : Pipeline
    {
        /// <summary>
        /// Creates the pipeline.
        /// </summary>
        /// <param name="name">The name of this pipeline.</param>
        /// <param name="settings">The settings for the pipeline.</param>
        public ValidateLinks(string name, ValidateLinksSettings settings)
            : base(name, GetModules(settings))
        {
        }

        private static IModuleList GetModules(ValidateLinksSettings settings) => new ModuleList
        {
            new If(
                ctx => settings.ValidateAbsoluteLinks.Invoke<bool>(ctx) || settings.ValidateRelativeLinks.Invoke<bool>(ctx),
                new Documents()
                    .FromPipelines(settings.Pipelines),
                new Where((doc, ctx) =>
                {
                    FilePath destinationPath = doc.FilePath(Keys.DestinationFilePath);
                    return destinationPath != null
                           && (destinationPath.Extension == ".html" || destinationPath.Extension == ".htm");
                }),
                new Execute(ctx =>
                    new Html.ValidateLinks()
                        .ValidateAbsoluteLinks(settings.ValidateAbsoluteLinks.Invoke<bool>(ctx))
                        .ValidateRelativeLinks(settings.ValidateRelativeLinks.Invoke<bool>(ctx))
                        .AsError(settings.ValidateLinksAsError.Invoke<bool>(ctx))))
        };
    }
}
