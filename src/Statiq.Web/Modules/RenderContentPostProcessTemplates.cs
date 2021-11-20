using System.Linq;
using Statiq.Common;
using Statiq.Core;

namespace Statiq.Web.Modules
{
    /// <summary>
    /// Renders the content templates as well as related modules like shortcodes.
    /// </summary>
    /// <remarks>
    /// The content templates are wrapped by a parent module since they're applied from
    /// multiple places so that the related modules are also executed when needed.
    /// </remarks>
    public class RenderContentPostProcessTemplates : ForAllDocuments
    {
        public RenderContentPostProcessTemplates(Templates templates)
            : base(
                new IModule[]
                {
                    new ExecuteIf(
                        Config.FromDocument(WebKeys.RenderPostProcessTemplates, true),
                        templates.GetModule(ContentType.Content, Phase.PostProcess))
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