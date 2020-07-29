using System.Linq;
using Statiq.Common;
using Statiq.Core;
using Statiq.Html;

namespace Statiq.Web.Modules
{
    /// <summary>
    /// Parent module that contains the modules used for processing full template languages like Razor.
    /// </summary>
    public class RenderPostProcessTemplates : ForAllDocuments
    {
        public RenderPostProcessTemplates(Templates templates)
            : base(
                new IModule[]
                {
                    new ExecuteIf(
                        Config.FromDocument(WebKeys.RenderPostProcessTemplates, true),
                        templates.GetModules(Phase.PostProcess))
                }
                .Concat(new IModule[]
                {
                    new ProcessShortcodes(),
                    new ExecuteIf(Config.FromSetting<bool>(WebKeys.MirrorResources))
                    {
                        new MirrorResources()
                    },
                    new ResolveXrefs(),
                    new ExecuteIf(Config.FromSetting<bool>(WebKeys.MakeLinksAbsolute))
                    {
                        new MakeLinksAbsolute()
                    },
                    new ExecuteIf(Config.FromSetting<bool>(WebKeys.MakeLinksRootRelative))
                    {
                        new MakeLinksRootRelative()
                    }
                })
                .ToArray())
        {
        }
    }
}
