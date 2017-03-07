using System;
using Wyam.Common.Configuration;
using Wyam.Common.Modules;
using Wyam.Core.Modules.Control;
using Wyam.Core.Modules.IO;

namespace Wyam.Blog.Pipelines
{
    /// <summary>
    /// Renders and outputs the content pages using the template layouts.
    /// </summary>
    public class RenderPages : RecipePipeline
    {
        /// <inheritdoc />
        public override string Name => nameof(Blog.RenderPages);

        /// <inheritdoc />
        public override ModuleList GetModules() => new ModuleList
        {
            new Documents(BlogPipelines.Pages),
            new Razor.Razor()
                .WithLayout("/_Layout.cshtml"),
            new WriteFiles()
        };
    }
}