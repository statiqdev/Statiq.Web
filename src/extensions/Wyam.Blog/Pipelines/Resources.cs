using System;
using Wyam.Common.Configuration;
using Wyam.Common.Modules;
using Wyam.Core.Modules.IO;

namespace Wyam.Blog.Pipelines
{
    /// <summary>
    /// Copies all other resources to the output path.
    /// </summary>
    public class Resources : RecipePipeline
    {
        /// <inheritdoc />
        public override string Name => nameof(Blog.Resources);

        /// <inheritdoc />
        public override ModuleList GetModules() => new ModuleList
        {
            new CopyFiles("**/*{!.cshtml,!.md,}")
        };
    }
}