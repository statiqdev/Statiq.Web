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
    /// Generates the API index file.
    /// </summary>
    public class ApiIndex : Pipeline
    {
        internal ApiIndex()
            : base(GetModules())
        {
        }

        private static IModuleList GetModules() => new ModuleList
        {
            new If(ctx => ctx.Documents[Docs.Api].Any(),
                new ReadFiles("_ApiIndex.cshtml"),
                new Meta(Keys.RelativeFilePath, "api/index.html"),
                new Meta(Keys.SourceFileName, "index.html"),
                new Title("API"),
                new Razor.Razor(),
                new WriteFiles())
                .WithoutUnmatchedDocuments()
        };
    }
}
