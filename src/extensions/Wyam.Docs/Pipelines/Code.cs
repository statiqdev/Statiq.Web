using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.CodeAnalysis;
using Wyam.Common.Execution;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Util;
using Wyam.Core.Modules.Control;
using Wyam.Core.Modules.IO;

namespace Wyam.Docs.Pipelines
{
    /// <summary>
    /// Loads source files.
    /// </summary>
    public class Code : Pipeline
    {
        internal Code()
            : base(GetModules())
        {
        }

        private static ModuleList GetModules() => new ModuleList
        {
            new ReadFiles(ctx => ctx.List<string>(DocsKeys.SourceFiles)),
            new ConcatBranch(
                new Where((doc, _) => doc.String(Keys.SourceFileExt) == ".csproj"),
                new ReadProject((doc, _) => doc.FilePath(Keys.SourceFilePath))),
            new ConcatBranch(
                new Where((doc, _) => doc.String(Keys.SourceFileExt) == ".sln"),
                new ReadSolution((doc, _) => doc.FilePath(Keys.SourceFilePath))),
            new Where((doc, _) => doc.String(Keys.SourceFileExt) == ".cs")
        };
    }
}
