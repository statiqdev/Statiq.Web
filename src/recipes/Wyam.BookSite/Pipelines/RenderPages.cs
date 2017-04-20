using System;
using Wyam.Common.Configuration;
using Wyam.Common.Execution;
using Wyam.Common.Modules;
using Wyam.Core.Modules.Control;
using Wyam.Core.Modules.IO;

namespace Wyam.BookSite.Pipelines
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

        private static ModuleList GetModules() => new ModuleList
        {
            new Documents(BookSite.Pages),
            new Razor.Razor()
                .WithLayout("/_Layout.cshtml"),
            new WriteFiles()
        };
    }
}