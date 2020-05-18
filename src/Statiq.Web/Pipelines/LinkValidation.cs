using System.Linq;
using Statiq.Common;
using Statiq.Core;
using Statiq.Html;

namespace Statiq.Web.Pipelines
{
    public class LinkValidation : Pipeline
    {
        public LinkValidation()
        {
            Deployment = true;

            ExecutionPolicy = ExecutionPolicy.Normal;

            Dependencies.AddRange(nameof(Content), nameof(Archives));

            OutputModules = new ModuleList
            {
                new ReplaceDocuments(Dependencies.ToArray()),
                new ValidateLinks()
                    .ValidateRelativeLinks(Config.FromSetting(WebKeys.ValidateRelativeLinks, true))
                    .ValidateAbsoluteLinks(Config.FromSetting<bool>(WebKeys.ValidateAbsoluteLinks))
                    .AsError(Config.FromSetting<bool>(WebKeys.ValidateLinksAsError))
            };
        }
    }
}
