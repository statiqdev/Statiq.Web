using System.Linq;
using Statiq.Common;
using Statiq.Core;

namespace Statiq.Web.Modules
{
    /// <summary>
    /// Parent module that contains the modules used for processing markup languages like Markdown.
    /// </summary>
    public class RenderProcessTemplates : ForAllDocuments
    {
        public RenderProcessTemplates(Templates templates)
            : base(
                new IModule[]
                {
                    new ProcessShortcodes("!")
                }
                .Concat(templates.GetModules(Phase.Process))
                .ToArray())
        {
        }
    }
}
