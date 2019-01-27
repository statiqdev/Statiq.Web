using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Execution;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Core.Modules.Control;
using Wyam.Core.Modules.IO;

namespace Wyam.Blog.Pipelines
{
    /// <summary>
    /// Generates the tag index.
    /// </summary>
    public class TagIndex : Pipeline
    {
        internal TagIndex()
            : base(GetModules())
        {
        }

        private static IModuleList GetModules() => new ModuleList
        {
            new If(
                ctx => ctx.Documents[Blog.Tags].Any(),
                new ReadFiles("_Tags.cshtml"),
                new FrontMatter(
                    new Yaml.Yaml()),
                new Razor.Razor()
                    .IgnorePrefix(null)
                    .WithLayout("/_Layout.cshtml"),
                new WriteFiles((doc, ctx) => "tags/index.html"))
            .WithoutUnmatchedDocuments()
        };
    }
}
