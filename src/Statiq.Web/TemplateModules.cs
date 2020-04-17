using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Statiq.Common;
using Statiq.Core;
using Statiq.Html;
using Statiq.Markdown;
using Statiq.Razor;

namespace Statiq.Web
{
    public class TemplateModules
    {
        public const string Markdown = nameof(Markdown);
        public const string Razor = nameof(Razor);
        public const string Handlebars = nameof(Handlebars);

        public List<TemplateModule> ProcessModules { get; } = new List<TemplateModule>
        {
            new TemplateModule(Markdown, MediaTypes.Markdown, new RenderMarkdown().UseExtensions())
        };

        public List<TemplateModule> PostProcessModules { get; } = new List<TemplateModule>()
        {
            new TemplateModule(
                Razor,
                true,
                new RenderRazor()
                    .WithLayout(Config.FromDocument((doc, ctx) =>
                    {
                        // Crawl up the tree looking for a layout
                        // TODO: Layout doesn't appear to be working, need to run that down
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
                    })))
        };
    }
}
