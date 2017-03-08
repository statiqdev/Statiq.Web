using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Execution;
using Wyam.Common.Modules;
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
            new ReadFiles(ctx => ctx.List<string>(DocsKeys.SourceFiles))
        };
    }
}
