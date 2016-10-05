using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.CodeAnalysis;
using Wyam.Common.Configuration;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Core.Modules.Control;
using Wyam.Core.Modules.Extensibility;
using Wyam.Core.Modules.IO;
using Wyam.Razor;

namespace Wyam.Docs
{
    public class Docs : IRecipe
    {
        public void Apply(IEngine engine)
        {
            // Global metadata defaults
            engine.GlobalMetadata[DocsKeys.SourceFiles] = "src/**/*.cs";
            engine.GlobalMetadata[DocsKeys.IncludeGlobal] = true;
            engine.GlobalMetadata[DocsKeys.ApiPathPrefix] = "api";

            engine.Pipelines.Add(DocsPipelines.Code,
                new ReadFiles((doc, ctx) => ctx.GlobalMetadata.List<string>(DocsKeys.SourceFiles))
            );

            engine.Pipelines.Add(DocsPipelines.Api,
                new Documents(DocsPipelines.Code),
                // Put analysis module inside execute to have access to global metadata at runtime
                new Execute(ctx => new AnalyzeCSharp()
                    .WhereNamespaces(ctx.GlobalMetadata.Get<bool>(DocsKeys.IncludeGlobal))
                    .WherePublic()
                    .WithCssClasses("pre", "prettyprint")
                    .WithWritePathPrefix(ctx.GlobalMetadata.String(DocsKeys.ApiPathPrefix))),
                new Razor.Razor()
                    .WithLayout("_ApiLayout.cshtml"),
                new WriteFiles()
            );
        }

        public void Scaffold(IDirectory directory)
        {
            throw new NotImplementedException();
        }
    }
}
