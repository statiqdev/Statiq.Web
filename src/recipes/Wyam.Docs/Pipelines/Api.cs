using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Wyam.CodeAnalysis;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Meta;
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

        private static IModuleList GetModules(ConcurrentDictionary<string, string> typeNamesToLink) => new ModuleList
        {
            new If(
                ctx => ctx.Documents[Docs.Code].Any()
                    || ctx.List<string>(DocsKeys.AssemblyFiles)?.Count > 0
                    || ctx.List<string>(DocsKeys.ProjectFiles)?.Count > 0
                    || ctx.List<string>(DocsKeys.SolutionFiles)?.Count > 0,
                new Documents(Docs.Code),
                new Execute(ctx => new AnalyzeCSharp() // Put analysis module inside execute to have access to global metadata at runtime
                    .WhereNamespaces(ctx.Bool(DocsKeys.IncludeGlobalNamespace))
                    .WherePublic()
                    .WithCssClasses("code", "cs")
                    .WithWritePathPrefix(ctx.DirectoryPath(DocsKeys.ApiPath))
                    .WithAssemblies(ctx.List<string>(DocsKeys.AssemblyFiles))
                    .WithProjects(ctx.List<string>(DocsKeys.ProjectFiles))
                    .WithSolutions(ctx.List<string>(DocsKeys.SolutionFiles))
                    .WithAssemblySymbols()
                    .WithImplicitInheritDoc(ctx.Bool(DocsKeys.ImplicitInheritDoc))),
                new Execute((doc, ctx) =>
                {
                    // Calculate a type name to link lookup for auto linking
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
                        typeNamesToLink.AddOrUpdate(WebUtility.HtmlEncode(name), ctx.GetLink(doc), (x, y) => string.Empty);
                    }
                }))
                .WithoutUnmatchedDocuments()
        };
    }
}
