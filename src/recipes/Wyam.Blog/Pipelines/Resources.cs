using System;
using Wyam.Common.Configuration;
using Wyam.Common.Execution;
using Wyam.Common.Modules;
using Wyam.Core.Modules.IO;

namespace Wyam.Blog.Pipelines
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

        private static IModuleList GetModules() => new ModuleList
        {
            new CopyFiles("**/*{!.cshtml,!.md,}")
        };
    }
}