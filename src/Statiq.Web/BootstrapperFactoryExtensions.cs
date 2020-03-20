using System;
using System.Collections.Generic;
using System.Text;
using Statiq.App;
using Statiq.Common;
using Statiq.Web.Shortcodes;

namespace Statiq.Web
{
    public static class BootstrapperFactoryExtensions
    {
        public static Bootstrapper CreateWeb(this BootstrapperFactory factory, string[] args) =>
            factory
                .CreateDefault(args)
                .AddPipelines(typeof(BootstrapperFactoryExtensions).Assembly)
                .AddSettingsIfNonExisting(new Dictionary<string, object>
                {
                    { WebKeys.MirrorResources, true }
                })
                .AddShortcode<ChildPages>();
    }
}
