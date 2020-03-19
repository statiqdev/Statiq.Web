using System;
using System.Collections.Generic;
using System.Text;
using Statiq.Common;
using Statiq.Core;
using Statiq.Html;
using Statiq.Markdown;
using Statiq.Razor;

namespace Statiq.Web.Modules
{
    /// <summary>
    /// Parent module that contains the modules used for processing full template languages like Razor.
    /// </summary>
    public class ProcessTemplates : ForAllDocuments
    {
        public ProcessTemplates()
            : base(
                new RenderRazor()
                    .WithLayout(Config.FromDocument((doc, ctx) =>
                    {
                        // Crawl up the tree looking for a layout
                        IDocument parent = doc;
                        while (parent != null)
                        {
                            if (parent.ContainsKey(WebKeys.Layout))
                            {
                                return parent.GetPath(WebKeys.Layout);
                            }
                            parent = parent.GetParent(ctx.Inputs);
                        }
                        return null;  // If no layout metadata, revert to default behavior
                    })),
                new ProcessShortcodes(),
                new ExecuteIf(Config.FromSetting<bool>(WebKeys.MirrorResources))
                {
                    new MirrorResources()
                })
        {
        }
    }
}
