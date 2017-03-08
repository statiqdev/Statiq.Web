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
    /// Copies all other resources to the output path.
    /// </summary>
    public class Resources : Pipeline
    {
        internal Resources()
            : base(GetModules())
        {
        }

        private static ModuleList GetModules() => new ModuleList
        {
            new CopyFiles("**/*{!.cshtml,!.md,!.less,}")
        };
    }
}
