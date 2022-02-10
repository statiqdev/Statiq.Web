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
    public class RenderContentProcessTemplates : ForAllDocuments
    {
        public RenderContentProcessTemplates(Templates templates)
            : base(
                new ProcessShortcodes("!"),
                templates.GetModule(ContentType.Content, Phase.Process),
                new ProcessShortcodes("^"))
        {
        }
    }
}