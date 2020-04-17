using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Statiq.Common;
using Statiq.Core;
using Statiq.Html;
using Statiq.Markdown;
using Statiq.Razor;

namespace Statiq.Web.Modules
{
    /// <summary>
    /// Parent module that contains the modules used for processing markup languages like Markdown.
    /// </summary>
    public class RenderProcessTemplates : ForAllDocuments
    {
        public RenderProcessTemplates(TemplateModules templateModules)
            : base(
                new IModule[]
                {
                    new ProcessShortcodes("!")
                }
                .Concat(templateModules.ProcessModules
                    .Where(x => x != null)
                    .Select(x => new ExecuteIf(x.Condition, x.Module)))
                .ToArray())
        {
        }
    }
}
