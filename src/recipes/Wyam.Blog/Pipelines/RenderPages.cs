using System;
using Wyam.Common.Configuration;
using Wyam.Common.Execution;
using Wyam.Common.Modules;
using Wyam.Core.Modules.Control;
using Wyam.Core.Modules.IO;

namespace Wyam.Blog.Pipelines
{
    /// <summary>
    /// Renders and outputs the content pages using the template layouts.
    /// </summary>
    public class RenderPages : Pipeline
    {
        internal RenderPages()
            : base(GetModules())
        {
        }

        private static IModuleList GetModules() => new ModuleList
        {
            new Documents(Blog.Pages),
            new Razor.Razor()
                .WithLayout("/_Layout.cshtml"),
            new WriteFiles()
        };
    }
}