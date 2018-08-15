using Wyam.Common.Execution;
using Wyam.Common.Modules;
using Wyam.Core.Modules.Control;
using Wyam.Core.Modules.IO;

namespace Wyam.Web.Pipelines
{
    /// <summary>
    /// generate sitemap
    /// </summary>
    public class Sitemap : Pipeline
    {
        /// <summary>
        /// Creates the pipeline.
        /// </summary>
        /// <param name="name">The name of this pipeline.</param>
        public Sitemap(string name)
            : base(name, GetModules())
        {
        }

        private static IModuleList GetModules() => new ModuleList
        {
            new Documents(ctx => ctx.Documents),
            new Core.Modules.Contents.Sitemap(),
            new WriteFiles((doc, ctx) => "sitemap.xml")
        };
    }
}
