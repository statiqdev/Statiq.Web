using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Util;
using Wyam.Core.Modules.Control;
using Wyam.Core.Modules.Extensibility;

namespace Wyam.WebRecipe.Pipelines
{
    /// <summary>
    /// Validates links.
    /// </summary>
    public class ValidateLinks : Pipeline
    {
        public ValidateLinks(params string[] pipelines)
            : base(GetModules(pipelines))
        {
        }

        private static IModuleList GetModules(string[] pipelines) => new ModuleList
        {
            new If(
                ctx => ctx.Bool(WebRecipeKeys.ValidateAbsoluteLinks) || ctx.Bool(WebRecipeKeys.ValidateRelativeLinks),
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
                        .ValidateAbsoluteLinks(ctx.Bool(WebRecipeKeys.ValidateAbsoluteLinks))
                        .ValidateRelativeLinks(ctx.Bool(WebRecipeKeys.ValidateRelativeLinks))
                        .AsError(ctx.Bool(WebRecipeKeys.ValidateLinksAsError))))
        };
    }
}
