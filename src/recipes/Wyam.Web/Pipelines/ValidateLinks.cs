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
        /// <param name="pipelines">The name of pipelines from which links should be validated.</param>
        /// <param name="validateAbsoluteLinks">A delegate to indicate whether absolute links should be validated.</param>
        /// <param name="validateRelativeLinks">A delegate to indicate whether relative links should be validated.</param>
        /// <param name="validateLinksAsError">A delegate to indicate whether links should validate as errors.</param>
        public ValidateLinks(string name, string[] pipelines, ContextConfig validateAbsoluteLinks, ContextConfig validateRelativeLinks, ContextConfig validateLinksAsError)
            : base(name, GetModules(pipelines, validateAbsoluteLinks, validateRelativeLinks, validateLinksAsError))
        {
        }

        private static IModuleList GetModules(string[] pipelines, ContextConfig validateAbsoluteLinks, ContextConfig validateRelativeLinks, ContextConfig validateLinksAsError) => new ModuleList
        {
            new If(
                ctx => validateAbsoluteLinks.Invoke<bool>(ctx) || validateRelativeLinks.Invoke<bool>(ctx),
                new Documents()
                    .FromPipelines(pipelines),
                new Where((doc, ctx) =>
                {
                    FilePath destinationPath = doc.FilePath(Keys.DestinationFilePath);
                    return destinationPath != null
                           && (destinationPath.Extension == ".html" || destinationPath.Extension == ".htm");
                }),
                new Execute(ctx =>
                    new Html.ValidateLinks()
                        .ValidateAbsoluteLinks(validateAbsoluteLinks.Invoke<bool>(ctx))
                        .ValidateRelativeLinks(validateRelativeLinks.Invoke<bool>(ctx))
                        .AsError(validateLinksAsError.Invoke<bool>(ctx))))
        };
    }
}
