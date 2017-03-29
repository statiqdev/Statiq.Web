using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.CodeAnalysis;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Modules;
using Wyam.Common.Util;
using Wyam.Core.Modules.Control;
using Wyam.Core.Modules.Extensibility;

namespace Wyam.Docs.Pipelines
{
    /// <summary>
    /// Uses Roslyn to analyze any source files loaded in the previous
    /// pipeline along with any specified assemblies. This pipeline
    /// results in documents that represent Roslyn symbols.
    /// </summary>
    public class Api : Pipeline
    {
        internal Api(ConcurrentDictionary<string, string> typeNamesToLink)
            : base(GetModules(typeNamesToLink))
        {
        }

        private static ModuleList GetModules(ConcurrentDictionary<string, string> typeNamesToLink) => new ModuleList
        {
            new If(ctx => ctx.Documents[Docs.Code].Any() || ctx.List<string>(DocsKeys.AssemblyFiles)?.Count > 0,
                new Documents(Docs.Code),
                // Put analysis module inside execute to have access to global metadata at runtime
                new Execute(ctx => new AnalyzeCSharp()
                    .WhereNamespaces(ctx.Bool(DocsKeys.IncludeGlobalNamespace))
                    .WherePublic()
                    .WithCssClasses("code", "cs")
                    .WithWritePathPrefix("api")
                    .WithAssemblies(ctx.List<string>(DocsKeys.AssemblyFiles))
                    .WithAssemblySymbols()),
                // Calculate a type name to link lookup for auto linking
                new Execute((doc, ctx) =>
                {
                    string name = null;
                    string kind = doc.String(CodeAnalysisKeys.Kind);
                    if (kind == "NamedType")
                    {
                        name = doc.String(CodeAnalysisKeys.DisplayName);
                    }
                    else if (kind == "Property" || kind == "Method")
                    {
                        IDocument containingType = doc.Document(CodeAnalysisKeys.ContainingType);
                        if (containingType != null)
                        {
                            name = $"{containingType.String(CodeAnalysisKeys.DisplayName)}.{doc.String(CodeAnalysisKeys.DisplayName)}";
                        }
                    }
                    if (name != null)
                    {
                        typeNamesToLink.AddOrUpdate(name, ctx.GetLink(doc), (x, y) => string.Empty);
                    }
                })
            )
        };
    }
}
