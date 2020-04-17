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

namespace Statiq.Web.Modules
{
    /// <summary>
    /// Parent module that contains the modules used for processing full template languages like Razor.
    /// </summary>
    public class RenderPostProcessTemplates : ForAllDocuments
    {
        public RenderPostProcessTemplates(TemplateModules templateModules)
            : base(
                templateModules.PostProcessModules
                    .Where(x => x != null)
                    .Select(x => new ExecuteIf(x.Condition, x.Module))
                    .Concat(new IModule[]
                    {
                        new ProcessShortcodes(),
                        new ExecuteIf(Config.FromSetting<bool>(WebKeys.MirrorResources))
                        {
                            new MirrorResources()
                        }
                    })
                    .ToArray())
        {
        }
    }
}
